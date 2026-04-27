using UnityEngine;

public class TopDownCharacterController : MonoBehaviour
{
    public enum ForwardAxis { X, MinusX, Z, MinusZ }

    [Header("Components")]
    public CharacterPhysicsMotor motor;
    public Transform playerTransform;
    public Animator animator;

    [Header("Movement Keys")]
    public KeyCode keyForward = KeyCode.W;
    public KeyCode keyBackward = KeyCode.S;
    public KeyCode keyLeft = KeyCode.A;
    public KeyCode keyRight = KeyCode.D;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode crouchKey = KeyCode.C;
    public KeyCode[] standUpKeys;

    [Header("Movement Settings")]
    [SerializeField] private float realTimeSpeed;
    [SerializeField] private float realTimeTurnSpeed;

    public float moveSpeed = 5f;
    public float sprintSpeedMultiplier = 1.8f;
    public float jumpSpeedMultiplier = 0.6f;
    public float crouchSpeedMultiplier = 0.4f;
    public float acceleration = 12f;
    public float sprintAccelerationMultiplier = 1.5f;
    public float crouchAccelerationMultiplier = 0.5f;
    public float jumpForce = 7f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 720f;
    public ForwardAxis modelForwardAxis = ForwardAxis.X;

    [Header("Animation Parameters Names")]
    public string speedFloatName = "Speed";
    public string turnFloatName = "TurnSpeed";
    public string jumpTriggerName = "Jump";
    public string groundedBoolName = "isGrounded";
    public string crouchBoolName = "isCrouching";

    [Header("Camera")]
    [SerializeField] private Camera cam;

    [Header("Input Lock System")]
    public InputLockRule[] inputLockRules;

    [System.Flags]
    public enum InputBlockType
    {
        None = 0,
        Move = 1 << 0,
        Sprint = 1 << 1,
        Jump = 1 << 2,
        Crouch = 1 << 3
    }

    public enum ActionState
    {
        Run,
        Jump,
        Crouch
    }

    [System.Serializable]
    public class InputLockRule
    {
        public ActionState state;
        public InputBlockType blockedInputs;
    }

    private ActionState currentState;
    private float lastYRotation;
    private bool isCrouching;

    void Awake()
    {
        if (motor != null && playerTransform == null)
            playerTransform = motor.transform;

        lastYRotation = playerTransform.eulerAngles.y;

        if (cam == null)
            cam = Camera.main;

        if (cam == null)
            Debug.LogWarning("Camera is not assigned!");
    }

    void Update()
    {
        if (motor == null || playerTransform == null) return;

        UpdateState();
        HandleJump();
        HandleCrouch();
        HandleMovement();
        HandleRotationToMouseYOnly();
        HandleAnimation();

        realTimeSpeed = motor.GetHorizontalSpeed();
    }

    void UpdateState()
    {
        if (!motor.IsGrounded)
            currentState = ActionState.Jump;
        else if (isCrouching)
            currentState = ActionState.Crouch;
        else
            currentState = ActionState.Run;
    }

    bool IsBlocked(InputBlockType type)
    {
        for (int i = 0; i < inputLockRules.Length; i++)
        {
            if (inputLockRules[i].state != currentState)
                continue;

            return (inputLockRules[i].blockedInputs & type) != 0;
        }
        return false;
    }

    bool IsStandUpPressed()
    {
        for (int i = 0; i < standUpKeys.Length; i++)
        {
            if (Input.GetKeyDown(standUpKeys[i]))
                return true;
        }
        return false;
    }

    void HandleCrouch()
    {
        if (IsBlocked(InputBlockType.Crouch)) return;
        if (currentState == ActionState.Jump) return;

        if (isCrouching && Input.GetKey(sprintKey))
        {
            SetCrouch(false);
            return;
        }

        if (isCrouching && IsStandUpPressed())
        {
            SetCrouch(false);
            return;
        }

        if (Input.GetKeyDown(crouchKey) && !Input.GetKey(sprintKey))
        {
            SetCrouch(!isCrouching);
        }
    }

    void SetCrouch(bool state)
    {
        isCrouching = state;
    }

