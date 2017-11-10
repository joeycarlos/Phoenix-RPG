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

    private Transform cameraTransform;                  // A reference to the main camera in the scenes transform
    private Vector3 cameraDirection;             // The current forward direction of the camera
    private Vector3 moveVector;
    private Rigidbody rb;

    private float currentJumpForce;
    private Vector3 dodgeForceVector;

    private bool inAerialDodgeState;

    void Start () {
        cameraTransform = Camera.main.transform;
        print(cameraTransform);
        rb = GetComponent<Rigidbody>();
        currentJumpForce = minJumpForce;
        inAerialDodgeState = false;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space) && isGrounded() && currentJumpForce < maxJumpForce) { ChargeJump(); }

        if (Input.GetKeyUp(KeyCode.Space) && isGrounded()) { ExecuteJump(); }

        if (Input.GetKeyDown(KeyCode.LeftShift)) { ExecuteDodge(); }
    }

    private void FixedUpdate()
    {
        MovePlayer();
        RotatePlayer();
    }

    private void ExecuteDodge()
    {
        // if not moving, default to backwards dodge
        if (moveVector.magnitude == 0)
            dodgeForceVector = -transform.forward;
        else
            dodgeForceVector = Vector3.Scale(Vector3.Normalize(moveVector), (new Vector3(1, 0, 1)).normalized);

        dodgeForceVector = Vector3.Scale(dodgeForceVector, new Vector3(dodgeHorizontalMultiplier, 0, dodgeHorizontalMultiplier));

        if (isGrounded())
        {
            dodgeForceVector += new Vector3(0, groundedDodgeLift, 0);
        } else
        {
            dodgeForceVector += new Vector3(0, aerialDodgeLift, 0);
            inAerialDodgeState = true;
            Invoke("SetDodgeStateFalse", dodgeStateTime);
        }

        rb.AddForce(dodgeForceVector, ForceMode.Impulse);
    }

    private void SetDodgeStateFalse()
    {
        inAerialDodgeState = false;
    }

    private void ChargeJump()
    {
        currentJumpForce += Time.deltaTime * jumpChargeRate;
    }

    private void ExecuteJump()
    {
        currentJumpForce = Mathf.Clamp(currentJumpForce, minJumpForce, maxJumpForce);
        rb.AddForce(0, currentJumpForce, 0, ForceMode.Impulse);
        currentJumpForce = minJumpForce;
    }

    // move player according to worldspace
    private void MovePlayer()
    {
        float horizontalInput = 0;
        float verticalInput = 0;
        rb.drag = 0;

        if (isGrounded())
        {
            if (Input.GetKey(KeyCode.W)) verticalInput = 1;
            if (Input.GetKey(KeyCode.S)) verticalInput = -1;
            if (Input.GetKey(KeyCode.A)) horizontalInput = -1;
            if (Input.GetKey(KeyCode.D)) horizontalInput = 1;

        }
        else
        {
            if (Input.GetKey(KeyCode.W) )
            {
                verticalInput = forwardGlideMultiplier;
                if (rb.velocity.y < 0 && !inAerialDodgeState) rb.drag = frontGlideDrag;

            }
            if (Input.GetKey(KeyCode.S) && rb.velocity.y < 0 && !inAerialDodgeState)
            {
                rb.drag = backGlideDrag;
            }
            if (Input.GetKey(KeyCode.A))
            {
                horizontalInput = -1;
                if (rb.velocity.y < 0 && !inAerialDodgeState) rb.drag = sideGlideDrag;
            }
            if (Input.GetKey(KeyCode.D))
            {
                horizontalInput = 1;
                if (rb.velocity.y < 0 && !inAerialDodgeState) rb.drag = sideGlideDrag;
            }
        }

        horizontalInput = horizontalInput * Time.deltaTime * horizontalMoveSpeed;
        Vector3 playerRight = Vector3.Scale(transform.right, new Vector3(1, 0, 1)).normalized;

        verticalInput = verticalInput * Time.deltaTime * verticalMoveSpeed;
        Vector3 playerForward = Vector3.Scale(transform.forward, new Vector3(1, 0, 1)).normalized;

        moveVector = verticalInput * playerForward + horizontalInput * playerRight;
        transform.Translate(moveVector, Space.World);
    }

    // rotate player to match camera free look rotation
    private void RotatePlayer()
    {
        cameraDirection = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
        Quaternion rotation = Quaternion.LookRotation(cameraTransform.forward, cameraTransform.up);
        transform.rotation = rotation;
    }

    // if raycast detects something very close below, return true
    private bool isGrounded()
    {
        if (Physics.Raycast(transform.position, Vector3.down, 0.5f))
            return true;
        else
            return false;
    }

}
