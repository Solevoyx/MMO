using System.Collections.Generic;
using UnityEngine;

public class MMOCamera : MonoBehaviour
{
    public Transform target;
    public Camera cam;

    public Vector3 offset = new Vector3(0f, 10f, -8f);

    [Header("Rotation")]
    public float mouseSensitivity = 3f;
    private float currentAngle;
    private float pitch;

    [Header("Zoom")]
    public float zoomSpeed = 5f;
    public float minDistance = -14f;
    public float maxDistance = -4f;
    public string zoomAxis = "Mouse ScrollWheel";

    private float targetDistance;

    [Header("Obstacle Avoidance")]
    public float collisionRadius = 0.3f;
    public LayerMask obstacleMask;
    public float collisionOffset = 0.2f;

    [Header("Smooth")]
    public float smooth = 12f;
    public float zoomSmooth = 10f;
    public float collisionSmooth = 15f;

    [Header("Keys")]
    public List<KeyCode> disableCameraKeys = new List<KeyCode>();

    private bool cameraActive = false;

    private Vector3 positionVelocity;

    void Awake()
    {
        if (!cam) cam = Camera.main;

        targetDistance = offset.z;
        SetCameraState(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            SetCameraState(true);

        foreach (var key in disableCameraKeys)
        {
            if (Input.GetKeyDown(key))
            {
                SetCameraState(false);
                break;
            }
        }
    }

    void LateUpdate()
    {
        if (!target || !cam) return;

        if (cameraActive)
            HandleMouseRotation();

        HandleZoom();

        Vector3 desiredPos = CalculateDesiredPosition();

        cam.transform.position = Vector3.SmoothDamp(
            cam.transform.position,
            desiredPos,
            ref positionVelocity,
            1f / smooth
        );

        cam.transform.rotation = Quaternion.Euler(pitch, currentAngle, 0f);
    }

    void HandleMouseRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        currentAngle += mouseX * mouseSensitivity * 100f * Time.deltaTime;

        pitch -= mouseY * mouseSensitivity * 100f * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -60f, 60f);
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis(zoomAxis);

        targetDistance += scroll * zoomSpeed;
        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);

        offset.z = Mathf.Lerp(
            offset.z,
            targetDistance,
            1f - Mathf.Exp(-zoomSmooth * Time.deltaTime)
        );
    }

    Vector3 CalculateDesiredPosition()
    {
        Quaternion rot = Quaternion.Euler(pitch, currentAngle, 0f);

        Vector3 flatOffset = new Vector3(offset.x, 0f, offset.z);
        Vector3 rotated = rot * flatOffset;

        Vector3 basePos = target.position + rotated + Vector3.up * offset.y;

        // --- Obstacle avoidance НЕ ломает offset ---
        Vector3 dir = (basePos - target.position).normalized;
        float dist = Vector3.Distance(target.position, basePos);

        if (Physics.SphereCast(target.position, collisionRadius, dir, out RaycastHit hit, dist, obstacleMask))
        {
            float safeDist = hit.distance - collisionOffset;
            basePos = target.position + dir * safeDist + Vector3.up * offset.y;
        }

        return basePos;
    }

    void SetCameraState(bool active)
    {
        cameraActive = active;

        Cursor.visible = !active;
        Cursor.lockState = active ? CursorLockMode.Locked : CursorLockMode.None;
    }
}