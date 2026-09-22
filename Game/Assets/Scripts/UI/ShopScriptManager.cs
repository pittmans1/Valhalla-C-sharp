using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ExpansionPack
{
    public string expansionID;
    public string packTitle;
    public List<string> includedCatIDs;
    public List<string> includedMapPrefabs;
    public bool isUnlockedByDefault = false;
}

public class ExpansionShopController : MonoBehaviour
{
    [Header("Available DLC content listing")]
    [SerializeField] private List<ExpansionPack> expansionCatalog;

    [Header("UI Reference Prefabs")]
    [SerializeField] private Transform shopListContainer;
    [SerializeField] private GameObject shopItemButtonPrefab; // Simple UI button template

    private void Start()
    {
        PopulateExpansionStoreUI();
    }

    private void PopulateExpansionStoreUI()
    {
        // Clear old shop entries if reloading screen
        foreach (Transform child in shopListContainer) Destroy(child.gameObject);

        foreach (var pack in expansionCatalog)
        {
            GameObject buttonObj = Instantiate(shopItemButtonPrefab, shopListContainer);
            
            // Set text displaying the Expansion Title
            Text itemText = buttonObj.GetComponentInChildren<Text>();
            if (itemText != null) itemText.text = pack.packTitle;

            // Check if player already owns it
            Button btn = buttonObj.GetComponent<Button>();
            bool ownsPack = SaveSystem.Instance.currentProgress.purchasedExpansionIDs.Contains(pack.expansionID) || pack.isUnlockedByDefault;

            if (ownsPack)
            {
                if (itemText != null) itemText.text += " (Owned)";
                btn.interactable = false;
            }
            else
            {
                // Hook up purchase button callback dynamically
                btn.onClick.AddListener(() => PurchaseExpansionPack(pack));
            }
        }
    }

    /// <summary>
    /// Simulates currency transactions, overrides level/cat matrices, and saves progress to disk.
    /// </summary>
    public void PurchaseExpansionPack(ExpansionPack targetPack)
    {
        Debug.Log($"Processing real-money validation request for Expansion Pack: {targetPack.packTitle}...");

        // --- Payment Middleware Simulation Link ---
        // (This is exactly where you link Steam DLC APIs or In-App Purchase plugins later)
        bool paymentSuccessful = true; 
        // ------------------------------------------

        if (paymentSuccessful)
        {
            // 1. Log pack identifier to progress file
            SaveSystem.Instance.currentProgress.purchasedExpansionIDs.Add(targetPack.expansionID);

            // 2. Inject all expansion cats into user profile directly, bypassing story progress
            foreach (string catID in targetPack.includedCatIDs)
            {
                SaveSystem.Instance.UnlockCatBreed(catID);
            }

            // 3. Push map variants directly to your infinite generation system pools
            if (InfiniteLevelGenerator.Instance != null)
            {
                InfiniteLevelGenerator.Instance.InjectExpansionMaps(targetPack.includedMapPrefabs);
            }

            // 4. Force serialization to file
            SaveSystem.Instance.SaveGameData();
            
            // Refresh shop buttons instantly
            PopulateExpansionStoreUI();
            Debug.Log($"Successfully unlocked Expansion Pack content: {targetPack.packTitle}!");
        }
    }
}
