using UnityEngine;

public class MMOCamera : MonoBehaviour
{
    public Transform target;
    public Camera cam;

    public Vector3 offset = new Vector3(0f, 10f, -8f);

    public KeyCode rotateLeftKey = KeyCode.Q;
    public KeyCode rotateRightKey = KeyCode.E;

    public float rotationSpeed = 180f;
    private float currentAngle;

    public float zoomSpeed = 5f;
    public float minHeight = 4f;
    public float maxHeight = 14f;

    public string zoomAxis = "Mouse ScrollWheel";

    public float smooth = 12f;

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    void LateUpdate()
    {
        if (!target || !cam) return;

        HandleRotation();
        HandleZoom();
        UpdateCamera();
    }

    void HandleRotation()
    {
        if (Input.GetKey(rotateLeftKey))
            currentAngle -= rotationSpeed * Time.deltaTime;

        if (Input.GetKey(rotateRightKey))
            currentAngle += rotationSpeed * Time.deltaTime;
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis(zoomAxis);

        offset.y -= scroll * zoomSpeed;
        offset.y = Mathf.Clamp(offset.y, minHeight, maxHeight);
    }

    void UpdateCamera()
    {
        Quaternion rot = Quaternion.Euler(0f, currentAngle, 0f);

        Vector3 flat = new Vector3(offset.x, 0f, offset.z);
        Vector3 rotated = rot * flat;

        Vector3 desired =
            target.position +
            rotated +
            Vector3.up * offset.y;

        cam.transform.position = Vector3.Lerp(
            cam.transform.position,
            desired,
            1f - Mathf.Exp(-smooth * Time.deltaTime)
        );

        // ? ¬¿∆ÕŒ: Õ» ¿ Œ√Œ LookAt
        cam.transform.rotation = Quaternion.Euler(
            cam.transform.eulerAngles.x,
            currentAngle,
            0f
        );
    }
}