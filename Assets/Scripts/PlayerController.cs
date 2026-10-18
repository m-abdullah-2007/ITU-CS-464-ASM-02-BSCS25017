using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpHeight = 1.8f;

    [Header("Jump Feel")]
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBuffer = 0.12f;
    [SerializeField] private float fallMultiplier = 2.2f;

    [Header("Ground Check")]
    [SerializeField] private Transform feet;
    [SerializeField] private float checkRadius = 0.15f;
    [SerializeField] private LayerMask groundMask;

    [Header("Landing Squash")]
    [SerializeField] private Transform body;
    [SerializeField] private float squashDuration = 0.2f;
    [SerializeField] private float squashHeight = 0.7f;

    private Rigidbody rb;
    private Transform cam;
    private Vector3 move;
    private Vector3 bodyRestScale;

    private bool isGrounded;
    private bool wasGrounded;
    private bool jumpQueued;

    private float lastGroundedTime = float.NegativeInfinity;
    private float lastPressedTime = float.NegativeInfinity;
    private float airTime;

    private Coroutine squashRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (body != null)
            bodyRestScale = body.localScale;
    }

    private void Start()
    {
        cam = Camera.main.transform;
    }

    private void Update()
    {
        // Read input every rendered frame.
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space);

        // Get camera directions, flattened onto the ground.
        Vector3 forward = cam.forward;
        Vector3 right = cam.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        // Move relative to the camera.
        move = forward * vertical + right * horizontal;

        // Prevent faster diagonal movement.
        move = Vector3.ClampMagnitude(move, 1f);

        // Check for ground around the feet.
        isGrounded = Physics.CheckSphere(
            feet.position,
            checkRadius,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

        // Track time in the air and detect a landing.
        if (!isGrounded)
        {
            airTime += Time.deltaTime;
        }
        else
        {
            if (!wasGrounded && airTime > 0.1f && body != null)
            {
                if (squashRoutine != null)
                    StopCoroutine(squashRoutine);

                squashRoutine = StartCoroutine(LandingSquash());
            }

            airTime = 0f;
        }

        wasGrounded = isGrounded;

        // Remember the last grounded time.
        // Do not refresh it while rising or awaiting a jump.
        if (isGrounded && !jumpQueued && rb.linearVelocity.y <= 0f)
            lastGroundedTime = Time.time;

        // Remember when Space was pressed.
        if (jumpPressed)
            lastPressedTime = Time.time;

        bool canJump =
            Time.time - lastGroundedTime <= coyoteTime;

        bool wantsJump =
            Time.time - lastPressedTime <= jumpBuffer;

        if (!jumpQueued && canJump && wantsJump)
        {
            jumpQueued = true;

            // Use up both windows.
            lastGroundedTime = float.NegativeInfinity;
            lastPressedTime = float.NegativeInfinity;
        }
    }

    private void FixedUpdate()
    {
        // Apply horizontal movement while preserving vertical velocity.
        rb.linearVelocity = new Vector3(
            move.x * speed,
            rb.linearVelocity.y,
            move.z * speed
        );

        if (jumpQueued)
        {
            Debug.Log("Space pressed!");
            // Reset vertical velocity so falling does not shorten the jump.
            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0f;
            rb.linearVelocity = velocity;

            // Calculate launch speed from the desired jump height.
            float gravity = -Physics.gravity.y;
            float launchSpeed =
                Mathf.Sqrt(2f * gravity * jumpHeight);

            rb.AddForce(
                Vector3.up * rb.mass * launchSpeed,
                ForceMode.Impulse
            );

            jumpQueued = false;
        }

        // Apply extra gravity only while falling.
        if (rb.linearVelocity.y < 0f)
        {
            rb.AddForce(
                Physics.gravity * (fallMultiplier - 1f),
                ForceMode.Acceleration
            );
        }
    }

    private IEnumerator LandingSquash()
    {
        // Widen the body as it squashes.
        float width = 1f / Mathf.Sqrt(squashHeight);

        Vector3 squashedScale = new Vector3(
            bodyRestScale.x * width,
            bodyRestScale.y * squashHeight,
            bodyRestScale.z * width
        );

        for (float elapsed = 0f;
             elapsed < squashDuration;
             elapsed += Time.deltaTime)
        {
            float t = elapsed / squashDuration;
            float eased = EaseOutBack(t);

            body.localScale = Vector3.LerpUnclamped(
                squashedScale,
                bodyRestScale,
                eased
            );

            yield return null;
        }

        // Finish exactly at the original scale.
        body.localScale = bodyRestScale;
        squashRoutine = null;
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        float x = t - 1f;

        return 1f + c3 * x * x * x + c1 * x * x;
    }

    private void OnDrawGizmosSelected()
    {
        if (feet == null)
            return;

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(feet.position, checkRadius);
    }
}