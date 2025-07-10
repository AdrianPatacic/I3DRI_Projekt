using System;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;
    [SerializeField] private Slider staminaBar;
    [SerializeField] private Camera mainCamera;

    private float currentSpeed;
    [SerializeField] private float crouchSpeed = 2f;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;

    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;

    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 25f;
    [SerializeField] private float staminaRegenRate = 15f;
    [SerializeField] private float currentStamina;

    [SerializeField] private bool isCrouching = false;
    [SerializeField] private bool isMoving;
    [SerializeField] private bool isSprinting;
    [SerializeField] private Vector3 velocity;
    [SerializeField] private bool isGrounded;

    [SerializeField] private bool lockedOn;
    public bool lockedOnPublic => lockedOn;

    [SerializeField] private GameObject lockedOnGameObject;
    public GameObject lockedOnGameObjectPublic => lockedOnGameObject;

    private enum StateEnum { idle, walking, sprinting, emote, crouching, crouchWalk }
    private StateEnum state = StateEnum.idle;
    private Vector3 lastPosition;



    void Start()
    {
        characterController = GetComponent<CharacterController>();
        currentStamina = maxStamina;
        
        if (staminaBar != null)
            staminaBar.maxValue = maxStamina;
        lastPosition = transform.position;
    }

    
    void Update()
    { 

        #region Grounding
        // isGrounded Check - has to be before characterControler.Move() because .move affects .isGrounded
        isGrounded = characterController.isGrounded;

        // stick the player to the ground and cap the -y
        if(isGrounded && characterController.velocity.y < 0)
        {
            velocity.y = -2f;
        }
        #endregion

        bool lockOnKey = Input.GetKeyDown(KeyCode.E);

        if(lockOnKey && lockedOn)
        {
            lockedOn = false;
        }else if(lockOnKey && !lockedOn)
        {
            lockedOn = true;
        }

            #region First Person Movement

            /*
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 move = transform.right * x + transform.forward * z;
            characterController.Move(move * currentSpeed * Time.deltaTime);

            Vector3 velocityCheck;
            velocityCheck = (transform.position - lastPosition) / Time.deltaTime;
            lastPosition = transform.position;

            isMoving = velocityCheck.magnitude > 0f;

            bool crouchKey = Input.GetKeyDown(KeyCode.LeftControl);

            if (crouchKey && !isCrouching)
            {
                isCrouching = true;
            }else if (crouchKey && isCrouching)
            {
                isCrouching = false;
            }
            */

            #endregion

            #region Third Person Movement

        //Get movement Inputs
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        //Get Camera forward and right
        Vector3 camForward = mainCamera.transform.forward;
        Vector3 camRight = mainCamera.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // Move player in direction of move input
        Vector3 move = camForward * z + camRight * x;
        characterController.Move(move.normalized * currentSpeed * Time.deltaTime);

        // Rotate player based on move input and camera rotation or based on lockedOnGameObject
        Vector3 moveDir = new Vector3(x, 0f, z);
        if (lockedOn)
        {
            Vector3 direction = (lockedOnGameObject.transform.position - transform.position);
            direction.y = 0f;
            if (direction != Vector3.zero)
            {
                Quaternion rot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 10f);
            }
        }
        else if(moveDir != Vector3.zero)
        {
            Vector3 worldDir = mainCamera.transform.TransformDirection(moveDir);
            worldDir.y = 0f;
            Quaternion rot = Quaternion.LookRotation(worldDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 10f);
        }


        // isMoving Check
        Vector3 velocityCheck;
        velocityCheck = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
        isMoving = velocityCheck.magnitude > 0f;

        // isCrouching Check
        bool crouchKey = Input.GetKeyDown(KeyCode.LeftControl);
        if (crouchKey && !isCrouching)
        {
            isCrouching = true;
        }
        else if (crouchKey && isCrouching)
        {
            isCrouching = false;
        }

        // isSprinting Check
        bool sprintKey = Input.GetKey(KeyCode.LeftShift);
        isSprinting = sprintKey && currentStamina > 0f && isMoving;

        // currentSpeed selection
        if (state == StateEnum.walking)
        {
            currentSpeed = walkSpeed;
        }
        else if (state == StateEnum.sprinting)
        {
            currentSpeed = sprintSpeed;
        }
        else if (state == StateEnum.crouching || state == StateEnum.crouchWalk)
        {
            currentSpeed = crouchSpeed;
        }

        #endregion

        #region Stamina
        if (isSprinting)
        {
            // Drain stamina if sprinting
            currentStamina -= staminaDrainRate * Time.deltaTime;
            if (currentStamina <= 0f)
            {
                // lower cap on stamina
                currentStamina = 0f;
                isSprinting = false;
            }
        }
        else
        {
            // regen stamina if not sprinting
            currentStamina += staminaRegenRate * Time.deltaTime;
            // top cap on stamina
            if (currentStamina > maxStamina)
                currentStamina = maxStamina;
        }

        if (staminaBar != null)
            staminaBar.value = currentStamina;
        #endregion

        #region Jump & Gravity
        // get jumpVelocity if jump is pressed
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // get gravity velocity if in the air
        if (!isGrounded)
            velocity.y += gravity * Time.deltaTime;

        //apply either the jump or gravity velocity
        characterController.Move(velocity * Time.deltaTime);
        #endregion

        #region State

        // state machine for animations
        if (isSprinting)
        {
            state = StateEnum.sprinting;
        }
        else if (isCrouching && isMoving)
        {
            state = StateEnum.crouchWalk;
        }else if (isMoving)
        {
            state = StateEnum.walking;
        }else if (isCrouching)
        {
            state = StateEnum.crouching;
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            state = StateEnum.emote;
        }
        else
            state = StateEnum.idle;










        animator.SetInteger("State", (int)state);
        #endregion
    }
}
