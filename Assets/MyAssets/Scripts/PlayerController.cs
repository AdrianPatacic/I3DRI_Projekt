using System;
using System.Collections;
using Assets.Models;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class PlayerController : Entity
    {
        

        [SerializeField] private CharacterController characterController;
        [SerializeField] private Animator animator;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private WeaponController weaponController;
        [SerializeField] private HudController hudController;
        [SerializeField] private AudioClip[] attackAudioClips;
        [SerializeField] private AudioClip[] hitAudioClips;
        [SerializeField] private AudioClip[] jumpAudioClips;

        [SerializeField] public bool dead = false;

        private float currentSpeed;
        [SerializeField] private float crouchSpeed = 2f;
        [SerializeField] private float attackMoveSpeed = 2f;
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintSpeed = 10f;

        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float coyoteTime = 0.01f;
        private float lastTimeGrounded;

        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float staminaDrainRate = 25f;
        [SerializeField] private float staminaRegenRate = 15f;
        [SerializeField] private float currentStamina;


        [SerializeField] private bool canMove = true;
        [SerializeField] private bool isCrouching = false;
        [SerializeField] private bool isRolling = false;
        [SerializeField] private bool isAttacking = false;
        [SerializeField] private bool isMoving;
        [SerializeField] private bool isSprinting;
        [SerializeField] private bool isJumping;
        [SerializeField] private bool isFalling;
        [SerializeField] private Vector3 velocity;
        [SerializeField] private bool isGrounded;

        [SerializeField] private bool lockedOn;
        public bool lockedOnPublic => lockedOn;

        [SerializeField] private GameObject lockedOnGameObject;
        public GameObject lockedOnGameObjectPublic => lockedOnGameObject;


        private enum StateEnum { idle, walking, sprinting, roll, crouching, crouchWalk, jumping, falling, attacking }
        private StateEnum state = StateEnum.idle;
        private Vector3 lastPosition;

        Coroutine staminaRecoveryCoroutine;

        void Start()
        {
            characterController = GetComponent<CharacterController>();
            currentStamina = maxStamina;
            currentHealth = maxHealth;

            lastPosition = transform.position;
        }


        void Update()
        {

            #region Grounding
            
            if(characterController.isGrounded && characterController.velocity.y <= 0)
            {
                lastTimeGrounded = Time.time;
                velocity.y = -2;
            }

            isGrounded = Time.time - lastTimeGrounded <= coyoteTime;

            #endregion

            #region LockOn
            bool lockOnKey = Input.GetKeyDown(KeyCode.E);

            if (lockedOnGameObject == null)
                lockedOn = false;

            if (lockOnKey && lockedOn)
            {
                lockedOn = false;
            }
            else if (lockOnKey && !lockedOn && lockedOnGameObject != null)
            {
                lockedOn = true;
            }
            #endregion

            #region Movement

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

            if (canMove)
            {
                Vector3 move = camForward * z + camRight * x;
                characterController.Move(move.normalized * currentSpeed * Time.deltaTime);
            }

            // Rotate player based on move input and camera rotation or based on lockedOnGameObject
            Vector3 moveDir = new Vector3(x, 0f, z);
            if (lockedOn)
            {
                Vector3 direction = lockedOnGameObject.transform.position - transform.position;
                direction.y = 0f;
                if (direction != Vector3.zero)
                {
                    Quaternion rot = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 10f);
                }
            }
            else if (moveDir != Vector3.zero)
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
            isMoving = velocityCheck.magnitude > 0.1f;

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
            if (state == StateEnum.attacking)
            {
                currentSpeed = attackMoveSpeed;
            }
            else if (state == StateEnum.walking)
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
                DrainStamina();

            }
            else if (currentStamina != maxStamina && staminaRecoveryCoroutine == null)
            {
                staminaRecoveryCoroutine = StartCoroutine(RecoverStamina());
            }

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

            #region Attack
            if (Input.GetKeyDown(KeyCode.K) && !isAttacking && currentStamina > 0)
            {
                isAttacking = true;
            }

            #endregion

            #region Roll

            if (Input.GetKeyDown(KeyCode.X) && !isRolling && isGrounded)
            {
                isRolling = true;
            }

            #endregion

            #region State

            isJumping = !isGrounded && characterController.velocity.y >= 0f;

            isFalling = !isGrounded && characterController.velocity.y < 0f;

            if (isRolling)
            {
                state = StateEnum.roll;
            }
            else if (isAttacking)
            {
                state = StateEnum.attacking;
            }
            else if (isJumping)
            {
                state = StateEnum.jumping;
            }
            else if (isFalling)
            {
                state = StateEnum.falling;
            }
            else if (isSprinting)
            {
                state = StateEnum.sprinting;
            }
            else if (isCrouching && isMoving)
            {
                state = StateEnum.crouchWalk;
            }
            else if (isMoving)
            {
                state = StateEnum.walking;
            }
            else if (isCrouching)
            {
                state = StateEnum.crouching;
            }
            else
                state = StateEnum.idle;

            animator.SetInteger("State", (int)state);
            #endregion
        }

        private IEnumerator RecoverStamina()
        {
            yield return new WaitForSeconds(1f);

            while (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                if (currentStamina > maxStamina)
                    currentStamina = maxStamina;

                yield return null; // wait 1 frame
            }

            staminaRecoveryCoroutine = null;
        }

        private void DrainStamina()
        {
            if (staminaRecoveryCoroutine != null)
            {
                StopCoroutine(staminaRecoveryCoroutine);
                staminaRecoveryCoroutine = null;
            }

            currentStamina -= staminaDrainRate * Time.deltaTime;
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                isSprinting = false;
            }
        }

        private void DrainStamina(float amount)
        {
            if (staminaRecoveryCoroutine != null)
            {
                StopCoroutine(staminaRecoveryCoroutine);
                staminaRecoveryCoroutine = null;
            }

            currentStamina -= amount;
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
            }
        }

        public void PlayRandomAttackSound()
        {
            int i = UnityEngine.Random.Range(0, attackAudioClips.Length);
            AudioSource.PlayClipAtPoint(attackAudioClips[i], transform.position);
        }

        public void PlayRandomHitSound()
        {
            int i = UnityEngine.Random.Range(0, hitAudioClips.Length);
            AudioSource.PlayClipAtPoint(hitAudioClips[i], transform.position);
        }

        public void PlayRandomJumpSound()
        {
            int i = UnityEngine.Random.Range(0, jumpAudioClips.Length);
            AudioSource.PlayClipAtPoint(jumpAudioClips[i], transform.position);
        }
        public void OnAttackStart()
        {
            weaponController.EnableCollider();
            DrainStamina(25);
        }

        public void OnAttackEnd()
        {
            isAttacking = false;
            weaponController.DisableCollider();
        }

        public void OnRollStart()
        {
            canMove = false;
            animator.applyRootMotion = true;
            DrainStamina(25);
        }

        public void OnRollEnd()
        {
            canMove = true;
            animator.applyRootMotion = false;
            isRolling = false;
        }

        public void EnableInvulnerability()
        {
            invulnreable = true;
        }

        public void DisableInvulnerability()
        {
            invulnreable = false;
        }

        protected override void Die()
        {
            Time.timeScale = 0f;
            hudController.EnableDeathScreen();
            dead = true;
        }

        public float GetMaxStamina() => maxStamina;
        public float GetMaxHealth() => maxHealth;

        public float GetStamina() => currentStamina;

        public float GetHealth() => currentHealth;


        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);
            PlayRandomHitSound();
        }
    }
}