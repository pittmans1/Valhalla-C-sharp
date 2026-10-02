using UnityEngine;

public enum DestructibleDamageType
{
    Kick,
    Spray,
    Impact,
    Explosion,
    Fall,
    Shove
}

public struct DestructibleHitInfo
{
    public DestructibleDamageType damageType;
    public float force;
    public Vector3 hitPoint;
    public Vector3 hitDirection;
    public GameObject sourceObject;
    public int sourcePlayerId;
    public float damageValue;
}
