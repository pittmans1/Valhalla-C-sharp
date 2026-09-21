using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CatController : MonoBehaviour
{
    [Header("Movement Mechanics")]
    [SerializeField] private float moveSpeed = 8.0f;
    [SerializeField] private float rotationSpeed = 720.0f;

    [SerializeField] private float jumpForce = 9.0f;

    [Header("Feline Choas Actions")]
    [SerializeField] private Transform pawTransform;    
    [SerializeField] private float swipeRadius = 1.3f;
    [SerializeField] private float swipeforce = 15.0f;
    [SerializeField] private float swipeCooldown = 1.0f;
    [SerializeField] private LayerMask smashableLayer;
    [SerializeField] private Rigidbody rb;

    private Vector3 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Start()
    {
        // prevent the cat from rotating due to physics interactions
        rb.freezeRotation = true;
    }

    void Update()
    {
        Move();
        Jump();
    }

    void Move()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical).normalized;
        rb.AddForce(movement * moveSpeed);
        if (Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.Space))
        {
            Swipe();
        }
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && Mathf.Abs(rb.velocity.y) < 0.01f)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void Swipe()
    {
        Collider[] hitColliders = Physics.OverlapSphere(pawTransform.position, swipeRadius, smashableLayer);
        foreach (var hitCollider in hitColliders)
        {
            Rigidbody hitRb = hitCollider.GetComponent<Rigidbody>();
            if (hitRb != null)
            {
                Vector3 direction = (hitCollider.transform.position - pawTransform.position).normalized;
                hitRb.AddForce(direction * swipeforce, ForceMode.Impulse);
            }
        }
    }
}