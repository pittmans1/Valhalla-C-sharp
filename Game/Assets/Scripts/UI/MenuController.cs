using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Chaos Cat/UI/Menu Controller")]
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

    [Header("Microphone voice layout targets")]
    [SerializeField] private Slider selfMicSensitivitySlider;
    [SerializeField] private Slider remoteFriendVolumeSlider;

    [Header("Gatto Sabotage Configuration")]
    [SerializeField] private Toggle preventCatInterferenceToggle;
    [SerializeField] private AudioClip catMeowSassSound;
    [SerializeField] private AudioClip catSwatSwipeSound;

    private bool isProcessingCatCounterAttack = false;
    private int catToggleDefianceCount = 0;
    private bool catIsBoredAndGone = false;
    public bool catIsntSabotaging = false;

    [Header("Panel Windows")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject lobbyRoomPanel;

    [Header("Main Menu Buttons")]
    [SerializeField] private Button hostLobbyButton;
    [SerializeField] private Button joinLobbyButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Set up clean interface layout default safely on boot
        ShowHomeScreen();
        LoadAndApplyCachedSettings();
        InitializeAudioControlSliders();
        
        // Hook up UI click events securely
        if (hostLobbyButton != null) hostLobbyButton.onClick.AddListener(OnHostLobbyClicked);
        if (joinLobbyButton != null) joinLobbyButton.onClick.AddListener(OnJoinLobbyClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
    }

    public void ShowHomeScreen() => SetPanelState(homeScreenPanel);
    public void ShowGameModeSelection() => SetPanelState(gameModePanel);
    public void ShowLobbyRoom() => SetPanelState(lobbySelectPanel);
    public void ShowSettingsScreen() => SetPanelState(settingPanel);

    // --- Settings Action Controller Hooks ---
    public void OnSelfMicSliderChanged(float val)
    {
        if (VoiceChatBridge.Instance != null)
        {
            VoiceChatBridge.Instance.SetLocalMicSensitivity(val);
        }
        EvaluateCatInterferenceIntervention(selfMicSensitivitySlider, false);
    }

    public void OnRemoteFriendSliderChanged(string PlayerID, float val)
    {
        if (VoiceChatBridge.Instance != null)
        {
            VoiceChatBridge.Instance.AdjustRemotePlayerIncomingVolume(PlayerID, val);
        }
        EvaluateCatInterferenceIntervention(remoteFriendVolumeSlider, true);
    }

    public void OnMasterVolumeSliderChanged(float incomingValue)
    {
        AudioListener.volume = incomingValue;
        EvaluateCatInterferenceIntervention(masterVolumeSlider, true);
        Debug.Log($"Master volume adjusted to: {incomingValue * 100f}%");
    }

    public void OnFullscreenToggled(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
        if (!isFullScreen)
        {
            Screen.SetResolution(1280, 720, false);
        }
    }

    public void OnResolutionChanged(int resolutionIndex)
    {
        if (resolutionIndex == 0) Screen.SetResolution(1920, 1080, Screen.fullScreen);
        else if (resolutionIndex == 1) Screen.SetResolution(1280, 720, Screen.fullScreen);
        else if (resolutionIndex == 2) Screen.SetResolution(2560, 1440, Screen.fullScreen);
    }

    public void SaveAndCloseSettings()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveGameData();
        }
    }

    private void SetPanelState(GameObject activePanel)
    {
        if (activePanel == null)
        {
            Debug.LogWarning("Attempted to set a null panel as active. Operation aborted.");
            return;
        }
        if (homeScreenPanel != null) homeScreenPanel.SetActive(homeScreenPanel == activePanel);
        if (gameModePanel != null) gameModePanel.SetActive(gameModePanel == activePanel);
        if (lobbySelectPanel != null) lobbySelectPanel.SetActive(lobbySelectPanel == activePanel);
        if (settingPanel != null) settingPanel.SetActive(settingPanel == activePanel);
    }

    public void SelectModeAndProceed(string choiceMode)
    {
        Debug.Log($"Mode selected globally: {choiceMode}");
        if (GameModeManager.Instance != null)
        {
            switch (choiceMode.ToLower())
            {
                case "story":
                    GameModeManager.Instance.activeMode = GameModeType.Story;
                    break;
                case "chaospvp":
                    GameModeManager.Instance.activeMode = GameModeType.ChaosPvP;
                    break;
                case "bustedmode":
                    GameModeManager.Instance.activeMode = GameModeType.BustedMode;
                    break;
                case "coopvsai":
                    GameModeManager.Instance.activeMode = GameModeType.CoOpVsAI;
                    break;
                default:
                    Debug.LogWarning($"Unrecognized game mode choice: {choiceMode}. Defaulting to Story.");
                    GameModeManager.Instance.activeMode = GameModeType.Story;
                    break;
            }
        }
        ShowLobbyRoom(); 
    }

    private void LoadAndApplyCachedSettings()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = AudioListener.volume;
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

    // --- Cat Sabotage Engine Subroutines ---
    public void OnCatProtectionToggleChanged(bool isProtected)
    {
        if (isProcessingCatCounterAttack || catIsBoredAndGone) return;
        if (isProtected)
        {
            catToggleDefianceCount++;
            if (catToggleDefianceCount == 1)
            {
                StartCoroutine(CatSmackToggleBackToOffRoutine());
            }
            else if (catToggleDefianceCount >= 2)
            {
                catIsBoredAndGone = true;
                catIsntSabotaging = true;
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
        yield return new WaitForSeconds(0.4f);

        Debug.Log("🐾 CAT REACTION: The cat swatted the protection toggle back to OFF!");
        
        // Dynamic fallback fallback route for both naming patterns
        if (AudioManagerHub.Instance != null && catMeowSassSound != null)
            AudioManagerHub.Instance.PlaySpatialExplosiveSFX(catMeowSassSound, Vector3.zero);

        preventCatInterferenceToggle.isOn = false;
        isProcessingCatCounterAttack = false;
    }

    public void EvaluateCatInterferenceIntervention(Slider targetSlider, bool invertScale)
    {
        if (isProcessingCatCounterAttack || catIsBoredAndGone) return;

        if (preventCatInterferenceToggle != null && !preventCatInterferenceToggle.isOn)
        {
            StartCoroutine(CatSwatSliderOppositeRoutine(targetSlider, invertScale));
        }
    }

    private System.Collections.IEnumerator CatSwatSliderOppositeRoutine(Slider targetSlider, bool invertScale)
    {
        isProcessingCatCounterAttack = true;
        yield return new WaitForSeconds(0.25f);

        Debug.Log($"🐾 SLIDER SWIPED! Cat swatted {targetSlider.gameObject.name} in the opposite direction!");

        if (AudioManagerHub.Instance != null && catSwatSwipeSound != null)
            AudioManagerHub.Instance.PlaySpatialExplosiveSFX(catSwatSwipeSound, Vector3.zero);

        float normalVal = targetSlider.value;
        float oppositeValue = targetSlider.maxValue - (normalVal - targetSlider.minValue);
        
        float t = 0f;
        float originalValue = targetSlider.value;
        while (t < 1f)
        {
            t += Time.deltaTime * 5f; 
            targetSlider.value = Mathf.Lerp(originalValue, oppositeValue, t);
            yield return null;
        }

        isProcessingCatCounterAttack = false;
    }

    private void InitializeAudioControlSliders()
    {
        // Removed duplicated AddListener call for masterVolume here to prevent infinite loop errors
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeSliderChanged);
        if (selfMicSensitivitySlider != null) selfMicSensitivitySlider.onValueChanged.AddListener(OnSelfMicSliderChanged);        
        if (preventCatInterferenceToggle != null) preventCatInterferenceToggle.onValueChanged.AddListener(OnCatProtectionToggleChanged);
    }

    private void OnHostLobbyClicked()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (lobbyRoomPanel != null) lobbyRoomPanel.SetActive(true);
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.InitializeLobbyRoom();
        }
    }
    private void OnJoinLobbyClicked()
    {
        Debug.LogWarning("Online lobby is unavailable until a network provider is configured.");
}
    private void OnQuitClicked()
    {
        Application.Quit();
    }
    private void OnDestroy()
    {
        if (hostLobbyButton != null) hostLobbyButton.onClick.RemoveListener(OnHostLobbyClicked);

        if (joinLobbyButton != null) joinLobbyButton.onClick.RemoveListener(OnJoinLobbyClicked);
        if (quitButton != null) quitButton.onClick.RemoveListener(OnQuitClicked);
    }
    }
