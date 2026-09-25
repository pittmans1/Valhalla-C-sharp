using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public static MenuController Instance;
    [Header("UI Screen Sub-Panels")]
    [SerializeField] private GameObject homeScreenPanel;
    [SerializeField] private GameObject gameModePanel;
    [SerializeField] private GameObject lobbySelectPanel;
    [SerializeField] private GameObject settingPanel;

    [Header("Settings Screen UI Controls")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Microhpne voice layout targets")]
    [SerializeField] private Slider selfMicSensitivitySlider;
    [SerializeField] private Slider remoteFriendVolumeSlider;

    [Header("Gatto Sabotage Configuration")]
    [SerializeField] private Toggle preventCatInterferenceToggle;
    [SerializeField] private AudioClip catMeowSassSound;
    [SerializeField] private AudioClip catSwatSwipeSound;

    private bool isProcessingCatCounterAttack = false;
    private int catToggleDefianceCount = 0;
    private bool catisBoredAndGone = false;

    private void Awake ()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Set up clean interface layout default safely on boot
        ShowHomeScreen();
        LoadAndApplyCachedSettings();
    }

    public void ShowHomeScreen() => SetPanelState(homeScreenPanel);

    public void ShowGameModeSelection() => SetPanelState(gameModePanel);


    public void ShowLobbyRoom() => SetPanelState(lobbySelectPanel);

    public void ShowSettingsScreen() => SetPanelState(settingPanel);

    // settings action controller hooks
    public void OnSelfMicSliderChanged(float val)
    {
        if (VoiceChatBridge.Instance !=null)
        {
            VoiceChatBridge.Instance.SetLocalMicSensitivity(val);
        }

        EvaluateCatInterferenceIntervention(selfMicSensitivitySlider, false);
    }

    public void OnRemoteFriendSliderChanged(string PlayerID,float val)
    {
        if (VoiceChatBridge.Instance != null)
        {
            VoiceChatBridge.Instance.AdjustRemotePlayerIncomingVolume(PlayerID, val);
        }

        EvaluateCatInterferenceIntervention(remoteFriendVolumeSlider, true);
    }

    public void OnMasterVolumeChanged(float incomingValue)
    {
        AudioListener.volume = incomingValue;
        EvaluateCatInterferenceIntervention(masterVolumeSlider, true);

        Debug.Log($"Master volume adjusted to: {incomingValue * 100f}%");
    }

    public void OnFullscreenToggled(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
        if(!isFullScreen)
        {
            Screen.SetResolution(1280, 720, false);
        }
    }

    public void OnResolutionChanged(int resolutionIndex)
    {
        Resolution[] availableResolutions = Screen.resolutions;
        if (resolutionIndex == 0) Screen.SetResolution(1920, 1080, Screen.fullScreen);
            else if (resolutionIndex == 1) Screen.SetResolution(1280, 720, Screen.fullScreen);
            else if (resolutionIndex == 2) Screen.SetResolution(2560, 1440, Screen.fullScreen);

        Debug.Log($"available Resolutions {availableResolutions}");
    }

    public void SaveAndCloseSettings ()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveGameData();
        }
    }
    // Loop through canvases cleanly avoiding heavy active panel state overlap errors
    private void SetPanelState(GameObject activePanel)
    {
        if (activePanel == null)
        {
            Debug.LogWarning("Attempted to set a null panel as active. Operation aborted.");
            return;
        }
        homeScreenPanel.SetActive(homeScreenPanel == activePanel);
        gameModePanel.SetActive(gameModePanel == activePanel);
        lobbySelectPanel.SetActive(lobbySelectPanel == activePanel);
        settingPanel.SetActive(settingPanel == activePanel);
    }

    // --- Action Button Hooks ---
    public void SelectModeAndProceed(string choiceMode)
    {
        Debug.Log($"Mode selected globally: {choiceMode}");
        // Example: GlobalGameManager.Instance.SetActiveMode(choiceMode);
        if(global::GameModeManager.Instance != null)
        {
            switch (choiceMode.ToLower())
            {
                case "story":
                    global::GameModeManager.Instance.activeMode = GameModeType.Story;
                    break;
                case "chaospvp":
                    global::GameModeManager.Instance.activeMode = GameModeType.ChaosPvP;
                    break;
                case "bustedmode":
                    global::GameModeManager.Instance.activeMode = GameModeType.BustedMode;
                    break;
                case "coopvsai":
                    global::GameModeManager.Instance.activeMode = GameModeType.CoOpVsAI;
                    break;
                default:
                    Debug.LogWarning($"Unrecognized game mode choice: {choiceMode}. Defaulting to Story mode.");
                    global::GameModeManager.Instance.activeMode = GameModeType.Story;
                    break;
            }
        }
        else
        {
            Debug.LogError("GameModeManager instance is not initialized. Cannot set game mode.");
        }
        ShowLobbyRoom(); // Proceed instantly into character choices next
    }

    private void LoadAndApplyCachedSettings()
    {
        // Fallback default setups on primary boot tracking
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = AudioListener.volume;
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
        }
        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        }
    }
    
    // -- shithead cat interference hahah ---

    public void OnCatProtectionToggleChanged(bool isProtected)
    {
        if(isProcessingCatCounterAttack || catIsBoredAndGone) return;
        if(isProtected)
        {
            catToggleDefianceCount++;
            if (catToggleDefianceCount == 1)
            {
                StartCoroutine(CatSmackToggleBackToOffRoutine());
            }
            else if (catToggleDefianceCount >= 2)
            {
                catIsBoredAndGone = true;
                preventCatInterferenceToggle.interactable = false;
                Text toggleLabel = preventCatInterferenceToggle.GetComponentInChildren<Text>();
                if (toggleLabel != null) toggleLabel.text = "Protect From Cat (Cat Walked Away 💤)";
                Debug.Log("The cat grew tired of your menu games and left to sleep in a cardboard box.");
            }
        }
    }

    private System.Collections.IEnumerator CatSmackToggleBackToOffRoutine()
    {
        isProcessingCatCounterAttack = true;
        yield return new WaitForSeconds(0.4f); // Quick dramatic delay for cat reaction time

        Debug.Log("🐾 CAT REACTION: The cat swatted the protection toggle back to OFF!");
        
        if (AudioManagerHub.Instance != null && catMeowSassSound != null)
            AudioManagerHub.Instance.PlaySpatialExplosiveSFX(catMeowSassSound, Vector3.zero);

        // Force reset checkbox state back to unchecked
        preventCatInterferenceToggle.isOn = false;
        isProcessingCatCounterAttack = false;
    }

    private void EvaluateCatInterferenceIntervention(Slider targetSlider, bool invertScale)
    {
        if (isProcessingCatCounterAttack || catIsBoredAndGone) return;

        // The cat only attacks if protection toggle is currently OFF
        if (!preventCatInterferenceToggle.isOn)
        {
            StartCoroutine(CatSwatSliderOppositeRoutine(targetSlider, invertScale));
        }
    }

    private System.Collections.IEnumerator CatSwatSliderOppositeRoutine(Slider targetSlider, bool invertScale)
    {
        isProcessingCatCounterAttack = true;
        yield return new WaitForSeconds(0.25f); // Tiny delay so you see your change before cat hits it back

        Debug.Log($"🐾 SLIDER SWIPED! Cat swatted {targetSlider.gameObject.name} in the opposite direction!");

        if (AudioManagerHub.Instance != null && catSwatSwipeSound != null)
            AudioManagerHub.Instance.PlaySpatialExplosiveSFX(catSwatSwipeSound, Vector3.zero);

        // Calculate direct opposite side mirror placement value coordinates
        float normalVal = targetSlider.value;
        float oppositeValue = targetSlider.maxValue - (normalVal - targetSlider.minValue);
        
        // Smooth slide it back across the bar to mimic a physical paw push
        float t = 0f;
        float originalValue = targetSlider.value;
        while (t < 1f)
        {
            t += Time.deltaTime * 5f; // Fast swat movement speed
            targetSlider.value = Mathf.Lerp(originalValue, oppositeValue, t);
            yield return null;
        }

        isProcessingCatCounterAttack = false;
    }
     private void InitializeAudioControlSliders()
    {
        // Bind functions directly to slider value changes dynamically
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeSliderChanged);
        if (selfMicSensitivitySlider != null) selfMicSensitivitySlider.onValueChanged.AddListener(OnSelfMicSliderChanged);
        if (remoteFriendVolumeSlider != null) remoteFriendVolumeSlider.onValueChanged.AddListener(OnRemoteFriendSliderChanged);
        
        if (preventCatInterferenceToggle != null) preventCatInterferenceToggle.onValueChanged.AddListener(OnCatProtectionToggleChanged);
    }

    public void QuitApplication()
    {
        Debug.Log("Exiting game application safely.");
        Application.Quit();
    }
}
