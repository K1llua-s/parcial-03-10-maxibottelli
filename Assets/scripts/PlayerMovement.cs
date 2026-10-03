using UnityEngine;

// Movimiento relativo a la cámara: W va hacia donde mirás.
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 7f;
    public float rotationSpeed = 12f;
    public Transform cameraTransform; // dejar vacío: se busca sola

    [Header("Salto")]
    public float jumpForce = 7f;
    [Range(0f, 1f)] public float groundNormalMinY = 0.5f;
    [Tooltip("Tiempo extra para saltar después de dejar de tocar el suelo")]
    public float coyoteTime = 0.15f;
    [Tooltip("Tiempo que se recuerda un salto apretado justo antes de aterrizar")]
    public float jumpBufferTime = 0.15f;

    [Header("Dash")]
    public KeyCode dashKey = KeyCode.LeftShift;
    public float dashSpeed = 22f;
    public float dashDuration = 0.18f;
    public float dashCooldown = 0.8f;

    Rigidbody rb;
    Vector3 inputDir;
    float lastGroundedTime = -10f;
    float lastJumpPressTime = -10f;

    bool isDashing;
    float dashTimer;
    float nextDashTime;
    Vector3 dashDir;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.sleepThreshold = 0f; // evita que el Rigidbody se "duerma" y deje de detectar el suelo
    }

    Transform FindCamera()
    {
        if (cameraTransform != null) return cameraTransform;
        if (Camera.main != null) return Camera.main.transform;
        Camera any = FindFirstObjectByType<Camera>();
        return any != null ? any.transform : null;
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Transform cam = FindCamera();
        Vector3 camForward = cam != null ? cam.forward : Vector3.forward;
        Vector3 camRight = cam != null ? cam.right : Vector3.right;
        camForward.y = 0f; camRight.y = 0f;
        camForward.Normalize(); camRight.Normalize();

        inputDir = camForward * v + camRight * h;
        if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();

        if (Input.GetButtonDown("Jump")) lastJumpPressTime = Time.time;

        if (Input.GetKeyDown(dashKey) && Time.time >= nextDashTime && !isDashing)
        {
            dashDir = inputDir.sqrMagnitude > 0.01f ? inputDir.normalized : transform.forward;
            isDashing = true;
            dashTimer = dashDuration;
            nextDashTime = Time.time + dashCooldown;
        }
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            dashTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = new Vector3(dashDir.x * dashSpeed, 0f, dashDir.z * dashSpeed);
            if (dashTimer <= 0f) isDashing = false;
            return;
        }

        // Movimiento XZ conservando la velocidad vertical (gravedad)
        Vector3 vel = rb.linearVelocity;
        vel.x = inputDir.x * moveSpeed;
        vel.z = inputDir.z * moveSpeed;
        rb.linearVelocity = vel;

        if (inputDir.sqrMagnitude > 0.01f)
        {
            Quaternion target = Quaternion.LookRotation(inputDir, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, target, rotationSpeed * Time.fixedDeltaTime));
        }

        // Salto: se permite si tocó suelo hace poco y si se apretó Espacio hace poco
        bool canJump = Time.time - lastGroundedTime <= coyoteTime;
        bool wantsJump = Time.time - lastJumpPressTime <= jumpBufferTime;

        if (canJump && wantsJump)
        {
            Vector3 v = rb.linearVelocity;
            v.y = 0f;
            rb.linearVelocity = v;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            lastGroundedTime = -10f;  // evita doble salto
            lastJumpPressTime = -10f;
        }
    }

    void OnCollisionEnter(Collision collision) { CheckGround(collision); }
    void OnCollisionStay(Collision collision) { CheckGround(collision); }

    void CheckGround(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y >= groundNormalMinY)
            {
                lastGroundedTime = Time.time;
                return;
            }
        }
    }

    public void ResetMotion()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        isDashing = false;
        lastJumpPressTime = -10f;
    }
}