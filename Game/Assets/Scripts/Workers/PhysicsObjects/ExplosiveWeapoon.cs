using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ExplosiveWeapon : MonoBehaviour
{
    [Header("Explosion Configuration")]
    [SerializeField] private float explosionRadius = 6.0f;
    [SerializeField] private float blastForce = 40.0f;
    [SerializeField] private float baseDamage = 45.0f;
    [SerializeField] private float impactThreshold = 4.0f;

    [Header("Visuals & Audio")]
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private AudioClip explosionSound;

    private Rigidbody rb;
    private bool hasDetonated = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasDetonated) return;

        // Detonate only if thrown with significant velocity force
        if (collision.relativeVelocity.magnitude >= impactThreshold)
        {
            Detonate();
        }
    }

    public void Detonate()
    {
        hasDetonated = true;
        Debug.Log($"Weapon Detonated! {gameObject.name} exploded!");

        // 1. Spatial physics overlap search to catch all targets in range
        Collider[] targetsInRange = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var target in targetsInRange)
        {
            // Push back and damage Cats
            if (target.TryGetComponent<CatBrainController>(out CatBrainController cat))
            {
                cat.TakeDamage(baseDamage);
                ApplyBlastForceToRigidbody(target.GetComponent<Rigidbody>());
            }
            // Stun or damage the Human
            else if (target.TryGetComponent<HumanBrain>(out HumanBrain human))
            {
                human.ExecuteSpray(); // Substitute with a human damage/stun system method
                ApplyBlastForceToRigidbody(target.GetComponent<Rigidbody>());
            }
            // Shatter environmental smashable items in the shockwave
            else if (target.TryGetComponent<SmashableProp>(out SmashableProp prop))
            {
                // Force a direct collision simulation to break it instantly
                prop.SendMessage("OnCollisionEnter", new Collision()); 
            }
        }

        // 2. Play FX Assets
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject); // Purge from runtime memory footprint cleanly
    }

    private void ApplyBlastForceToRigidbody(Rigidbody targetRb)
    {
        if (targetRb == null) return;
        targetRb.AddExplosionForce(blastForce, transform.position, explosionRadius, 1.0f, ForceMode.Impulse);
    }
}
