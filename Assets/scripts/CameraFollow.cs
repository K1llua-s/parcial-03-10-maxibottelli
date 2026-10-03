using UnityEngine;

// Cámara en tercera persona estable: orbita con el mouse y sigue al jugador sin vibrar.
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);
    public float distance = 7f;
    public float mouseSensitivity = 3f;
    public float minPitch = -10f;
    public float maxPitch = 60f;
    public bool lockCursor = true;

    [Header("Colisión de cámara (opcional)")]
    public bool useCollision = false;

    float yaw;
    float pitch = 20f;

    void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        yaw = transform.eulerAngles.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (Time.timeScale > 0f)
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focus = target.position + targetOffset;
        Vector3 desired = focus - rot * Vector3.forward * distance;

        if (useCollision &&
            Physics.Linecast(focus, desired, out RaycastHit hit, ~0, QueryTriggerInteraction.Ignore) &&
            !hit.transform.IsChildOf(target))
        {
            desired = hit.point + hit.normal * 0.2f;
        }

        transform.SetPositionAndRotation(desired, rot);
    }
}