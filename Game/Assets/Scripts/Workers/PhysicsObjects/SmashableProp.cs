using UnityEngine;

public class SmashableProp : MonoBehaviour
{
    [Header("Scoring Profile")]
    [SerializeField] private string itemTypeTag = "Vase";
    [SerializeField] private int pointValue = 150;

    [Header("Physics Limits")]
    [SerializeField] private float breakForceThreshold = 5.0f;
    [SerializeField] private float damageResistance = 0f;
    [SerializeField] private GameObject brokenPrefab;
    [SerializeField] private PropFragmentPool fragmentPool;

    public string ItemTypeTag => itemTypeTag;
    public int PointValue => pointValue;
    public float EffectiveBreakThreshold => breakForceThreshold + damageResistance;
    public bool IsBroken => isBroken;

    private bool isBroken = false;
    private DestructiblePropRegistry registry;

    private void Awake()
    {
        registry = DestructiblePropRegistry.Instance;
        if (registry != null)
        {
            registry.RegisterProp(this);
            fragmentPool = registry.FragmentPool;
        }
    }

    private void OnEnable()
    {
        if (registry == null)
        {
            registry = DestructiblePropRegistry.Instance;
        }

        if (registry != null)
        {
            registry.RegisterProp(this);
            fragmentPool ??= registry.FragmentPool;
        }
    }

    private void OnDisable()
    {
        if (registry != null)
        {
            registry.UnregisterProp(this);
        }
    }

    private void OnDestroy()
    {
        if (registry != null)
        {
            registry.UnregisterProp(this);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isBroken)
        {
            return;
        }

        float impactMagnitude = collision.relativeVelocity.magnitude;
        if (impactMagnitude > breakForceThreshold)
        {
            Vector3 hitPoint = collision.contacts.Length > 0 ? collision.GetContact(0).point : transform.position;
            DamageResolver.Resolve(this, impactMagnitude, DestructibleDamageType.Impact, hitPoint, collision.relativeVelocity.normalized, collision.collider.gameObject);
        }
    }

    public void ApplyDamage(float force, DestructibleDamageType damageType = DestructibleDamageType.Impact, Vector3? hitPoint = null, Vector3? hitDirection = null, GameObject sourceObject = null, int sourcePlayerId = -1)
    {
        if (isBroken || force <= 0f)
        {
            return;
        }

        DamageResolver.Resolve(this, force, damageType, hitPoint ?? transform.position, hitDirection ?? Vector3.up, sourceObject, sourcePlayerId);
    }

    public void ApplyExplosion()
    {
        ApplyExplosion(transform.position, 0f, 0f);
    }

    public void ApplyExplosion(Vector3 origin, float blastForce, float radius)
    {
        float force = breakForceThreshold + 1f + Mathf.Max(blastForce, 0f);
        DamageResolver.Resolve(this, force, DestructibleDamageType.Explosion, origin, (transform.position - origin).normalized, gameObject);
    }

    public void TriggerBreak(DestructibleDamageType damageType, float force, Vector3 hitPoint, Vector3 hitDirection, GameObject sourceObject = null, int sourcePlayerId = -1)
    {
        if (isBroken)
        {
            return;
        }

        isBroken = true;
        Debug.Log($"{itemTypeTag} was completely smashed with a force of {force} via {damageType}!");

        if (DestructionEventBus.Instance != null)
        {
            DestructionEventBus.Instance.PublishPropDestroyed(this, damageType, force, hitPoint, hitDirection, sourceObject, sourcePlayerId);
        }
        else if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.OnItemDestroyed(itemTypeTag, pointValue);
        }

        if (DestructiblePropRegistry.Instance != null)
        {
            DestructiblePropRegistry.Instance.NotifyDestroyed(this, new DestructibleHitInfo
            {
                damageType = damageType,
                force = force,
                hitPoint = hitPoint,
                hitDirection = hitDirection,
                sourceObject = sourceObject,
                sourcePlayerId = sourcePlayerId,
                damageValue = force
            });
        }

        if (brokenPrefab != null)
        {
            GameObject fragments = fragmentPool != null
                ? fragmentPool.GetFragments(brokenPrefab, transform.position, transform.rotation)
                : Instantiate(brokenPrefab, transform.position, transform.rotation);

            if (fragments != null)
            {
                fragments.transform.localScale = transform.lossyScale;

                if (damageType == DestructibleDamageType.Explosion)
                {
                    foreach (Rigidbody fragment in fragments.GetComponentsInChildren<Rigidbody>())
                    {
                        fragment.AddExplosionForce(force * 2f, hitPoint, 8f, 1f, ForceMode.Impulse);
                    }
                }

                if (fragmentPool != null)
                {
                    fragmentPool.ReturnFragments(fragments, 4f);
                }
            }
        }

        Destroy(gameObject);
    }
}
