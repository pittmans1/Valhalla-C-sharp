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
        if (shopListContainer == null || shopItemButtonPrefab == null) return;

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
            bool ownsPack = pack.isUnlockedByDefault ||
                (SaveSystem.Instance != null && SaveSystem.Instance.currentProgress != null &&
                 SaveSystem.Instance.currentProgress.purchasedExpansionIDs.Contains(pack.expansionID));

            if (ownsPack)
            {
                if (itemText != null) itemText.text += " (Owned)";
                if (btn != null) btn.interactable = false;
            }
            else
            {
                if (itemText != null) itemText.text += " (Unavailable)";
                if (btn != null) btn.interactable = false;
            }
        }
    }

    /// <summary>
    /// Purchases remain disabled until a platform provider validates the transaction.
    /// </summary>
    public void PurchaseExpansionPack(ExpansionPack targetPack)
    {
        if (targetPack == null || SaveSystem.Instance == null || SaveSystem.Instance.currentProgress == null)
        {
            Debug.LogError("Cannot purchase an invalid expansion or before save data is ready.");
            return;
        }

        if (SaveSystem.Instance.currentProgress.purchasedExpansionIDs.Contains(targetPack.expansionID))
        {
            Debug.Log($"Expansion already owned: {targetPack.packTitle}");
            return;
        }

        Debug.LogWarning($"Purchase blocked for {targetPack.packTitle}: no store provider is configured. No unlock was recorded.");
    }
}
