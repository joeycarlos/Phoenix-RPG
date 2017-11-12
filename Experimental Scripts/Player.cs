using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;


public class Player : MonoBehaviour {

    [SerializeField] float horizontalMoveSpeed = 10f;
    [SerializeField] float verticalMoveSpeed = 10f;
    [SerializeField] float minJumpForce = 4f;
    [SerializeField] float maxJumpForce = 8f;
    [SerializeField] float jumpChargeRate = 6f;
    [SerializeField] float groundedDodgeLift = 3f;
    [SerializeField] float aerialDodgeLift = 10f;
    [SerializeField] float dodgeHorizontalMultiplier = 30f;
    [SerializeField] float forwardGlideMultiplier = 3f;
    [SerializeField] float backGlideDrag = 10f;
    [SerializeField] float frontGlideDrag = 5f;
    [SerializeField] float sideGlideDrag = 5f;
    [SerializeField] float dodgeStateTime = 1f;
    [SerializeField] float projectileVerticalOffset = 1.7f;
    [SerializeField] float projectileHorizontalOffset = 1.5f;
    [SerializeField] float initialProjectileForce = 30f;
    [SerializeField] float timeBetweenShots = 0.5f;
    [SerializeField] float backwardsMovementDivisor = 2f;
    [SerializeField] private float dodgeSpeedMultiplier = 4f;
    [SerializeField] private float chargeJumpSpeedDivisor = 3f;

    [SerializeField] GameObject projectile;

    public Animator animator;
    private float horizontalInput;
    private float verticalInput;
    private bool inAir;
    private bool inDodgeState;
    private bool inChargeJumpState;



    private Transform cameraTransform;                  // A reference to the main camera in the scenes transform
    private Vector3 moveVector;
    private Rigidbody rb;

    private float currentJumpForce;
    private Vector3 dodgeForceVector;

    private bool inAerialDodgeState;

    private float timeUntilNextShot;

    void Start () {
        cameraTransform = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
        currentJumpForce = minJumpForce;
        inAerialDodgeState = false;
        inChargeJumpState = false;
        timeUntilNextShot = 0;
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        timeUntilNextShot -= Time.deltaTime;

        if (Input.GetKey(KeyCode.Space) && isGrounded() && currentJumpForce < maxJumpForce) { ChargeJump(); }

        if (Input.GetKeyUp(KeyCode.Space) && isGrounded()) { ExecuteJump(); }

        if (Input.GetKey(KeyCode.Mouse0) && timeUntilNextShot <= 0)
        {
            ShootProjectile();
            timeUntilNextShot = timeBetweenShots;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift)) { ExecuteDodge(); }

        MovePlayer();
        RotatePlayer();
    }

    private void ShootProjectile()
    {
        Vector3 projectileSpawnPoint = transform.position + (transform.forward.normalized * projectileHorizontalOffset) + transform.up * projectileVerticalOffset;
        GameObject clone = Instantiate(projectile, projectileSpawnPoint, Quaternion.FromToRotation(Vector3.up, transform.forward)) as GameObject;
        Rigidbody rb = clone.GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * initialProjectileForce, ForceMode.Impulse);
    }

    private void ExecuteDodge()
    {
        inDodgeState = true;
        Invoke("SetAnimDodgeStateFalse", dodgeStateTime);
        animator.SetBool("inDodgeState", inDodgeState);
    }

    private void SetAnimDodgeStateFalse()
    {
        inDodgeState = false;
        animator.SetBool("inDodgeState", inDodgeState);
    }

    private void SetDodgeStateFalse()
    {
        inAerialDodgeState = false;
    }

    private void ChargeJump()
    {
        currentJumpForce += Time.deltaTime * jumpChargeRate;
        inChargeJumpState = true;
        animator.SetBool("inChargeJumpState", inChargeJumpState);
    }

    private void ExecuteJump()
    {
        currentJumpForce = Mathf.Clamp(currentJumpForce, minJumpForce, maxJumpForce);
        rb.AddForce(0, currentJumpForce, 0, ForceMode.Impulse);
        currentJumpForce = minJumpForce;
        inChargeJumpState = false;
        animator.SetBool("inChargeJumpState", inChargeJumpState);
    }

    // move player according to worldspace
    private void MovePlayer()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        animator.SetFloat("horizontalInput", horizontalInput);
        animator.SetFloat("verticalInput", verticalInput);

        rb.drag = 0;

        if (!isGrounded())
        {

            if (verticalInput == 1 && rb.velocity.y < 0 && !inAerialDodgeState)
            {
                rb.drag = frontGlideDrag;
            }
                
            else if (Input.GetKey(KeyCode.S) && rb.velocity.y < 0 && !inAerialDodgeState)
            {
                rb.drag = backGlideDrag;
            }

            if ( (horizontalInput == -1 || horizontalInput == 1) && rb.velocity.y < 0 && !inAerialDodgeState)
            {
                rb.drag = sideGlideDrag;
            }
        }

        // slow backwards movement
        if (verticalInput < 0 && !inDodgeState) verticalInput = verticalInput / backwardsMovementDivisor;

        horizontalInput = horizontalInput * Time.deltaTime * horizontalMoveSpeed;
        Vector3 playerRight = Vector3.Scale(transform.right, new Vector3(1, 0, 1)).normalized;

        verticalInput = verticalInput * Time.deltaTime * verticalMoveSpeed;
        Vector3 playerForward = Vector3.Scale(transform.forward, new Vector3(1, 0, 1)).normalized;

        moveVector = verticalInput * playerForward + horizontalInput * playerRight;

        moveVector = Vector3.ClampMagnitude(moveVector, verticalMoveSpeed);

        if (inDodgeState)
        {
            moveVector = moveVector * dodgeSpeedMultiplier;
        }

        if (verticalInput >= 0 && !isGrounded() && !inDodgeState)
        {
            verticalInput = verticalInput * Time.deltaTime * verticalMoveSpeed * forwardGlideMultiplier;
        }

        if (inChargeJumpState)
        {
            moveVector = moveVector / chargeJumpSpeedDivisor;
        }

        transform.Translate(moveVector, Space.World);
    }

    // rotate player to match camera free look rotation
    private void RotatePlayer()
    {
        Quaternion rotation = Quaternion.LookRotation(cameraTransform.forward, cameraTransform.up);
        transform.rotation = rotation;
    }

    // if raycast detects something very close below, return true
    private bool isGrounded()
    {
        if (Physics.Raycast(transform.position, Vector3.down, 0.5f))
        {
            inAir = false;
            animator.SetBool("inAir", inAir);
            return true;
        }

        else
            inAir = true;
        animator.SetBool("inAir", inAir);
        return false;
    }

}
