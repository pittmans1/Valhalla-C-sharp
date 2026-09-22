using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("UI Screen Sub-Panels")]
    [SerializeField] private GameObject homeScreenPanel;
    [SerializeField] private GameObject gameModePanel;
    [SerializeField] private GameObject lobbySelectPanel;
    [SerializeField] private GameObject settingPanel;

    private void Start()
    {
        // Set up clean interface layout default safely on boot
        ShowHomeScreen();
    }

    public void ShowHomeScreen()
    {
        SetPanelState(homeScreenPanel);
    }

    public void ShowGameModeSelection()
    {
        SetPanelState(gameModePanel);
    }

    public void ShowLobbyRoom()
    {
        SetPanelState(lobbySelectPanel);
    }

    public void ShowSettingsScreen()
    {
        SetPanelState(settingPanel);
    }

    // Loop through canvases cleanly avoiding heavy active panel state overlap errors
    private void SetPanelState(GameObject activePanel)
    {
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
        
        ShowLobbyRoom(); // Proceed instantly into character choices next
    }

    public void QuitApplication()
    {
        Debug.Log("Exiting game application safely.");
        Application.Quit();
    }
}
