using UnityEngine;

public static class DamageResolver
{
    public static void Resolve(SmashableProp prop, float force, DestructibleDamageType damageType, Vector3 hitPoint, Vector3 hitDirection, GameObject sourceObject = null, int sourcePlayerId = -1)
    {
        if (prop == null || force <= 0f)
        {
            return;
        }

        if (prop.IsBroken)
        {
            return;
        }

        if (force < prop.EffectiveBreakThreshold)
        {
            if (DestructionEventBus.Instance != null)
            {
                DestructionEventBus.Instance.PublishPropDamaged(prop, damageType, force, hitPoint, hitDirection, sourceObject, sourcePlayerId);
            }

            return;
        }

        prop.TriggerBreak(damageType, force, hitPoint, hitDirection, sourceObject, sourcePlayerId);
    }
}
