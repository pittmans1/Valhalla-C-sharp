using UnityEngine;

public class CatPaws : MonoBehaviour
{
    [Header("Swipe Attack Settings")]
    [SerializeField] private float swipeForce = 5.0f;
    [SerializeField] private float swipeCooldown = 1.0f;
    private bool canSwipe = true;

    public void Swipe()
    {
        if (canSwipe)
        {
            // Implement swipe logic here, e.g., apply force to nearby objects
            Debug.Log("Cat swipes with force: " + swipeForce);
            StartCoroutine(SwipeCooldown());
        }
    }

    private System.Collections.IEnumerator SwipeCooldown()
    {
        canSwipe = false;
        yield return new WaitForSeconds(swipeCooldown);
        canSwipe = true;
    }
}