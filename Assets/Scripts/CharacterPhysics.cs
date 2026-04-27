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
        float targetSpeed = sprint ? sprintSpeed : moveSpeed;
        Vector3 targetVelocity = input * targetSpeed;

        Vector3 velocityDiff = targetVelocity - smoothVelocity;

        if (velocityDiff.sqrMagnitude > 0.0001f)
        {
            Vector3 accelStep = Vector3.ClampMagnitude(velocityDiff, acceleration * Time.deltaTime);
            smoothVelocity += accelStep;
        }
        else
        {
            smoothVelocity = targetVelocity;
        }

        // Прыжок
        if (jumpRequested && IsGrounded)
        {
            verticalVelocity.y = jumpForce;
            jumpRequested = false;
        }

        // Гравитация
        verticalVelocity.y += gravity * Time.deltaTime;

        // СТАБИЛИЗАЦИЯ ЗЕМЛИ (ОДИН РАЗ!)
        if (IsGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -1f;
        }

        Vector3 finalVelocity = smoothVelocity + verticalVelocity;
        controller.Move(finalVelocity * Time.deltaTime);
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