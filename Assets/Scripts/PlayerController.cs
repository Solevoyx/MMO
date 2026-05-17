using UnityEngine;

public class TopDownCharacterController : MonoBehaviour
{
    public enum ForwardAxis { X, MinusX, Z, MinusZ }

    [System.Serializable]
    public struct AttackTriggerData
    {
        public string triggerName;            // Имя триггера анимации

        [Header("Timers")]
        public float preDashDelay;            // ЗАМАХ: задержка до начала движения (в секундах)
        public float dashDuration;            // РЫВОК: сколько секунд длится движение вперед
        public float postDashDelay;           // ВОССТАНОВЛЕНИЕ: задержка после движения до конца атаки

        [Header("Movement Settings")]
        public float moveSpeedMultiplier;     // Множитель скорости рывка
        public float accelerationMultiplier;  // Множитель резкости старта рывка
    }

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
    public KeyCode attackKey = KeyCode.Mouse0;
    public KeyCode[] standUpKeys;

    [Header("Input Buffer Settings")]
    [Tooltip("Время (в секундах), в течение которого нажатие кнопки сохраняется в памяти")]
    public float inputBufferTime = 0.25f;

    [Header("Movement Settings")]
    [SerializeField] private float realTimeSpeed;

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
    public float rotationSpeedMultiplier;
    public ForwardAxis modelForwardAxis = ForwardAxis.X;
    public Transform movementReference;

    [Header("Animation Parameters Names")]
    public string speedFloatName = "Speed";
    public string jumpTriggerName = "Jump";
    public string groundedBoolName = "isGrounded";
    public string crouchBoolName = "isCrouching";
    public string attackingBoolName = "isAttack";

    public AttackTriggerData[] attackTriggerNames;

    private bool isCrouching;
    private bool isAttacking;
    private bool isDashing;
    private int currentAttackIndex;

    private float preDashTimer;
    private float dashTimer;
    private float postDashTimer;

    private Vector3 attackMoveDirection;

    // Таймеры буфера ввода
    private float jumpBufferTimer;
    private float crouchBufferTimer;
    private float attackBufferTimer;

    void Awake()
    {
        if (motor != null && playerTransform == null)
            playerTransform = motor.transform;
    }

    void Update()
    {
        if (motor == null || playerTransform == null) return;

        UpdateInputBuffers();

        HandleJump();
        HandleCrouch();
        HandleAttack();
        HandleMovement();
        HandleAnimation();

        realTimeSpeed = motor.GetHorizontalSpeed();
    }

    // НОВЫЙ МЕТОД: Обновление таймеров памяти нажатий
    void UpdateInputBuffers()
    {
        if (Input.GetKeyDown(jumpKey)) jumpBufferTimer = inputBufferTime;
        else if (jumpBufferTimer > 0f) jumpBufferTimer -= Time.deltaTime;

        if (Input.GetKeyDown(crouchKey)) crouchBufferTimer = inputBufferTime;
        else if (crouchBufferTimer > 0f) crouchBufferTimer -= Time.deltaTime;

        if (Input.GetKeyDown(attackKey)) attackBufferTimer = inputBufferTime;
        else if (attackBufferTimer > 0f) attackBufferTimer -= Time.deltaTime;
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
        if (!motor.IsGrounded || isAttacking) return;

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

        // ИСПОЛЬЗУЕМ БУФЕР ВМЕСТО GetKeyDown
        if (crouchBufferTimer > 0f && !Input.GetKey(sprintKey))
        {
            crouchBufferTimer = 0f; // Сбрасываем буфер после использования
            SetCrouch(!isCrouching);
        }
    }

    void SetCrouch(bool state)
    {
        isCrouching = state;
    }

