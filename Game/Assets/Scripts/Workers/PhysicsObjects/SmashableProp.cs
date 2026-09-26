using UnityEngine;

// REMOVED the namespace block wrapper so it compiles globally with your manager hooks!
public class SmashableProp : MonoBehaviour
{
    [Header("Scoring Profile")]
    [SerializeField] private string itemTypeTag = "Vase";
    [SerializeField] private int pointValue = 150;

    [Header("Physics Limits")]
    [SerializeField] private float breakForceThreshold = 5.0f;
    [SerializeField] private GameObject brokenPrefab;
    
    private bool isBroken = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > breakForceThreshold)
        {
            Break(collision.relativeVelocity.magnitude, null, 0f, 0f);
        }
    }

    public void ApplyExplosion()
    {
        ApplyExplosion(transform.position, 0f, 0f);
    }

    public void ApplyExplosion(Vector3 origin, float blastForce, float radius)
    {
        Break(breakForceThreshold + 1f, origin, blastForce, radius);
    }

    private void Break(float impactForce, Vector3? explosionOrigin, float blastForce, float radius)
    {
        if (isBroken) return;

        isBroken = true;
        Debug.Log($"{itemTypeTag} was completely smashed with a force of {impactForce}!");

        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.OnItemDestroyed(itemTypeTag, pointValue);
        }

        if (brokenPrefab != null)
        {
            GameObject fragments = Instantiate(brokenPrefab, transform.position, transform.rotation);
            fragments.transform.localScale = transform.lossyScale;

            if (explosionOrigin.HasValue && blastForce > 0f && radius > 0f)
            {
                foreach (Rigidbody fragment in fragments.GetComponentsInChildren<Rigidbody>())
                {
                    fragment.AddExplosionForce(blastForce, explosionOrigin.Value, radius, 1f, ForceMode.Impulse);
                }
            }
        }

        Destroy(gameObject);
    }
}
