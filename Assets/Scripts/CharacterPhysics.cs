using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PhotonView))]
public class CharacterPhysicsMotor : MonoBehaviourPun
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

    public bool IsGrounded => controller.isGrounded;

    void Awake()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // 🔥 КЛЮЧЕВОЙ ФИКС: только владелец симулирует движение
        if (!photonView.IsMine)
            return;

        ApplyMovement();
    }

    public void SetMoveData(Vector3 moveInput, bool isSprinting, float moveSpeed, float sprintSpeed, float acceleration)
    {
        input = moveInput;
        sprint = isSprinting;
        this.moveSpeed = moveSpeed;
        this.sprintSpeed = sprintSpeed;
        this.acceleration = acceleration;
    }

    void ApplyMovement()
    {
        float targetSpeed = sprint ? sprintSpeed : moveSpeed;
        Vector3 targetVelocity = input * targetSpeed;

        // --- Smooth movement ---
        Vector3 velocityDiff = targetVelocity - smoothVelocity;

        if (velocityDiff.sqrMagnitude > 0.0001f)
        {
            Vector3 accelStep = Vector3.ClampMagnitude(
                velocityDiff,
                acceleration * Time.deltaTime
            );

            smoothVelocity += accelStep;
        }
        else
        {
            smoothVelocity = targetVelocity;
        }

        // --- Jump ---
        if (jumpRequested && IsGrounded)
        {
            verticalVelocity.y = jumpForce;
            jumpRequested = false;
        }

        // --- Gravity ---
        verticalVelocity.y += gravity * Time.deltaTime;

        // --- Ground stabilisation ---
        if (IsGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
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