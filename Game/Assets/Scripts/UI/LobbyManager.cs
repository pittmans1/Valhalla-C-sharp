using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class LobbyPlayerSlot
{
    public string playerID;
    public string playerName;
    public bool isReady;
    public int selectedCatIndex;
}

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;

    [Header("Lobby UI Elements")]
    [SerializeField] private Text roomCodeDisplayText;
    [SerializeField] private Toggle privateMatchToggle;
    [SerializeField] private Transform playerListContainer;
    [SerializeField] private GameObject playerSlotPrefabTemplate; // UI row asset card

    [Header("Content Databases")]
    [SerializeField] private List<CatProfileData> availableCatBreedsPool = new List<CatProfileData>();

    // Running tracking data caches
    private List<LobbyPlayerSlot> connectedPlayers = new List<LobbyPlayerSlot>();
    public bool IsPrivateRoom { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (privateMatchToggle != null)
        {
            privateMatchToggle.isOn = false;
            privateMatchToggle.interactable = false;
        }

        // Automatically default initialize a host slot for the local user on window launch
        AddPlayerToLobbySlot("LocalHost_1", "Player 1 (You)");
    }

    /// <summary>
    /// Called by MenuController to populate lobby tracks cleanly when hosting a match session.
    /// </summary>
    public void InitializeLobbyRoom()
    {
        // Wipe old player lists to avoid stacking duplicate users every time you open the screen
        connectedPlayers.Clear();

        // Offline local slot only; no remote players are fabricated.
        AddPlayerToLobbySlot("LocalHost_1", "You (Player 1)");
        OnCreateMatchPressed();
    }

    // --- 🌐 MATCH TOKEN & ROOM CREATION CODES ---

    public void OnCreateMatchPressed()
    {
        IsPrivateRoom = false;

        if (roomCodeDisplayText != null) 
            roomCodeDisplayText.text = "LOCAL SESSION (OFFLINE)";
            
        UpdateLobbyDisplayUI();
    }

    // --- 🎮 COUCH CO-OP & NETWORK PLAYER JOINING LOGIC ---

    public void AddPlayerToLobbySlot(string generatedID, string displayName)
    {
        // Block duplicate register entries
        if (connectedPlayers.Exists(x => x.playerID == generatedID)) return;

        LobbyPlayerSlot newSlot = new LobbyPlayerSlot
        {
            playerID = generatedID,
            playerName = displayName,
            isReady = false,
            selectedCatIndex = 0 // Defaults cleanly onto the first asset profile index
        };

        connectedPlayers.Add(newSlot);
        UpdateLobbyDisplayUI();
    }

    public void CycleSelectedCatCharacter(string playerID, int directionForwardOrBackward)
    {
        LobbyPlayerSlot targetSlot = connectedPlayers.Find(x => x.playerID == playerID);
        if (targetSlot == null || availableCatBreedsPool.Count == 0) return;

        int nextIndex = targetSlot.selectedCatIndex + directionForwardOrBackward;
        if (nextIndex >= availableCatBreedsPool.Count) nextIndex = 0;
        if (nextIndex < 0) nextIndex = availableCatBreedsPool.Count - 1;

        targetSlot.selectedCatIndex = nextIndex;
        
        Debug.Log($"{targetSlot.playerName} cycled selection profile card index to: {targetSlot.selectedCatIndex}");
        UpdateLobbyDisplayUI();
    }

    // --- 🔄 DYNAMIC UI RE-RENDERING ENGINE ---

    private void UpdateLobbyDisplayUI()
    {
        if (playerListContainer == null || playerSlotPrefabTemplate == null) return;

        // Zero-Allocation Purging of old structural container display clones
        foreach (Transform child in playerListContainer) 
        {
            Destroy(child.gameObject);
        }

        // Instantiates and maps visual display row cards for each active slot
        foreach (var playerSlot in connectedPlayers)
        {
            GameObject instantiatedRow = Instantiate(playerSlotPrefabTemplate, playerListContainer);
            
            // Map name texts
            Text nameLabel = instantiatedRow.transform.Find("PlayerNameText")?.GetComponent<Text>();
            if (nameLabel != null) nameLabel.text = playerSlot.playerName;

            // Map current breed choice name strings dynamically out of ScriptableObject profiles
            Text breedLabel = instantiatedRow.transform.Find("SelectedBreedText")?.GetComponent<Text>();
            if (breedLabel != null && availableCatBreedsPool.Count > 0)
            {
                CatProfileData selectedData = availableCatBreedsPool[playerSlot.selectedCatIndex];
                if (selectedData != null)
                {
                    // Fallback validation to cleanly avoid null tracking crashes on fresh boots
                    bool isUnlocked = true;
                    
                    if (SaveSystem.Instance != null && SaveSystem.Instance.currentProgress != null)
                    {
                        var prog = SaveSystem.Instance.currentProgress;
                        isUnlocked = prog.unlockedCatIDs.Contains(selectedData.catBreedID) || prog.isPremiumLazyPassPurchased;
                    }

                    breedLabel.text = isUnlocked ? selectedData.displayBreedName : "🔒 LOCKED CAT BREED";
                }
            }
        }
    }

    // --- 🎬 LAUNCH ENGINE HANDOFF ---

    public void StartGameMatchSession()
    {
        if (PlayerSpawner.Instance == null)
        {
            Debug.LogError("Cannot launch match frame: PlayerSpawner instance is completely missing from current context managers.");
            return;
        }

        List<CatProfileData> launchProfilesList = new List<CatProfileData>();

        foreach (var slot in connectedPlayers)
        {
            if (slot.selectedCatIndex < availableCatBreedsPool.Count)
            {
                launchProfilesList.Add(availableCatBreedsPool[slot.selectedCatIndex]);
            }
        }

        Debug.Log($"Handing execution commands over to PlayerSpawner. Loading gameplay level scene zones now...");
        
        PlayerSpawner.Instance.SpawnAllPlayers(launchProfilesList);
        gameObject.SetActive(false);
    }
}