    void HandleJump()
    {
        // ИСПОЛЬЗУЕМ БУФЕР ВМЕСТО GetKeyDown
        if (jumpBufferTimer > 0f && motor.IsGrounded && !isCrouching && !isAttacking)
        {
            jumpBufferTimer = 0f; // Сбрасываем буфер после использования
            motor.RequestJump(jumpForce);

            if (animator != null && !string.IsNullOrEmpty(jumpTriggerName))
                animator.SetTrigger(jumpTriggerName);
        }
    }

    void HandleAttack()
    {
        if (animator == null || attackTriggerNames == null || attackTriggerNames.Length == 0)
            return;

        // 1. СТАРТ С НУЛЯ: ИСПОЛЬЗУЕМ БУФЕР ВМЕСТО GetKeyDown
        if (!isAttacking && attackBufferTimer > 0f && motor.IsGrounded)
        {
            attackBufferTimer = 0f; // Сбрасываем буфер после использования
            currentAttackIndex = 0;
            StartNextAttack();
            return;
        }

        if (!isAttacking) return;

        AttackTriggerData currentStep = attackTriggerNames[currentAttackIndex];

        // --- ЛОГИКА ОБРАБОТКИ ТРЁХ ФАЗ ---

        if (preDashTimer > 0f)
        {
            preDashTimer -= Time.deltaTime;
            attackMoveDirection = GetAttackDirectionInput();
            motor.SetMoveData(Vector3.zero, false, 0f, 0f, acceleration);

            if (preDashTimer <= 0f && currentStep.dashDuration > 0f)
            {
                isDashing = true;
            }
        }
        else if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
            else
            {
                motor.SetMoveData(
                    attackMoveDirection,
                    false,
                    moveSpeed * currentStep.moveSpeedMultiplier,
                    moveSpeed * currentStep.moveSpeedMultiplier,
                    acceleration * currentStep.accelerationMultiplier
                );
            }
        }
        else if (postDashTimer > 0f)
        {
            postDashTimer -= Time.deltaTime;
            motor.SetMoveData(Vector3.zero, false, 0f, 0f, acceleration);
        }

        // --- ПОВОРОТ ПЕРСОНАЖА С ГЛОБАЛЬНЫМ МНОЖИТЕЛЕМ ДЛЯ ВСЕХ АТАК ---
        if (isAttacking)
        {
            Vector3 currentLookDirection = GetAttackDirectionInput();

            if (currentLookDirection.sqrMagnitude > 0.001f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(currentLookDirection);
                Quaternion targetRotation = lookRotation * GetAxisOffset();

                // Умножаем глобальную скорость на твой глобальный множитель
                playerTransform.rotation = Quaternion.RotateTowards(
                    playerTransform.rotation,
                    targetRotation,
                    rotationSpeed * rotationSpeedMultiplier * Time.deltaTime
                );
            }
        }

