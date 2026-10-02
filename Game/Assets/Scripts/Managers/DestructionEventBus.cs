using System;
using UnityEngine;

public struct DestructionEvent
{
    public SmashableProp Prop;
    public DestructibleDamageType DamageType;
    public float Force;
    public Vector3 HitPoint;
    public Vector3 HitDirection;
    public GameObject SourceObject;
    public int SourcePlayerId;
    public float DamageValue;
    public string ItemTypeTag;
    public int PointValue;
}

public class DestructionEventBus : MonoBehaviour
{
    public static DestructionEventBus Instance { get; private set; }

    public event Action<DestructionEvent> PropDestroyed;
    public event Action<DestructionEvent> PropDamaged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            return;
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void PublishPropDamaged(SmashableProp prop, DestructibleDamageType damageType, float force, Vector3 hitPoint, Vector3 hitDirection, GameObject sourceObject = null, int sourcePlayerId = -1)
    {
        if (prop == null)
        {
            return;
        }

        var evt = new DestructionEvent
        {
            Prop = prop,
            DamageType = damageType,
            Force = force,
            HitPoint = hitPoint,
            HitDirection = hitDirection,
            SourceObject = sourceObject,
            SourcePlayerId = sourcePlayerId,
            DamageValue = force,
            ItemTypeTag = prop.ItemTypeTag,
            PointValue = prop.PointValue
        };

        PropDamaged?.Invoke(evt);
    }

    public void PublishPropDestroyed(SmashableProp prop, DestructibleDamageType damageType, float force, Vector3 hitPoint, Vector3 hitDirection, GameObject sourceObject = null, int sourcePlayerId = -1)
    {
        if (prop == null)
        {
            return;
        }

        var evt = new DestructionEvent
        {
            Prop = prop,
            DamageType = damageType,
            Force = force,
            HitPoint = hitPoint,
            HitDirection = hitDirection,
            SourceObject = sourceObject,
            SourcePlayerId = sourcePlayerId,
            DamageValue = force,
            ItemTypeTag = prop.ItemTypeTag,
            PointValue = prop.PointValue
        };

        PropDestroyed?.Invoke(evt);
    }
}
