using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterPhysicsMotor : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;

    private Vector3 input;
    private bool sprint;

    private float moveSpeed;
    private float sprintSpeed;
    private float acceleration;

    private Vector3 smoothVelocity;
    private Vector3 verticalVelocity;
    private bool jumpRequested;
    private float jumpForce;

    [Header("Gravity Settings")]
    public float gravity = -20f;
    public float groundStickForce = -2f; // Сила "прилипания" к земле

    // Свойство для получения состояния земли в контроллере
    public bool IsGrounded => controller.isGrounded;

    void Awake()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();
    }

    public void SetMoveData(Vector3 moveInput, bool isSprinting, float moveSpeed, float sprintSpeed, float acceleration)
    {
        input = moveInput;
        sprint = isSprinting;
        this.moveSpeed = moveSpeed;
        this.sprintSpeed = sprintSpeed;
        this.acceleration = acceleration;
    }

    void Update()
    {
        ApplyMovement();
    }

    void ApplyMovement()
    {
        // Рассчитываем целевую скорость
        float targetSpeed = sprint ? sprintSpeed : moveSpeed;
        Vector3 targetVelocity = input * targetSpeed;

        // Плавное ускорение и торможение
        smoothVelocity = Vector3.Lerp(
            smoothVelocity,
            targetVelocity,
            1f - Mathf.Exp(-acceleration * Time.deltaTime)
        );

        // Обработка гравитации
        if (IsGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = groundStickForce;
        }

        // Обработка прыжка
        if (jumpRequested && IsGrounded)
        {
            verticalVelocity.y = jumpForce;
            jumpRequested = false;
        }

        verticalVelocity.y += gravity * Time.deltaTime;

        // Итоговый вектор движения
        Vector3 finalVelocity = smoothVelocity + verticalVelocity;
        controller.Move(finalVelocity * Time.deltaTime);

        // Сбрасываем запрос на прыжок в конце кадра, если мы не на земле
        if (jumpRequested && !IsGrounded)
        {
            jumpRequested = false;
        }
    }

    public void RequestJump(float force)
    {
        jumpForce = force;
        jumpRequested = true;
    }

    public float GetHorizontalSpeed()
    {
        Vector3 v = controller.velocity;
        v.y = 0;
        return v.magnitude;
    }
}