    void HandleJump()
    {
        if (IsBlocked(InputBlockType.Jump)) return;

        if (Input.GetKeyDown(jumpKey) && motor.IsGrounded && !isCrouching)
        {
            motor.RequestJump(jumpForce);

            if (animator != null && !string.IsNullOrEmpty(jumpTriggerName))
                animator.SetTrigger(jumpTriggerName);
        }
    }

    void HandleMovement()
    {
        if (IsBlocked(InputBlockType.Move)) return;

        Vector3 localInput = Vector3.zero;

        if (Input.GetKey(keyForward)) localInput.z += 1f;
        if (Input.GetKey(keyBackward)) localInput.z -= 1f;
        if (Input.GetKey(keyLeft)) localInput.x -= 1f;
        if (Input.GetKey(keyRight)) localInput.x += 1f;

        if (localInput.sqrMagnitude > 1f)
            localInput.Normalize();

        Quaternion logicalRotation = playerTransform.rotation * Quaternion.Inverse(GetAxisOffset());
        Vector3 relativeMovement = logicalRotation * localInput;

        bool isSprinting = Input.GetKey(sprintKey)
                           && !IsBlocked(InputBlockType.Sprint)
                           && currentState != ActionState.Crouch;

        float targetMoveSpeed = moveSpeed;

        if (currentState == ActionState.Crouch)
        {
            targetMoveSpeed *= crouchSpeedMultiplier;
            isSprinting = false;
        }
        else if (currentState == ActionState.Jump)
        {
            targetMoveSpeed *= jumpSpeedMultiplier;
            isSprinting = false;
        }
        else if (isSprinting)
        {
            targetMoveSpeed *= sprintSpeedMultiplier;
        }

        float currentAcceleration = acceleration;

        if (isSprinting && currentState != ActionState.Jump)
            currentAcceleration *= sprintAccelerationMultiplier;

        if (currentState == ActionState.Crouch)
            currentAcceleration *= crouchAccelerationMultiplier;

        motor.SetMoveData(
            relativeMovement,
            isSprinting,
            targetMoveSpeed,
            targetMoveSpeed,
            currentAcceleration
        );
    }

    Quaternion GetAxisOffset()
    {
        switch (modelForwardAxis)
        {
            case ForwardAxis.X:
                return Quaternion.Euler(0, -90, 0);
            case ForwardAxis.MinusX:
                return Quaternion.Euler(0, 90, 0);
            case ForwardAxis.Z:
                return Quaternion.identity;
            case ForwardAxis.MinusZ:
                return Quaternion.Euler(0, 180, 0);
            default:
                return Quaternion.identity;
        }
    }

    void HandleRotationToMouseYOnly()
    {
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, playerTransform.position);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 point = ray.GetPoint(distance);
            Vector3 direction = point - playerTransform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f) return;

            Quaternion lookRotation = Quaternion.LookRotation(direction);
            Quaternion targetRotation = lookRotation * GetAxisOffset();

            playerTransform.rotation = Quaternion.RotateTowards(
                playerTransform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    void HandleAnimation()
    {
        if (animator == null) return;

        if (!string.IsNullOrEmpty(speedFloatName))
            animator.SetFloat(speedFloatName, motor.GetHorizontalSpeed());

        if (!string.IsNullOrEmpty(groundedBoolName))
            animator.SetBool(groundedBoolName, motor.IsGrounded);

        if (!string.IsNullOrEmpty(crouchBoolName))
            animator.SetBool(crouchBoolName, isCrouching);

        float currentY = playerTransform.eulerAngles.y;
        float delta = Mathf.DeltaAngle(lastYRotation, currentY);

        realTimeTurnSpeed = Mathf.Abs(delta) / Time.deltaTime;
        lastYRotation = currentY;

        if (!string.IsNullOrEmpty(turnFloatName))
        {
            float turnSpeedNormalized = Mathf.InverseLerp(0f, rotationSpeed, realTimeTurnSpeed);
            animator.SetFloat(turnFloatName, turnSpeedNormalized);
        }
    }
}