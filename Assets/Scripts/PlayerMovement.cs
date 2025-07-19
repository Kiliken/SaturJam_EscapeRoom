using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements.Experimental;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] GameObject damageParticle;

    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float speedModifier = 0f;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplayer;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchingSpeed;
    public float crouchYScale;
    private float startYScale;
    bool crouching;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask groundLayer;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;


    [Space(10)]
    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;


    public MovementState state;
    public enum MovementState {
        walking,
        sprinting,
        crouching,
        air
    }
    [HideInInspector] public bool playerActive = true;

    // melee
    private bool canMelee = true;
    [SerializeField] float meleeDamage = 1f;
    [SerializeField] float meleeRange = 1f;
    [SerializeField] float meleeCD = 3f;
    private float meleeCDTimer = 0f;


    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;

        startYScale = transform.localScale.y;
        crouching = false;
    }


    private void MyInput() {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetAxisRaw("Jump")!=0 && readyToJump && grounded) {
            readyToJump = false;
            //Debug.Log("jump");
            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetAxisRaw("Crouch") != 0) {
            if (crouching == false)
            {
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

                crouching = true;
            }
        }

        if (Input.GetAxisRaw("Crouch") == 0) {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            crouching = false;
        }
    }


    private void StateHandler() {
        if (Input.GetAxisRaw("Crouch") != 0) {
            state = MovementState.crouching;
            moveSpeed = crouchingSpeed;
        
        }
        else if (grounded && Input.GetAxisRaw("Sprint") != 0)
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed + speedModifier;
        }

        else if (grounded) {
            state = MovementState.walking;
            moveSpeed = walkSpeed + speedModifier;
        }

        else
        {
            state = MovementState.air;
        }
    }


    private void MovePlayer() {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope()) {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplayer, ForceMode.Force);
    
        rb.useGravity = !OnSlope();
    }


    // Update is called once per frame
    void Update()
    {
        if(playerActive){
            grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer);
        
            MyInput();
            SpeedControl();
            StateHandler();

            if (grounded)
            {
                rb.drag = groundDrag;
            }
            else
            {
                rb.drag = 0;
            }

            if(!canMelee){
                if(meleeCDTimer < meleeCD){
                    meleeCDTimer += Time.deltaTime;
                }
                else{
                    canMelee = true;
                    meleeCDTimer = 0;
                }
            }
        }
    }


    private void FixedUpdate()
    {
        if(playerActive){
            MovePlayer();
        }
    }

    private void SpeedControl() {

        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }

        else {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }


    private void Jump() {
        exitingSlope = true;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }


    private void ResetJump()
    {
        exitingSlope = false;
        readyToJump = true;
    }


    private bool OnSlope() {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f)) {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }


    private Vector3 GetSlopeMoveDirection() { 
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}
