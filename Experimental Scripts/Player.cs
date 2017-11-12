using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;


public class Player : MonoBehaviour {


    // ----- EDITABLE VARIABLES -----

    // GENERAL MOVEMENT VARIABLES
    [SerializeField] float horizontalMoveSpeed = 10f;
    [SerializeField] float verticalMoveSpeed = 10f;
    [SerializeField] float backwardsMovementDivisor = 2f;

    // JUMP VARIABLES
    [SerializeField] float minJumpForce = 4f;
    [SerializeField] float maxJumpForce = 8f;
    [SerializeField] float jumpChargeRate = 6f;
    [SerializeField] float chargeJumpSpeedDivisor = 3f;

    // GLIDING VARIABLES
    [SerializeField] float backGlideDrag = 10f;
    [SerializeField] float frontGlideDrag = 5f;
    [SerializeField] float sideGlideDrag = 5f;

    // DODGE VARIABLES
    [SerializeField] float dodgeStateTime = 1f;
    [SerializeField] float dodgeSpeedMultiplier = 4f;

    // PROJECTILE ATTACK VARIABLES
    [SerializeField] float projectileVerticalOffset = 1.7f;
    [SerializeField] float projectileHorizontalOffset = 1.5f;
    [SerializeField] float initialProjectileForce = 30f;
    [SerializeField] float timeBetweenShots = 0.5f;
    [SerializeField] GameObject projectile;


    // ----- PRIVATE VARIABLES -----

    // EXTERNAL REFERENCES
    public Animator animator;
    private Rigidbody rb;
    private Transform cameraTransform;

    // INPUT AXIS
    private float horizontalInput;
    private float verticalInput;

    // STATE BOOL
    private bool inChargeJumpState;
    private bool inDodgeState;
    private bool inAerialDodgeState;
    private bool isGrounded;

    // STATE FLOAT
    private float timeUntilNextShot;
    private float currentJumpForce;

    // STATE VECTOR
    private Vector3 moveVector;


    // ----- MAIN FLOW ----- 

    void Start () {

        // Link external components
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        cameraTransform = Camera.main.transform;

        // Initialize state variables
        inDodgeState = false;
        inChargeJumpState = false;
        inAerialDodgeState = false;
        isGrounded = false;

        currentJumpForce = minJumpForce;
        timeUntilNextShot = 0;
        
    }

    private void Update()
    {
        CheckIfGrounded();
        DecrementShootingCooldown();

        // Read input
        if (Input.GetKey(KeyCode.Space) && isGrounded && !inDodgeState)                                         { ChargeJump(); }
        if (Input.GetKeyUp(KeyCode.Space) && isGrounded)                                                        { ExecuteJump(); }
        if (Input.GetKey(KeyCode.Mouse0) && timeUntilNextShot <= 0 && !inChargeJumpState && !inDodgeState)      { ShootProjectile(); }
        if (Input.GetKeyDown(KeyCode.LeftShift) && !inChargeJumpState)                                          { EnterDodgeState(); }
        ReadMovementInput();

        // Adjust movement
        UpdateAerialDrag();
        CalculateMoveVector();

        // Move and rotate the player
        MovePlayer();
        RotatePlayer();

        // Update animator
        UpdateAnimator();
    }


    // ----- MOVE HELPER FUNCTIONS -----

