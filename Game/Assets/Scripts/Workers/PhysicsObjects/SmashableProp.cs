using UnityEngine;

// REMOVED the namespace block wrapper so it compiles globally with your manager hooks!
public class SmashableProp : MonoBehaviour
{
    [Header("Scoring Profile")]
    [SerializeField] private string itemTypeTag = "Vase";
    [SerializeField] private int pointValue = 150;

    [Header("Physics Limits")]
    [SerializeField] private float breakForceThreshold = 5.0f;
    
    private bool isBroken = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > breakForceThreshold)
        {
            Break(collision.relativeVelocity.magnitude);
        }
    }

    public void ApplyExplosion()
    {
        Break(breakForceThreshold + 1f);
    }

    private void Break(float force)
    {
        if (isBroken) return;

        isBroken = true;
        Debug.Log($"{itemTypeTag} was completely smashed with a force of {force}!");

        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.OnItemDestroyed(itemTypeTag, pointValue);
        }

        Destroy(gameObject, 0.1f);
    }
}
