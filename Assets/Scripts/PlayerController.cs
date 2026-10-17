using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] float speed = 5f;                 // m/s

    [Header("Jump")]
    [SerializeField] float jumpHeight = 1.8f;          // metres
    [SerializeField] float coyoteTime = 0.1f;          // s
    [SerializeField] float jumpBuffer = 0.12f;         // s
    [SerializeField] float fallMultiplier = 2.2f;      // x gravity while falling

    [Header("Ground check")]
    [SerializeField] Transform feet;                   // child at (0, -0.9, 0)
    [SerializeField] float checkRadius = 0.3f;
    [SerializeField] LayerMask groundMask;             // ONLY the Ground layer

    [Header("Juice: landing squash")]
    [SerializeField] Transform body;                   // child "Body" with the mesh
    [SerializeField] float squashY = 0.7f;
    [SerializeField] float squashDuration = 0.25f;     // s
    [SerializeField] float minAirTime = 0.1f;          // s in the air before a landing counts

    Rigidbody rb;
    Transform cam;
    Vector3 move;                                      // built in Update, used in FixedUpdate
    bool isGrounded, wasGrounded, jumpQueued;
    float lastGroundedTime = -1f, lastPressedTime = -1f, airTime;
    Coroutine squashRoutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        if (Camera.main != null) cam = Camera.main.transform;
    }

    void Update()
    {
        // ---- ground check ----
        isGrounded = Physics.CheckSphere(feet.position, checkRadius, groundMask,
                                         QueryTriggerInteraction.Ignore);

        // ---- landing detection for the juice ----
        if (isGrounded && !wasGrounded && airTime > minAirTime)
        {
            if (squashRoutine != null) StopCoroutine(squashRoutine);
            squashRoutine = StartCoroutine(Squash());
        }
        airTime = isGrounded ? 0f : airTime + Time.deltaTime;
        wasGrounded = isGrounded;

        // ---- move input, relative to the camera ----
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 forward = cam != null ? cam.forward : Vector3.forward;
        Vector3 right = cam != null ? cam.right : Vector3.right;
        forward.y = 0f; forward.Normalize();
        right.y = 0f; right.Normalize();

        move = Vector3.ClampMagnitude(forward * v + right * h, 1f);

        // ---- jump windows ----
        if (Input.GetKeyDown(KeyCode.Space)) lastPressedTime = Time.time;

        if (isGrounded && !jumpQueued && rb.linearVelocity.y <= 0f)
            lastGroundedTime = Time.time;

        bool canJump = Time.time - lastGroundedTime <= coyoteTime;
        bool wantJump = Time.time - lastPressedTime <= jumpBuffer;

        if (canJump && wantJump && !jumpQueued)
        {
            jumpQueued = true;
            lastGroundedTime = -1f;   // use up both windows
            lastPressedTime = -1f;
        }
    }

    void FixedUpdate()
    {
        Vector3 vel = rb.linearVelocity;
        rb.linearVelocity = new Vector3(move.x * speed, vel.y, move.z * speed);

        if (jumpQueued)
        {
            // zero y first so a jump taken while falling is still full height
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            float g = -Physics.gravity.y;
            float v0 = Mathf.Sqrt(2f * g * jumpHeight);
            rb.AddForce(Vector3.up * rb.mass * v0, ForceMode.Impulse);
            jumpQueued = false;
        }

        // extra gravity only while falling
        if (rb.linearVelocity.y < 0f)
            rb.AddForce(Physics.gravity * (fallMultiplier - 1f), ForceMode.Acceleration);
    }

    IEnumerator Squash()
    {
        for (float elapsed = 0f; elapsed < squashDuration; elapsed += Time.deltaTime)
        {
            float p = Ease.OutBack(elapsed / squashDuration);   // 0 = squashed, 1 = rest
            SetBody(Mathf.LerpUnclamped(squashY, 1f, p));       // unclamped keeps the overshoot
            yield return null;
        }
        body.localScale = Vector3.one;                          // exactly at rest
        body.localPosition = Vector3.zero;
        squashRoutine = null;
    }

    // y scale, with x and z keeping the volume; the body's feet stay on the ground
    void SetBody(float y)
    {
        if (body == null) return;
        float xz = 1f / Mathf.Sqrt(y);
        body.localScale = new Vector3(xz, y, xz);
        body.localPosition = new Vector3(0f, y - 1f, 0f);
    }

    void OnDrawGizmosSelected()
    {
        if (feet == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(feet.position, checkRadius);
    }
}
