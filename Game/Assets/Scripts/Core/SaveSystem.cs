using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using System;

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
    public PlayerSaveProfile GameData {get; private set;} = new PlayerSaveProfile();
    
    private string saveFilePath;
    private readonly string cryptoSecretKey = "C474sTr0Ph3Sp177Y";
    [System.NonSerialized] 
    private System.Collections.Generic.Dictionary<int, CatBreedData> masterCatDatabase;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            saveFilePath = Path.Combine(Application.persistentDataPath, "CATastrophe_save.json");
            LoadGameData();
        }
        else Destroy(gameObject);
    }
    private void Start()
    {
        // Make sure this matches our registry boot call
        masterCatDatabase = CatBreedRegistry.GetMasterDatabase();
        Debug.Log($"[DATABASE] Master Cat Registry online. Loaded {masterCatDatabase.Count} playable breed variants.");
    }

    public void SaveGameData()
    {
        // EnsureProgressCollections();
        // string jsonOutput = JsonUtility.ToJson(currentProgress, true);
        // string temporaryPath = saveFilePath + ".tmp";

        try
        {
            string cleanJsonText = JsonUtility.ToJson(GameData, true);
            byte[] encryptedBytes = EncryptStringToBytes(cleanJsonText, cryptoSecretKey);
            File.WriteAllBytes(saveFilePath, encryptedBytes);
            // File.WriteAllText(temporaryPath, jsonOutput);
            // File.Copy(temporaryPath, saveFilePath, true);
            // File.Delete(temporaryPath);
        }
        catch (IOException exception)
        {
            Debug.LogError($"Could not save progression: {exception.Message}");
        }
    }

    public void LoadGameData()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                byte[] encryptedData = File.ReadAllBytes(saveFilePath);
                string decryptedJsonText = DecryptStringFromBytes(encryptedData, cryptoSecretKey);

                GameData = JsonUtility.FromJson<PlayerSaveProfile>(decryptedJsonText);
                // string jsonText = File.ReadAllText(saveFilePath);
                // currentProgress = JsonUtility.FromJson<GameProgressData>(jsonText) ?? new GameProgressData();
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"Could not load progression. Resetting to defaults: {exception.Message}");
                createDefaultProfile();
            }
        }
        else createDefaultProfile();

        // EnsureProgressCollections();
    }

    private void createDefaultProfile()
    {
        GameData = new PlayerSaveProfile();
        GameData.unlockedMapIDs.AddRange(new int[] {1,2,3,4});
        GameData.unlockedClothingIDs.AddRange(new int[]{1,7,9,14,25});
        SaveGameData();
    }

    // private void EnsureProgressCollections()
    // {
    //     currentProgress ??= new GameProgressData();
    //     currentProgress.unlockedCatIDs ??= new List<string>();
    //     currentProgress.unlockedClothesIDs ??= new List<string>();
    //     currentProgress.unlockedAchievements ??= new List<string>();
    //     currentProgress.purchasedExpansionIDs ??= new List<string>();
    // }

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
    // Ensure the casing matches EXACTLY what NetworkCatController is looking for!
    public CatBreedData GetCatBreedByID(int id)
    {
        if (masterCatDatabase != null && masterCatDatabase.TryGetValue(id, out CatBreedData data))
        {
            return data;
        }
        
        Debug.LogError($"[DATABASE ERROR] Attempted to query invalid Cat Breed ID: {id}");
        return null;
    }

    #region Cryptography Engine Engines
    private byte[] EncryptStringToBytes(string plainText, string secretKey)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
        byte[] ivBytes = new byte[16];
        Array.Copy(keyBytes, ivBytes, 16); // Mirror initial vectors out for simple parsing loops

        using (Aes aesEngine = Aes.Create())
        {
            aesEngine.Key = keyBytes;
            aesEngine.IV = ivBytes;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aesEngine.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
                    cryptoStream.Write(inputBytes, 0, inputBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    return memoryStream.ToArray();
                }
            }
        }
    }
    private string DecryptStringFromBytes(byte[] cipherData, string secretKey)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
        byte[] ivBytes = new byte[16];
        Array.Copy(keyBytes, ivBytes, 16);

        using (Aes aesEngine = Aes.Create())
        {
            aesEngine.Key = keyBytes;
            aesEngine.IV = ivBytes;
            using (MemoryStream memoryStream = new MemoryStream(cipherData))
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aesEngine.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (StreamReader streamReader = new StreamReader(cryptoStream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
        }
    }
    #endregion
}