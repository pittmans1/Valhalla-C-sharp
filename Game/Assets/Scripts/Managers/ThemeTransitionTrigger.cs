using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class MenuThemeProfile
{
    public string themeName;
    [Header("Camera Spatial Target")]
    public Transform cameraRigAnchorPoint; // Drag an empty GameObject positioned perfectly in each map station here
    
    [Header("Theme Typography")]
    public TMP_FontAsset themeFontAsset; // Assign the unique font matching this level's style
}

public class ThemeTransitionTrigger : MonoBehaviour
{
    public static ThemeTransitionTrigger Instance { get; private set; }

    [Header("Theme Profile Sequences")]
    [SerializeField] private List<MenuThemeProfile> themeProfiles = new List<MenuThemeProfile>();
    [SerializeField] private float timePerThemeStation = 12.0f; // How long a map stays active before switching
    [SerializeField] private float cameraFadeSpeed = 2.0f;

    [Header("Required Engine Connections")]
    [SerializeField] private Transform menuMainCameraTransform;
    [SerializeField] private CanvasGroup menuUIPanelCanvasGroup; // Drag your master buttons panel here to fade them out smoothly during travel
    [SerializeField] private List<TextMeshProUGUI> targetedMenuTextsToUpdate = new List<TextMeshProUGUI>(); // Drag all button texts here

    private int currentThemeIndex = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Enforce base theme settings on entry frame
        if (themeProfiles.Count > 0)
        {
            ApplyThemeProfileInstant(themeProfiles[0]);
            StartCoroutine(ExecuteThemeRotationLoopRoutine());
        }
    }

    private IEnumerator ExecuteThemeRotationLoopRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(timePerThemeStation);

            // Step 1: Fade out active canvas screens to prepare for travel
            float elapsed = 0f;
            while (elapsed < 0.4f)
            {
                elapsed += Time.deltaTime;
                if (menuUIPanelCanvasGroup != null)
                {
                    menuUIPanelCanvasGroup.alpha = 1.0f - (elapsed / 0.4f);
                }
                yield return null;
            }

            // Step 2: Calculate and cycle index parameters securely
            currentThemeIndex = (currentThemeIndex + 1) % themeProfiles.Count;
            MenuThemeProfile nextTheme = themeProfiles[currentThemeIndex];

            // Step 3: Shift camera coordinates and swap fonts securely
            ApplyThemeProfileInstant(nextTheme);

            // Step 4: Fade menu screens back up to full visibility smoothly
            elapsed = 0f;
            while (elapsed < 0.4f)
            {
                elapsed += Time.deltaTime;
                if (menuUIPanelCanvasGroup != null)
                {
                    menuUIPanelCanvasGroup.alpha = elapsed / 0.4f;
                }
                yield return null;
            }
        }
    }

    private void ApplyThemeProfileInstant(MenuThemeProfile profile)
    {
        if (profile == null) return;

        // Teleport the main camera straight to the target map coordinate anchor node position
        if (menuMainCameraTransform != null && profile.cameraRigAnchorPoint != null)
        {
            menuMainCameraTransform.position = profile.cameraRigAnchorPoint.position;
            menuMainCameraTransform.rotation = profile.cameraRigAnchorPoint.rotation;
        }

        // Loop through all assigned UI text tracking slots and rewrite their font styles instantly
        if (profile.themeFontAsset != null)
        {
            foreach (TextMeshProUGUI textElement in targetedMenuTextsToUpdate)
            {
                if (textElement != null)
                {
                    textElement.font = profile.themeFontAsset;
                    // textElement.UpdateMeshPixelsData(); // Forces TextMeshPro to redraw cleanly with zero delays
                }
            }
        }

        Debug.Log($"[ThemeTransitionTrigger] Menu theme successfully cycled to style layout: {profile.themeName}");
    }
}
