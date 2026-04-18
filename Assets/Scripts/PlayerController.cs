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
    public KeyCode squatKey = KeyCode.C;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 9f;
    public float acceleration = 12f;
    public float sprintAccelerationMultiplier = 1.5f;
    public float squatAccelerationMultiplier = 0.5f;
    public float jumpForce = 7f;
    public float jumpMoveSpeed = 3f;
    public float squatMoveSpeed = 2f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 720f;
    public ForwardAxis modelForwardAxis = ForwardAxis.X;

    [Header("Animation Parameters Names")]
    public string speedFloatName = "Speed";
    public string jumpTriggerName = "Jump";
    public string groundedBoolName = "isGrounded";
    public string squatBoolName = "Squat";

    private Camera cam;
    private bool isJumpingState;
    private bool isSquatting;

    void Awake()
    {
        cam = Camera.main;

        if (motor != null && playerTransform == null)
            playerTransform = motor.transform;
    }

    void Update()
    {
        if (motor == null || playerTransform == null)
            return;

        UpdateJumpState();
        HandleSquat();
        HandleJump();
        HandleMovement();
        HandleRotationToMouseYOnly();
        HandleAnimation();
    }

    private Quaternion GetAxisOffset()
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

    void UpdateJumpState()
    {
        isJumpingState = !motor.IsGrounded;
    }

    void HandleJump()
    {
        if (isSquatting) return;
        if (Input.GetKeyDown(jumpKey) && motor.IsGrounded)
        {
            motor.RequestJump(jumpForce);

            if (animator != null && !string.IsNullOrEmpty(jumpTriggerName))
            {
                animator.SetTrigger(jumpTriggerName);
            }
        }
    }

    void HandleSquat()
    {
        isSquatting = Input.GetKey(squatKey) && motor.IsGrounded;

        if (animator != null && !string.IsNullOrEmpty(squatBoolName))
            animator.SetBool(squatBoolName, isSquatting);
    }

    void HandleMovement()
    {
        Vector3 localInput = Vector3.zero;

        if (Input.GetKey(keyForward)) localInput.z += 1f;
        if (Input.GetKey(keyBackward)) localInput.z -= 1f;
        if (Input.GetKey(keyLeft)) localInput.x -= 1f;
        if (Input.GetKey(keyRight)) localInput.x += 1f;

        if (localInput.sqrMagnitude > 1f)
            localInput.Normalize();

        Quaternion logicalRotation =
            playerTransform.rotation * Quaternion.Inverse(GetAxisOffset());

        Vector3 relativeMovement = logicalRotation * localInput;

        bool isSprinting = Input.GetKey(sprintKey);
        float targetMoveSpeed = moveSpeed;
        float currentAcceleration = acceleration;

        // --- ПРЫЖОК ---
        if (isJumpingState)
        {
            targetMoveSpeed = jumpMoveSpeed;
            isSprinting = false;
        }

        // --- ПРИСЕД ---
        if (isSquatting)
        {
            targetMoveSpeed = squatMoveSpeed;
            currentAcceleration *= squatAccelerationMultiplier;
            isSprinting = false;
        }
        // --- СПРИНТ ---
        else if (isSprinting)
        {
            currentAcceleration *= sprintAccelerationMultiplier;
        }

        motor.SetMoveData(
            relativeMovement,
            isSprinting,
            targetMoveSpeed,
            sprintSpeed,
            currentAcceleration
        );
    }

    void HandleRotationToMouseYOnly()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, playerTransform.position);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 point = ray.GetPoint(distance);
            Vector3 direction = point - playerTransform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
                return;

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
        if (animator == null)
            return;

        if (!string.IsNullOrEmpty(speedFloatName))
            animator.SetFloat(speedFloatName, motor.GetHorizontalSpeed());

        if (!string.IsNullOrEmpty(groundedBoolName))
            animator.SetBool(groundedBoolName, motor.IsGrounded);
    }
}