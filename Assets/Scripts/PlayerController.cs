using UnityEngine;

public class TopDownCharacterController : MonoBehaviour
{
    public enum ForwardAxis { X, MinusX, Z, MinusZ }

    [Header("Player")]
    public CharacterController playerController;
    public Transform playerTransform;

    [Header("Movement Keys")]
    public KeyCode keyForward = KeyCode.W;
    public KeyCode keyBackward = KeyCode.S;
    public KeyCode keyLeft = KeyCode.A;
    public KeyCode keyRight = KeyCode.D;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float acceleration = 12f;

    [Header("Rotation Settings (Y only)")]
    public float rotationSpeed = 720f;
    public ForwardAxis modelForwardAxis = ForwardAxis.X;

    private Camera cam;
    private Vector3 currentVelocity;

    void Awake()
    {
        cam = Camera.main;

        if (playerController != null && playerTransform == null)
            playerTransform = playerController.transform;
    }

    void Update()
    {
        if (playerController == null || playerTransform == null)
            return;

        HandleMovement();
        HandleRotationToMouseYOnly();
    }

    void HandleMovement()
    {
        Vector3 input = Vector3.zero;

        if (Input.GetKey(keyForward)) input.z += 1f;
        if (Input.GetKey(keyBackward)) input.z -= 1f;
        if (Input.GetKey(keyLeft)) input.x -= 1f;
        if (Input.GetKey(keyRight)) input.x += 1f;

        // чтобы диагональ не была быстрее
        if (input.sqrMagnitude > 1f)
            input.Normalize();

        // движение относительно персонажа
        Vector3 moveDirection =
            playerTransform.forward * input.z +
            playerTransform.right * input.x;

        Vector3 targetVelocity = moveDirection * moveSpeed;

        // ?? экспоненциально-плавная инерция (AAA feel)
        currentVelocity = Vector3.Lerp(
            currentVelocity,
            targetVelocity,
            1f - Mathf.Exp(-acceleration * Time.deltaTime)
        );

        // применение движения
        playerController.Move(currentVelocity * Time.deltaTime);
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

            Quaternion axisOffset = Quaternion.identity;

            switch (modelForwardAxis)
            {
                case ForwardAxis.X:
                    axisOffset = Quaternion.Euler(0, -90, 0);
                    break;
                case ForwardAxis.MinusX:
                    axisOffset = Quaternion.Euler(0, 90, 0);
                    break;
                case ForwardAxis.Z:
                    axisOffset = Quaternion.identity;
                    break;
                case ForwardAxis.MinusZ:
                    axisOffset = Quaternion.Euler(0, 180, 0);
                    break;
            }

            Quaternion targetRotation = lookRotation * axisOffset;

            playerTransform.rotation = Quaternion.RotateTowards(
                playerTransform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }
}