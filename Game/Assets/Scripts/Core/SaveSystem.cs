using System.IO;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameProgressData
{
    // Unlock matrices
    public List<string> unlockedCatIDs = new List<string>() { "Tuxedo", "Orange" };
    public List<string> unlockedClothesIDs = new List<string>() { "DefaultHat" };
    public List<string> unlockedAchievements = new List<string>();
    public List<string> purchasedExpansionIDs = new List<string>();
    // Stats
    public int totalVasesSmashed = 0;
    public bool isPremiumLazyPassPurchased = false; // Microtransaction override flag
}

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;
    public GameProgressData currentProgress = new GameProgressData();
    
    private string saveFilePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            saveFilePath = Path.Combine(Application.persistentDataPath, "chaos_save.json");
            LoadGameData();
        }
        else Destroy(gameObject);
    }

    public void SaveGameData()
    {
        string jsonOutput = JsonUtility.ToJson(currentProgress, true);
        File.WriteAllText(saveFilePath, jsonOutput);
        Debug.Log("Game progression synced to disk securely.");
    }

    public void LoadGameData()
    {
        if (File.Exists(saveFilePath))
        {
            string jsonText = File.ReadAllText(saveFilePath);
            currentProgress = JsonUtility.FromJson<GameProgressData>(jsonText);
        }
    }

    // --- Progression, Lobbies, and Achievement Logic ---
    public void AwardProgressAchievement(string achievementID, bool isPrivateLobby)
    {
        // Private lobbies and public matches both track progression safely
        if (currentProgress.unlockedAchievements.Contains(achievementID)) return;

        currentProgress.unlockedAchievements.Add(achievementID);
        SaveGameData();
        Debug.Log($"Achievement Unlocked: {achievementID}!");
    }

    public void UnlockCatBreed(string catID)
    {
        if (!currentProgress.unlockedCatIDs.Contains(catID))
        {
            currentProgress.unlockedCatIDs.Add(catID);
            SaveGameData();
        }
    }

    // --- Microtransaction "Lazy Bypass" Action ---
    public void BuyAllUnlocksViaStore()
    {
        currentProgress.isPremiumLazyPassPurchased = true;
        
        // Force inject every item profile into the list instantly
        string[] allCats = { "FatOrange", "FatTuxedo", "Leopard", "Siamese", "BlackCat" };
        string[] allClothes = { "Crown", "GoldCollar", "WizardHat", "Sunglasses" };

        foreach (var cat in allCats) UnlockCatBreed(cat);
        foreach (var cloth in allClothes) if (!currentProgress.unlockedClothesIDs.Contains(cloth)) currentProgress.unlockedClothesIDs.Add(cloth);

        SaveGameData();
        Debug.Log("Premium Lazy Bypass Activated. All content unlocked via payment simulation!");
    }
}