    private void ReadMovementInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
    }

    private void CalculateMoveVector()
    {
        // Calculate horizontal move vector component
        float horizontalMagnitude = horizontalInput * Time.deltaTime * horizontalMoveSpeed;
        Vector3 playerRightDirection = Vector3.Scale(transform.right, new Vector3(1, 0, 1)).normalized;

        // Calculate vertical move vector component
        float verticalMagnitude = verticalInput * Time.deltaTime * verticalMoveSpeed;
        Vector3 playerForwardDirection = Vector3.Scale(transform.forward, new Vector3(1, 0, 1)).normalized;

        // Calculate resultant vector
        moveVector = verticalMagnitude * playerForwardDirection + horizontalMagnitude * playerRightDirection;
        moveVector = Vector3.ClampMagnitude(moveVector, verticalMoveSpeed);                                     // Ensure diagonal speed is capped at forward speed

        // Adjust move vector for special cases
        if (verticalInput < 0 && !inDodgeState) moveVector = moveVector / backwardsMovementDivisor;             // Reduce backwards movement
        if (inDodgeState) { moveVector = moveVector * dodgeSpeedMultiplier; }                                   // Increase movement speed if dodging
        if (inChargeJumpState) { moveVector = moveVector / chargeJumpSpeedDivisor; }                            // Reduce movement speed if charging a jump
    }

    // Increase drag if descending and not dodging -- to emulate gliding
    private void UpdateAerialDrag()
    {
        rb.drag = 0;

        if (!isGrounded && rb.velocity.y < 0 && !inDodgeState)
        {
            if (verticalInput == 1) { rb.drag = frontGlideDrag; }                                       // forward glide
            else if (verticalInput == -1) { rb.drag = backGlideDrag; }                                  // backward glide
            else if ((horizontalInput == -1 || horizontalInput == 1)) { rb.drag = sideGlideDrag; }      // side glide
        }
    }

    private void MovePlayer()
    {
        transform.Translate(moveVector, Space.World);   // Move the transform relative to worldspace
    }

    private void RotatePlayer()
    {
        // rotate player to match camera free look rotation
        Quaternion cameraRotation = Quaternion.LookRotation(cameraTransform.forward, cameraTransform.up);
        transform.rotation = cameraRotation;
    }


    // ----- DODGE HELPER FUNCTIONS -----

    private void EnterDodgeState()
    {
        inDodgeState = true;
        Invoke("ExitDodgeState", dodgeStateTime);
    }

    private void ExitDodgeState()
    {
        inDodgeState = false;
    }


    // ----- JUMP HELPER FUNCTIONS -----

    private void EnterChargeJumpState()
    {
        inChargeJumpState = true;
    }

    private void ExitChargeJumpState()
    {
        inChargeJumpState = false;
    }

    private void ChargeJump()
    {
        if (inChargeJumpState == false) EnterChargeJumpState();
        currentJumpForce += Time.deltaTime * jumpChargeRate;

    }

    private void ExecuteJump()
    {
        currentJumpForce = Mathf.Clamp(currentJumpForce, minJumpForce, maxJumpForce);   // ensure jump force is within bounds
        rb.AddForce(0, currentJumpForce, 0, ForceMode.Impulse);                         // execute jump
        currentJumpForce = minJumpForce;                                                // reset stored jump force
        ExitChargeJumpState();
    }


    // ----- PROJECTILE ATTACK HELPER FUNCTIONS -----

    private void ShootProjectile()
    {
        // Define projectile spawn point relative to character
        Vector3 projectileSpawnPoint = transform.position + (transform.forward.normalized * projectileHorizontalOffset) + transform.up * projectileVerticalOffset;

        // Create projectile
        GameObject clone = Instantiate(projectile, projectileSpawnPoint, Quaternion.FromToRotation(Vector3.up, transform.forward)) as GameObject;

        // Shoot projectile
        Rigidbody rb = clone.GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * initialProjectileForce, ForceMode.Impulse);

        // Reset shot recharge countdown
        timeUntilNextShot = timeBetweenShots;
    }

    private void DecrementShootingCooldown()
    {
        timeUntilNextShot -= Time.deltaTime;
    }
    

    // ----- ANIMATOR HELPER FUNCTIONS -----
    private void UpdateAnimator()
    {
        animator.SetBool("inDodgeState", inDodgeState);
        animator.SetBool("inChargeJumpState", inChargeJumpState);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("horizontalInput", horizontalInput);
        animator.SetFloat("verticalInput", verticalInput);
    }


    // ----- GENERAL HELPER FUNCTIONS -----
    private bool CheckIfGrounded()
    {
        // if raycast detects something very close below, return true and change state
        if (Physics.Raycast(transform.position, Vector3.down, 0.5f))
        {
            isGrounded = true;
            return true;
        }
        else
        {
            isGrounded = false;
            return false;
        }
    }



}