        // --- ЖЕСТКАЯ ПРОВЕРКА ЗАВЕРШЕНИЯ ВСЕЙ АТАКИ ---
        if (preDashTimer <= 0f && !isDashing && postDashTimer <= 0f)
        {
            // Переходим к следующему удару комбо, если кнопка УДЕРЖИВАЕТСЯ ИЛИ нажатие сохранено в буфере
            if (Input.GetKey(attackKey) || attackBufferTimer > 0f)
            {
                attackBufferTimer = 0f; // Очищаем буфер, чтобы нажатие не "перетекло" дальше

                currentAttackIndex++;
                if (currentAttackIndex >= attackTriggerNames.Length)
                    currentAttackIndex = 0;

                StartNextAttack();
            }
            else
            {
                isAttacking = false;
                isDashing = false;
                currentAttackIndex = 0;
            }
        }
    }

    void StartNextAttack()
    {
        isAttacking = true;
        isCrouching = false;
        isDashing = false;

        AttackTriggerData currentStep = attackTriggerNames[currentAttackIndex];

        preDashTimer = currentStep.preDashDelay;
        dashTimer = currentStep.dashDuration;
        postDashTimer = currentStep.postDashDelay;

        if (preDashTimer <= 0f && dashTimer > 0f)
        {
            isDashing = true;
        }

        if (!string.IsNullOrEmpty(currentStep.triggerName))
        {
            animator.SetTrigger(currentStep.triggerName);
        }

        attackMoveDirection = GetAttackDirectionInput();
    }

    Vector3 GetAttackDirectionInput()
    {
        Vector3 localInput = Vector3.zero;
        if (Input.GetKey(keyForward)) localInput.z += 1f;
        if (Input.GetKey(keyBackward)) localInput.z -= 1f;
        if (Input.GetKey(keyLeft)) localInput.x -= 1f;
        if (Input.GetKey(keyRight)) localInput.x += 1f;

        if (movementReference != null)
        {
            float cameraY = movementReference.eulerAngles.y;
            Quaternion cameraYaw = Quaternion.Euler(0f, cameraY, 0f);

            if (localInput.sqrMagnitude > 0.001f)
            {
                localInput.Normalize();
                Vector3 forward = cameraYaw * Vector3.forward;
                Vector3 right = cameraYaw * Vector3.right;
                return (forward * localInput.z + right * localInput.x).normalized;
            }
            return cameraYaw * Vector3.forward;
        }
        else
        {
            if (localInput.sqrMagnitude > 0.001f)
            {
                return localInput.normalized;
            }
            Quaternion logicalRotation = playerTransform.rotation * Quaternion.Inverse(GetAxisOffset());
            return logicalRotation * Vector3.forward;
        }
    }

    void HandleMovement()
    {
        if (isAttacking || isDashing)
            return;

        Vector3 localInput = Vector3.zero;

        if (Input.GetKey(keyForward)) localInput.z += 1f;
        if (Input.GetKey(keyBackward)) localInput.z -= 1f;
        if (Input.GetKey(keyLeft)) localInput.x -= 1f;
        if (Input.GetKey(keyRight)) localInput.x += 1f;

        if (localInput.sqrMagnitude > 1f)
            localInput.Normalize();

        Transform reference = movementReference != null ? movementReference : playerTransform;

        float cameraY = reference.eulerAngles.y;
        Quaternion cameraYaw = Quaternion.Euler(0f, cameraY, 0f);

        Vector3 forward = cameraYaw * Vector3.forward;
        Vector3 right = cameraYaw * Vector3.right;

        Vector3 relativeMovement = forward * localInput.z + right * localInput.x;

        bool isSprinting = Input.GetKey(sprintKey) && !isCrouching;

        float targetMoveSpeed = moveSpeed;

        if (isCrouching)
        {
            targetMoveSpeed *= crouchSpeedMultiplier;
            isSprinting = false;
        }
        else if (!motor.IsGrounded)
        {
            targetMoveSpeed *= jumpSpeedMultiplier;
            isSprinting = false;
        }
        else if (isSprinting)
        {
            targetMoveSpeed *= sprintSpeedMultiplier;
        }

        float currentAcceleration = acceleration;

        if (isSprinting && motor.IsGrounded)
            currentAcceleration *= sprintAccelerationMultiplier;

        if (isCrouching)
            currentAcceleration *= crouchAccelerationMultiplier;

        if (relativeMovement.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(relativeMovement);
            Quaternion targetRotation = lookRotation * GetAxisOffset();

            playerTransform.rotation = Quaternion.RotateTowards(
                playerTransform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

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

    void HandleAnimation()
    {
        if (animator == null) return;

        if (!string.IsNullOrEmpty(speedFloatName))
            animator.SetFloat(speedFloatName, motor.GetHorizontalSpeed());

        if (!string.IsNullOrEmpty(groundedBoolName))
            animator.SetBool(groundedBoolName, motor.IsGrounded);

        if (!string.IsNullOrEmpty(crouchBoolName))
            animator.SetBool(crouchBoolName, isCrouching);

        if (!string.IsNullOrEmpty(attackingBoolName))
            animator.SetBool(attackingBoolName, isAttacking);
    }
}