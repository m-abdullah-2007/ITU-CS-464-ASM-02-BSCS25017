using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Camera Settings")]
    [SerializeField]
    private Vector3 offset =
        new Vector3(0f, 4f, -7f);

    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField] private float lookAhead = 0.35f;

    private Rigidbody targetRb;
    private Vector3 smoothVelocity;

    private void Awake()
    {
        // Find the player if no target was assigned.
        if (target == null)
        {
            PlayerController player =
                FindAnyObjectByType<PlayerController>();

            if (player != null)
                target = player.transform;
        }

        if (target != null)
            targetRb = target.GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (target == null)
            return;

        // Start at the correct position instead of flying in.
        transform.position = target.position + offset;
        transform.LookAt(target.position);
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 flatVelocity = Vector3.zero;

        if (targetRb != null)
        {
            flatVelocity = targetRb.linearVelocity;
            flatVelocity.y = 0f;
        }

        // Predict a point ahead of the player's movement.
        Vector3 aimPoint =
            target.position + flatVelocity * lookAhead;

        Vector3 wantedPosition = aimPoint + offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            wantedPosition,
            ref smoothVelocity,
            smoothTime
        );

        // Look at the predicted point, as required by the assignment.
        transform.LookAt(aimPoint);
    }
}