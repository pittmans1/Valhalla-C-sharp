using System.Collections.Generic;
using UnityEngine;


[AddComponentMenu("Chaos Cat/Workers/Cat/Cat Paws")]
public class CatPaws : MonoBehaviour
{
    [Header("Swipe Attack Settings")]
    [SerializeField] private float swipeForce = 5.0f;
    [SerializeField] private float swipeCooldown = 1.0f;
    
    [Header("UI Sabotage Configurations")]
    [SerializeField] private float sliderSwatSpeed = 0.5f;

    private bool canSwipe = true;
    private bool hasSmackedProtectToggle = false;

    #region GAMEPLAY: PHYSICAL SWIPE SYSTEM
    
    public void Swipe()
    {
        if (canSwipe)
        {
            // Physical world-space swipe logic (props, weapons, humans)
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

    #endregion

    #region UI LOBBY: SABOTAGE ENGINE

    /// <summary>
    /// Process system panel interference rules when the Cat AI overlaps with lobby configurations.
    /// </summary>
    public void ProcessMenuSabotage(VoiceRowUI targetRow, float currentSliderValue, System.Action<float> onSliderValueSwatted)
    {
        if (targetRow == null) return;

        // Rule 1: Handle the "Protect From Cat" Toggle behavior
        if (targetRow.isProtectedFromCatActive)
        {
            if (!hasSmackedProtectToggle && canSwipe)
            {
                ExecuteInstantToggleSmack(targetRow);
            }
            return; // Cat instantly wanders off bored after one smack
        }

        // Reset the smack tracker if a human player turns protection back off manually
        hasSmackedProtectToggle = false;

        // Rule 2: Smoothly swat the target microphone volume level sliders backward
        if (currentSliderValue > 0f)
        {
            ExecuteSmoothSliderSwat(currentSliderValue, onSliderValueSwatted);
        }
    }

    private void ExecuteInstantToggleSmack(VoiceRowUI targetRow)
    {
        Debug.Log("[CatPaws] Protect From Cat detected! Executing instant override smack.");
        
        // Force toggle state down instantly via non-notifying setters
        // targetRow.SabotageForceToggle(false);
        hasSmackedProtectToggle = true;

        // Use the existing physical cooldown routine to lock out rapid animations
        StartCoroutine(SwipeCooldown());
    }

    private void ExecuteSmoothSliderSwat(float currentValue, System.Action<float> updateCallback)
    {
        // Calculate a regression value over physical delta frames
        float newSliderValue = Mathf.Max(0f, currentValue - (Time.deltaTime * sliderSwatSpeed));
        
        // Send updated float data stream back through the UI event wrapper channel
        updateCallback?.Invoke(newSliderValue);
    }

    #endregion
}
