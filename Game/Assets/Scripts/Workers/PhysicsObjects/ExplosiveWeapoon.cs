using UnityEngine;
using System.Collections.Generic;

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

    private bool hasDetonated = false;

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
        if (hasDetonated) return;
        hasDetonated = true;
        Debug.Log($"Weapon Detonated! {gameObject.name} exploded!");

        HashSet<Component> processedTargets = new HashSet<Component>();
        Collider[] targetsInRange = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var target in targetsInRange)
        {
            CatBrainController cat = target.GetComponentInParent<CatBrainController>();
            if (cat != null && processedTargets.Add(cat))
            {
                cat.TakeDamage(baseDamage);
                ApplyBlastForceToRigidbody(cat.GetComponent<Rigidbody>());
            }
            else if (target.GetComponentInParent<HumanBrain>() is HumanBrain human && processedTargets.Add(human))
            {
                human.ExecuteSpray();
                ApplyBlastForceToRigidbody(human.GetComponent<Rigidbody>());
            }
            else if (target.GetComponentInParent<SmashableProp>() is SmashableProp prop && processedTargets.Add(prop))
            {
                prop.ApplyExplosion(transform.position, blastForce, explosionRadius);
            }
        }

        // 2. Play FX Assets
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        Destroy(gameObject); // Purge from runtime memory footprint cleanly
    }

    private void ApplyBlastForceToRigidbody(Rigidbody targetRb)
    {
        if (targetRb == null) return;
        targetRb.AddExplosionForce(blastForce, transform.position, explosionRadius, 1.0f, ForceMode.Impulse);
    }
}
