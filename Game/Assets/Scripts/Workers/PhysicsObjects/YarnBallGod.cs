using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class YarnBallGod : MonoBehaviour
{
    [Header("Boss Variables")]
    [SerializeField] private float rolloutSpeed = 12f;
    [SerializeField] private float directCrushDamage = 999f;
    [SerializeField] private float suctionRadius = 10f;
    [SerializeField] private float suctionForce = 15f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Massive physical scale presence
        transform.localScale = Vector3.one * 5f;
        rb.mass = 500f; 
    }

    private void FixedUpdate()
    {
        // Pull nearby stray cats in like a cosmic gravitational black hole
        Collider[] surroundingObjects = Physics.OverlapSphere(transform.position, suctionRadius);
        foreach (var col in surroundingObjects)
        {
            if (col.TryGetComponent<CatBrainController>(out CatBrainController cat))
            {
                Vector3 suctionPullDirection = (transform.position - cat.transform.position).normalized;
                if (cat.TryGetComponent<Rigidbody>(out Rigidbody catRb))
                {
                    catRb.AddForce(suctionPullDirection * suctionForce);
                }
            }
        }

        // Continuously roll forward down the map Z corridor path natively
        rb.AddForce(Vector3.forward * rolloutSpeed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<CatBrainController>(out CatBrainController cat))
        {
            Debug.Log("THE YARN BALL GOD CRUSHED A FOOLISH FELINE!");
            cat.isSleeping = true; // Force lock sleep down state instantly
            GameModeManager.Instance.CheckCatDownStates();
        }
    }
}
