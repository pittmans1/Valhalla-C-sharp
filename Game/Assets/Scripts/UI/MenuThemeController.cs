using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class ThemeStyleData
{
    public string themeName = "LivingRoom";
    public Transform cameraTripodAnchor; // Drag your 'Cam_Anchor_Node' out of each map folder here
    public TMP_FontAsset themeFontAsset; // Link the custom font asset that matches this map's personality
}

public class MenuThemeController : MonoBehaviour
{
    public static MenuThemeController Instance { get; private set; }

    [Header("Active Level Theme Sequence Loop")]
    [SerializeField] private List<ThemeStyleData> availableThemes = new List<ThemeStyleData>();
    [SerializeField] private List<TextMeshProUGUI> menuButtonTexts = new List<TextMeshProUGUI>();

    [Header("Transition Settings")]
    [SerializeField] private float timePerThemeStation = 10.0f; // Time in seconds before swapping maps

    private int activeIndex = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Automatically scan and find all text elements inside your buttons group on frame 1
        FindAllMenuButtonTextComponents();

        // Enforce your starting level theme profile immediately on boot
        if (availableThemes.Count > 0)
        {
            ApplyTargetThemeProfile(availableThemes[activeIndex]);
            StartCoroutine(ExecuteThemeRotationLoopRoutine());
        }
    }

    private void FindAllMenuButtonTextComponents()
    {
        menuButtonTexts.Clear();
        
        // Grab all TextMeshPro elements inside the active UI Canvas
        TextMeshProUGUI[] foundTexts = GameObject.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (var txt in foundTexts)
        {
            // Verify the text is a child of one of your button elements
            if (txt.gameObject.transform.parent != null && txt.gameObject.transform.parent.gameObject.GetComponent<UnityEngine.UI.Button>() != null)
            {
                menuButtonTexts.Add(txt);
            }
        }
    }

    private IEnumerator ExecuteThemeRotationLoopRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(timePerThemeStation);

            // Cycle index securely across your total configured available maps
            activeIndex = (activeIndex + 1) % availableThemes.Count;
            
            // Swap background camera angles and morph typography instantly!
            ApplyTargetThemeProfile(availableThemes[activeIndex]);
        }
    }

    public void ApplyTargetThemeProfile(ThemeStyleData profile)
    {
        if (profile == null) return;

        // Teleport the main game camera straight onto your level's tripod node anchor position coordinates
        Camera mainCam = Camera.main;
        if (mainCam != null && profile.cameraTripodAnchor != null)
        {
            mainCam.transform.position = profile.cameraTripodAnchor.position;
            mainCam.transform.rotation = profile.cameraTripodAnchor.rotation;
        }

        // Loop through your button layout list array and swap their fonts instantly
        if (profile.themeFontAsset != null)
        {
            foreach (var buttonText in menuButtonTexts)
            {
                if (buttonText != null)
                {
                    buttonText.font = profile.themeFontAsset;
                    // buttonText.UpdateMeshPixelsData(); // Forces TextMeshPro to redraw text geometry instantly with no lags
                }
            }
        }

        Debug.Log($"[MenuThemeController] Menu theme successfully transitioned to layout style: {profile.themeName}");
    }
}
