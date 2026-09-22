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
        if (isBroken) return;

        if (collision.relativeVelocity.magnitude > breakForceThreshold)
        {
            isBroken = true;
            Debug.Log($"{itemTypeTag} was completely smashed with a force of {collision.relativeVelocity.magnitude}!");
            
            // Back to a clean, standard, reliable single-line manager check!
            if (GameModeManager.Instance != null)
            {
                GameModeManager.Instance.OnItemDestroyed(itemTypeTag, pointValue);
            }
            
            Destroy(gameObject, 0.1f);
        }
    }
}
