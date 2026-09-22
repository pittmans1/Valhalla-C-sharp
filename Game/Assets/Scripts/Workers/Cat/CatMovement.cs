using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CatMovement : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 moveDirection;

    [Header("Locomotion Tuning")]
    [SerializeField] private float moveSpeed = 8.0f;
    [SerializeField] private float jumpForce = 9.0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        rb.freezeRotation = true;
    }

    // Driven purely by the brain layer
    public void SetMoveDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;
    }

    public void ExecuteJump()
    {
        // Simple ground check approximation based on vertical velocity
        if (Mathf.Abs(rb.linearVelocity.y) < 0.01f)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void BoostSpeed(float multiplier)
    {
        moveSpeed *= multiplier;
    }

    private void FixedUpdate()
    {
        if (moveDirection.magnitude > 0.01f)
        {
            rb.AddForce(moveDirection * moveSpeed);
        }
    }
}
