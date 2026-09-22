using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ClothingItem
{
    public string clothingID;
    public GameObject clothingPrefab;
    public enum ClothingSlot { Head, Neck, Back }
    public ClothingSlot targetSlot;
}

public class CatClothingAttacher : MonoBehaviour
{
    [Header("Skeleton Attachment Anchors")]
    [SerializeField] private Transform headAnchor;
    [SerializeField] private Transform neckAnchor;
    [SerializeField] private Transform backAnchor;

    [Header("Available Cosmetics Database")]
    [SerializeField] private List<ClothingItem> cosmeticDatabase;

    // Track active equipped instances to clean them up when switching items
    private Dictionary<string, GameObject> equippedItems = new Dictionary<string, GameObject>();

    /// <summary>
    /// Spawns a clothing prefab and perfectly anchors/orients it to the cat's bone structure.
    /// </summary>
    public void EquipClothing(string itemID)
    {
        // 1. Locate item details in our local database
        ClothingItem item = cosmeticDatabase.Find(x => x.clothingID == itemID);
        if (item == null)
        {
            Debug.LogWarning($"Cosmetic ID '{itemID}' not found in database!");
            return;
        }

        // 2. Identify correct anchor position
        Transform targetAnchor = GetAnchorFromSlot(item.targetSlot);
        if (targetAnchor == null) return;

        // 3. Clear existing clothing in that slot so items don't stack awkwardly
        string slotKey = item.targetSlot.ToString();
        if (equippedItems.ContainsKey(slotKey))
        {
            Destroy(equippedItems[slotKey]);
            equippedItems.Remove(slotKey);
        }

        // 4. Instantiate, nest, and reset positions cleanly
        GameObject instantiatedCloth = Instantiate(item.clothingPrefab, targetAnchor);
        instantiatedCloth.transform.localPosition = Vector3.zero;
        instantiatedCloth.transform.localRotation = Quaternion.identity;
        instantiatedCloth.transform.localScale = Vector3.one;

        equippedItems.Add(slotKey, instantiatedCloth);
    }

    private Transform GetAnchorFromSlot(ClothingItem.ClothingSlot slot)
    {
        return slot switch
        {
            ClothingItem.ClothingSlot.Head => headAnchor,
            ClothingItem.ClothingSlot.Neck => neckAnchor,
            ClothingItem.ClothingSlot.Back => backAnchor,
            _ => null
        };
    }
}
