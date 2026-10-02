using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Serializable]
    private struct ResolutionOption
    {
        public string label;
        public int width;
        public int height;
    }

    [Serializable]
    private struct FrameRateOption
    {
        public string label;
        public int targetFrameRate;
    }

    public static MenuController Instance;
    
    [Header("UI Screen Sub-Panels")]
    [SerializeField] private GameObject homeScreenPanel;
    [SerializeField] private GameObject gameModePanel;
    [SerializeField] private GameObject lobbySelectPanel;
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject catFashionPanel;
    [SerializeField] private GameObject humanFashionPanel;
    [SerializeField] private GameObject storePanel;
    [SerializeField] private GameObject gashaponMachinePanel;

    [Header("Settings Screen UI Controls")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Dropdown resolutionDropdown;
    [SerializeField] private Dropdown frameRateDropdown;
    [SerializeField] private Dropdown graphicsQualityDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Button generalSettingsTab;
    [SerializeField] private Button parentControlsTab;
    [SerializeField] private Button closeSettingsButton;
    [SerializeField] private GameObject generalSettingsPage;
    [SerializeField] private GameObject parentControlsPage;
    [SerializeField] private Button pauseResumeButton;

    [Header("Settings Option Arrays")]
    [SerializeField] private ResolutionOption[] resolutionOptions =
    {
        new ResolutionOption { label = "1920 x 1080", width = 1920, height = 1080 },
        new ResolutionOption { label = "1280 x 720", width = 1280, height = 720 },
        new ResolutionOption { label = "2560 x 1440", width = 2560, height = 1440 }
    };
    [SerializeField] private FrameRateOption[] frameRateOptions =
    {
        new FrameRateOption { label = "30 FPS", targetFrameRate = 30 },
        new FrameRateOption { label = "60 FPS", targetFrameRate = 60 },
        new FrameRateOption { label = "120 FPS", targetFrameRate = 120 },
        new FrameRateOption { label = "Unlimited", targetFrameRate = -1 }
    };

    [Header("Microphone voice layout targets")]
    [SerializeField] private Slider selfMicSensitivitySlider;
    [SerializeField] private Slider remoteFriendVolumeSlider;
    [SerializeField] private Slider mouseLookSensitivitySlider;

    [Header("Gatto Sabotage Configuration")]
    [SerializeField] private Toggle preventCatInterferenceToggle;
    [SerializeField] private AudioClip catMeowSassSound;
    [SerializeField] private AudioClip catSwatSwipeSound;
    [SerializeField] private RectTransform catInterferenceVisual;
    [SerializeField] private GameObject catInterferenceRow;
    [SerializeField] private InputField parentCodeInput;
    [SerializeField] private Text parentControlStatus;
    [SerializeField] private Toggle allowScaryModesToggle;
    [SerializeField] private Toggle autoMuteNonFriendsToggle;
    [SerializeField] private Button unlockParentControlsButton;
    [SerializeField] private Button lockParentControlsButton;
    [SerializeField] private Button changeParentCodeButton;
    private bool parentControlsUnlocked;
    private bool isGameplaySession;
    private bool isPausedBySettings;

    private const string ParentCodeSaltKey = "ParentControlsCodeSalt";
    private const string ParentCodeHashKey = "ParentControlsCodeHash";

    private bool isProcessingCatCounterAttack = false;
    private int catToggleDefianceCount = 0;
    private bool catIsBoredAndGone = false;
    public bool catIsntSabotaging = false;

    [Header("Panel Windows")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject lobbyRoomPanel;

    [Header("Main Menu Buttons")]
    [SerializeField] private Button storyMode;
    [SerializeField] private Button onlineLobby;
    [SerializeField] private Button localCoOp;
    [SerializeField] private Button gasaphonMachine;
    [SerializeField] private Button catFashionHub;
    [SerializeField] private Button humanFashionHub;
    [SerializeField] private Button settings;
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
        SetParentControlsUnlocked(false);
        InitializeAudioControlSliders();
        LoadAndApplyCachedSettings();
        InitializeMainMenuButtonListeners();
        ShowSettingsTab(false);
        
        // Force home screen to be the only active panel on startup
        ShowHomeScreen();
    }

    private void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;

        if (isGameplaySession)
        {
            if (settingPanel != null && settingPanel.activeSelf) CloseSettings();
            else OpenPauseSettings();
        }
        else if (settingPanel != null && settingPanel.activeSelf)
        {
            CloseSettings();
        }
    }

    public void EnterGameplaySession()
    {
        isGameplaySession = true;
        SetCatSabotageAvailable(false);
    }

    private void OpenPauseSettings()
    {
        if (settingPanel == null) return;
        SetCatSabotageAvailable(false);
        Time.timeScale = 0f;
        isPausedBySettings = true;
        settingPanel.SetActive(true);
        if (pauseResumeButton != null) pauseResumeButton.gameObject.SetActive(true);
        ShowSettingsTab(false);
    }

    private void CloseSettings()
    {
        if (settingPanel != null) settingPanel.SetActive(false);
        if (parentControlsUnlocked) LockParentControls();
        if (isPausedBySettings)
        {
            Time.timeScale = 1f;
            isPausedBySettings = false;
        }
        else if (!isGameplaySession)
        {
            ShowHomeScreen();
        }
    }

    private void ShowSettingsTab(bool showParentControls)
    {
        if (!showParentControls && parentControlsUnlocked) LockParentControls();
        if (generalSettingsPage != null) generalSettingsPage.SetActive(!showParentControls);
        if (parentControlsPage != null) parentControlsPage.SetActive(showParentControls);
    }

    /// <summary>
    /// Connects your physical UI button clicks directly to your custom layout functions.
    /// </summary>
    private void InitializeMainMenuButtonListeners()
    {
        // Main Core Menu Buttons Hookups
        if (storyMode != null) storyMode.onClick.AddListener(() => SelectModeAndProceed("story"));
        if (onlineLobby != null) onlineLobby.onClick.AddListener(ShowLobbyRoom);
        if (localCoOp != null) localCoOp.onClick.AddListener(() => SelectModeAndProceed("chaospvp"));
        if (gasaphonMachine != null) gasaphonMachine.onClick.AddListener(showGashaponPanel);
        if (catFashionHub != null) catFashionHub.onClick.AddListener(showCatFashionPanel);
        if (humanFashionHub != null) humanFashionHub.onClick.AddListener(showHumanFashionPanel);
        if (settings != null) settings.onClick.AddListener(ShowSettingsScreen);
        if (generalSettingsTab != null) generalSettingsTab.onClick.AddListener(() => ShowSettingsTab(false));
        if (parentControlsTab != null) parentControlsTab.onClick.AddListener(() => ShowSettingsTab(true));
        if (pauseResumeButton != null) pauseResumeButton.onClick.AddListener(CloseSettings);
        if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(CloseSettings);
        if (unlockParentControlsButton != null) unlockParentControlsButton.onClick.AddListener(UnlockOrCreateParentCode);
        if (lockParentControlsButton != null) lockParentControlsButton.onClick.AddListener(LockParentControls);
        if (changeParentCodeButton != null) changeParentCodeButton.onClick.AddListener(ChangeParentCode);

        // Sub-Screen Action Buttons Hookups
        if (hostLobbyButton != null) hostLobbyButton.onClick.AddListener(OnHostLobbyClicked);
        if (joinLobbyButton != null) joinLobbyButton.onClick.AddListener(OnJoinLobbyClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
    }

    public void ShowHomeScreen() => SetPanelState(homeScreenPanel);
    public void ShowGameModeSelection() => SetPanelState(gameModePanel);
    public void ShowLobbyRoom() => SetPanelState(lobbySelectPanel);
    public void ShowSettingsScreen()
    {
        if (isGameplaySession) OpenPauseSettings();
        else
        {
            SetCatSabotageAvailable(true);
            SetPanelState(settingPanel);
        }
    }
    public void showCatFashionPanel () => SetPanelState(catFashionPanel);
    public void showHumanFashionPanel () => SetPanelState(humanFashionPanel);
    public void showStorePanel () => SetPanelState(storePanel);
    public void showGashaponPanel () => SetPanelState(gashaponMachinePanel);

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

    public void OnMouseLookSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat("HumanLookSensitivity", value);
        if (HumanBrain.Instance != null)
        {
            HumanBrain.Instance.SetPlayerLookSensitivity(value);
        }
        EvaluateCatInterferenceIntervention(mouseLookSensitivitySlider, false);
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
        if (resolutionOptions == null || resolutionOptions.Length == 0) return;
        ResolutionOption option = resolutionOptions[Mathf.Clamp(resolutionIndex, 0, resolutionOptions.Length - 1)];
        Screen.SetResolution(option.width, option.height, Screen.fullScreen);
    }

    public void SaveAndCloseSettings()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveGameData();
        }
    }
    /// <summary>
    /// Dynamic multi-window visibility swapper. Toggles every panel state cleanly.
    /// </summary>
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
        if (catFashionPanel != null) catFashionPanel.SetActive(catFashionPanel == activePanel);
        if (humanFashionPanel != null) humanFashionPanel.SetActive(humanFashionPanel == activePanel);
        if (storePanel != null) storePanel.SetActive(storePanel == activePanel);
        if (gashaponMachinePanel != null) gashaponMachinePanel.SetActive(gashaponMachinePanel == activePanel);
    }

    public void SelectModeAndProceed(string choiceMode)
    {
        if (PlayerPrefs.GetInt("AllowScaryModes", 1) == 0 && IsScaryModeId(choiceMode))
        {
            Debug.LogWarning($"Parent controls blocked scary mode: {choiceMode}");
            return;
        }

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

    private bool IsScaryModeId(string modeId)
    {
        string normalizedId = modeId.ToLowerInvariant();
        return normalizedId.Contains("scary") || normalizedId.Contains("horror") || normalizedId.Contains("nightmare");
    }

    private void LoadAndApplyCachedSettings()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = AudioListener.volume;
        }
        if (mouseLookSensitivitySlider != null)
        {
            mouseLookSensitivitySlider.value = PlayerPrefs.GetFloat("HumanLookSensitivity", 2f);
        }
        if (allowScaryModesToggle != null)
        {
            allowScaryModesToggle.isOn = PlayerPrefs.GetInt("AllowScaryModes", 1) == 1;
        }
        if (autoMuteNonFriendsToggle != null)
        {
            autoMuteNonFriendsToggle.isOn = PlayerPrefs.GetInt("AutoMuteNonFriends", 0) == 1;
            if (VoiceChatBridge.Instance != null)
            {
                VoiceChatBridge.Instance.SetAutoMuteNonFriends(autoMuteNonFriendsToggle.isOn);
            }
        }
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
        }
        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(GetResolutionLabels());
            resolutionDropdown.SetValueWithoutNotify(GetCurrentResolutionIndex());
            resolutionDropdown.RefreshShownValue();
        }
        if (frameRateDropdown != null && frameRateOptions != null && frameRateOptions.Length > 0)
        {
            frameRateDropdown.ClearOptions();
            frameRateDropdown.AddOptions(GetFrameRateLabels());
            int frameRateIndex = GetSavedIndex("FrameRateLimitIndex", 1, frameRateOptions.Length);
            frameRateDropdown.SetValueWithoutNotify(frameRateIndex);
            frameRateDropdown.RefreshShownValue();
            frameRateDropdown.onValueChanged.AddListener(ApplyFrameRateLimit);
            ApplyFrameRateLimit(frameRateIndex);
        }
        if (graphicsQualityDropdown != null && QualitySettings.names.Length > 0)
        {
            graphicsQualityDropdown.ClearOptions();
            graphicsQualityDropdown.AddOptions(new List<string>(QualitySettings.names));
            int qualityIndex = GetSavedIndex("GraphicsQualityIndex", QualitySettings.GetQualityLevel(), QualitySettings.names.Length);
            graphicsQualityDropdown.SetValueWithoutNotify(qualityIndex);
            graphicsQualityDropdown.RefreshShownValue();
            graphicsQualityDropdown.onValueChanged.AddListener(ApplyGraphicsQuality);
            ApplyGraphicsQuality(qualityIndex);
        }
    }

    // --- Cat Sabotage Engine Subroutines ---
    public void OnCatProtectionToggleChanged(bool isProtected)
    {
        if (isGameplaySession || isProcessingCatCounterAttack || catIsBoredAndGone) return;
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
        if (isGameplaySession || isProcessingCatCounterAttack || catIsBoredAndGone) return;

        if (preventCatInterferenceToggle != null && !preventCatInterferenceToggle.isOn)
        {
            StartCoroutine(CatSwatSliderOppositeRoutine(targetSlider, invertScale));
        }
    }

    private System.Collections.IEnumerator CatSwatSliderOppositeRoutine(Slider targetSlider, bool invertScale)
    {
        isProcessingCatCounterAttack = true;
        yield return new WaitForSecondsRealtime(0.25f);
        if (isGameplaySession)
        {
            isProcessingCatCounterAttack = false;
            yield break;
        }

        yield return AnimateCatInterference(targetSlider, true);

        Debug.Log($"🐾 SLIDER SWIPED! Cat swatted {targetSlider.gameObject.name} in the opposite direction!");

        if (AudioManagerHub.Instance != null && catSwatSwipeSound != null)
            AudioManagerHub.Instance.PlaySpatialExplosiveSFX(catSwatSwipeSound, Vector3.zero);

        float normalVal = targetSlider.value;
        float oppositeValue = targetSlider.maxValue - (normalVal - targetSlider.minValue);
        
        float t = 0f;
        float originalValue = targetSlider.value;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 5f; 
            targetSlider.value = Mathf.Lerp(originalValue, oppositeValue, t);
            yield return null;
        }

        yield return AnimateCatInterference(targetSlider, false);

        isProcessingCatCounterAttack = false;
    }

    private void InitializeAudioControlSliders()
    {
        // Removed duplicated AddListener call for masterVolume here to prevent infinite loop errors
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeSliderChanged);
        if (selfMicSensitivitySlider != null) selfMicSensitivitySlider.onValueChanged.AddListener(OnSelfMicSliderChanged);        
        if (mouseLookSensitivitySlider != null) mouseLookSensitivitySlider.onValueChanged.AddListener(OnMouseLookSensitivityChanged);
        if (preventCatInterferenceToggle != null) preventCatInterferenceToggle.onValueChanged.AddListener(OnCatProtectionToggleChanged);
        if (allowScaryModesToggle != null) allowScaryModesToggle.onValueChanged.AddListener(OnAllowScaryModesChanged);
        if (autoMuteNonFriendsToggle != null) autoMuteNonFriendsToggle.onValueChanged.AddListener(OnAutoMuteNonFriendsChanged);
        if (autoMuteNonFriendsToggle != null && VoiceChatBridge.Instance != null)
        {
            VoiceChatBridge.Instance.SetAutoMuteNonFriends(autoMuteNonFriendsToggle.isOn);
        }
    }

    private void SetCatSabotageAvailable(bool available)
    {
        if (catInterferenceRow != null) catInterferenceRow.SetActive(available);
        if (preventCatInterferenceToggle != null)
        {
            preventCatInterferenceToggle.interactable = available && !catIsBoredAndGone;
        }
        if (!available && catInterferenceVisual != null)
        {
            catInterferenceVisual.gameObject.SetActive(false);
        }
    }

    private void UnlockOrCreateParentCode()
    {
        string enteredCode = parentCodeInput.text.Trim();
        string savedHash = PlayerPrefs.GetString(ParentCodeHashKey, string.Empty);
        if (string.IsNullOrEmpty(savedHash))
        {
            if (!SaveParentCode(enteredCode)) return;
            SetParentControlsUnlocked(true);
            parentControlStatus.text = "Parent code created";
            parentCodeInput.text = string.Empty;
            return;
        }

        string savedSalt = PlayerPrefs.GetString(ParentCodeSaltKey, string.Empty);
        if (HashParentCode(savedSalt, enteredCode) == savedHash)
        {
            SetParentControlsUnlocked(true);
            parentControlStatus.text = "Controls unlocked";
            parentCodeInput.text = string.Empty;
        }
        else
        {
            parentControlStatus.text = "Incorrect code";
        }
    }

    private void LockParentControls()
    {
        parentCodeInput.text = string.Empty;
        SetParentControlsUnlocked(false);
    }

    private void ChangeParentCode()
    {
        if (!parentControlsUnlocked) return;
        if (!SaveParentCode(parentCodeInput.text.Trim())) return;
        parentCodeInput.text = string.Empty;
        parentControlStatus.text = "Parent code changed";
    }

    private bool SaveParentCode(string code)
    {
        if (code.Length < 4)
        {
            parentControlStatus.text = "Code must be at least 4 characters";
            return false;
        }

        byte[] saltBytes = new byte[16];
        using (RandomNumberGenerator random = RandomNumberGenerator.Create())
        {
            random.GetBytes(saltBytes);
        }

        string salt = Convert.ToBase64String(saltBytes);
        PlayerPrefs.SetString(ParentCodeSaltKey, salt);
        PlayerPrefs.SetString(ParentCodeHashKey, HashParentCode(salt, code));
        PlayerPrefs.Save();
        return true;
    }

    private string HashParentCode(string salt, string code)
    {
        using (SHA256 sha = SHA256.Create())
        {
            byte[] saltedCode = Encoding.UTF8.GetBytes(salt + code);
            return Convert.ToBase64String(sha.ComputeHash(saltedCode));
        }
    }

    private void SetParentControlsUnlocked(bool unlocked)
    {
        parentControlsUnlocked = unlocked;
        if (allowScaryModesToggle != null) allowScaryModesToggle.interactable = unlocked;
        if (autoMuteNonFriendsToggle != null) autoMuteNonFriendsToggle.interactable = unlocked;
        if (unlockParentControlsButton != null) unlockParentControlsButton.gameObject.SetActive(!unlocked);
        if (lockParentControlsButton != null) lockParentControlsButton.gameObject.SetActive(unlocked);
        if (changeParentCodeButton != null) changeParentCodeButton.gameObject.SetActive(unlocked);
        if (parentControlStatus != null) parentControlStatus.text = unlocked ? "Controls unlocked" : "Controls locked";
    }

    private void OnAllowScaryModesChanged(bool allowed)
    {
        if (!parentControlsUnlocked) return;
        PlayerPrefs.SetInt("AllowScaryModes", allowed ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnAutoMuteNonFriendsChanged(bool enabled)
    {
        if (!parentControlsUnlocked) return;
        PlayerPrefs.SetInt("AutoMuteNonFriends", enabled ? 1 : 0);
        PlayerPrefs.Save();
        if (VoiceChatBridge.Instance != null)
        {
            VoiceChatBridge.Instance.SetAutoMuteNonFriends(enabled);
        }
    }

    private int GetCurrentResolutionIndex()
    {
        Resolution resolution = Screen.currentResolution;
        if (resolutionOptions == null) return 0;
        for (int i = 0; i < resolutionOptions.Length; i++)
        {
            if (resolutionOptions[i].width == resolution.width && resolutionOptions[i].height == resolution.height) return i;
        }
        return 0;
    }

    private List<string> GetResolutionLabels()
    {
        List<string> labels = new List<string>();
        if (resolutionOptions == null) return labels;
        foreach (ResolutionOption option in resolutionOptions) labels.Add(option.label);
        return labels;
    }

    private List<string> GetFrameRateLabels()
    {
        List<string> labels = new List<string>();
        if (frameRateOptions == null) return labels;
        foreach (FrameRateOption option in frameRateOptions) labels.Add(option.label);
        return labels;
    }

    private int GetSavedIndex(string key, int defaultValue, int optionCount)
    {
        return Mathf.Clamp(PlayerPrefs.GetInt(key, defaultValue), 0, optionCount - 1);
    }

    private void ApplyFrameRateLimit(int index)
    {
        if (frameRateOptions == null || frameRateOptions.Length == 0) return;
        int selectedIndex = Mathf.Clamp(index, 0, frameRateOptions.Length - 1);
        Application.targetFrameRate = frameRateOptions[selectedIndex].targetFrameRate;
        PlayerPrefs.SetInt("FrameRateLimitIndex", selectedIndex);
    }

    private void ApplyGraphicsQuality(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
        PlayerPrefs.SetInt("GraphicsQualityIndex", index);
    }

    private System.Collections.IEnumerator AnimateCatInterference(Slider targetSlider, bool entering)
    {
        if (catInterferenceVisual == null || targetSlider == null) yield break;

        RectTransform panelRect = settingPanel.GetComponent<RectTransform>();
        Vector2 targetPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRect,
            targetSlider.GetComponent<RectTransform>().position, null, out targetPosition);
        targetPosition.x += entering ? 70f : panelRect.rect.width * 0.5f + 100f;
        Vector2 startPosition = entering
            ? new Vector2(panelRect.rect.width * 0.5f + 100f, targetPosition.y)
            : catInterferenceVisual.anchoredPosition;
        Vector2 endPosition = entering
            ? new Vector2(targetPosition.x, targetPosition.y)
            : new Vector2(panelRect.rect.width * 0.5f + 100f, targetPosition.y);

        if (entering) catInterferenceVisual.gameObject.SetActive(true);
        float elapsed = 0f;
        while (elapsed < 0.22f)
        {
            elapsed += Time.unscaledDeltaTime;
            catInterferenceVisual.anchoredPosition = Vector2.Lerp(startPosition, endPosition, elapsed / 0.22f);
            yield return null;
        }
        catInterferenceVisual.anchoredPosition = endPosition;
        if (!entering) catInterferenceVisual.gameObject.SetActive(false);
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
        if (storyMode != null) storyMode.onClick.RemoveAllListeners();
        if (onlineLobby != null) onlineLobby.onClick.RemoveAllListeners();
        if (localCoOp != null) localCoOp.onClick.RemoveAllListeners();if (catFashionHub != null) catFashionHub.onClick.RemoveAllListeners();
        if (humanFashionHub != null) humanFashionHub.onClick.RemoveAllListeners();
        if (settings != null) settings.onClick.RemoveAllListeners();if (hostLobbyButton != null) hostLobbyButton.onClick.RemoveAllListeners();
        if (joinLobbyButton != null) joinLobbyButton.onClick.RemoveAllListeners();
        if (quitButton != null) quitButton.onClick.RemoveAllListeners();
    }
}
