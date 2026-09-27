using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class CatVariantProfile
{
    public string catName;
    public GameObject modelObject3D; // Active mesh target inside your 3D viewport stand
}


public class CatFashionHub : MonoBehaviour
{
    [Header("Top Carousel Setup")]
    [SerializeField] private TextMeshProUGUI catNameText;
    [SerializeField] private Button leftArrowButton;
    [SerializeField] private Button rightArrowButton;
    [SerializeField] private List<CatVariantProfile> catVariants = new List<CatVariantProfile>();
    private int currentCatIndex = 0;

    [Header("Dynamic Wardrobe Grid Data")]
    [SerializeField] private Transform gridContentParent; // Assign the Scroll View 'Content' GameObject here
    [SerializeField] private GameObject itemSlotPrefab;     // Drop your ItemSlot UI template prefab here
    [SerializeField] private List<WardrobeItem> totalWardrobeDatabase = new List<WardrobeItem>();
    
    private ItemCategory currentActiveCategory = ItemCategory.Hats;

    private void Start()
    {
        // Wire up Carousel Arrow button actions
        if (leftArrowButton != null) leftArrowButton.onClick.AddListener(CycleLeft);
        if (rightArrowButton != null) rightArrowButton.onClick.AddListener(CycleRight);

        UpdateCarouselDisplay();
        GenerateWardrobeGrid();
    }

    // --- Carousel Engine ---
    public void CycleLeft()
    {
        if (catVariants.Count == 0) return;
        currentCatIndex--;
        if (currentCatIndex < 0) currentCatIndex = catVariants.Count - 1;
        UpdateCarouselDisplay();
    }

    public void CycleRight()
    {
        if (catVariants.Count == 0) return;
        currentCatIndex++;
        if (currentCatIndex >= catVariants.Count) currentCatIndex = 0;
        UpdateCarouselDisplay();
    }

    private void UpdateCarouselDisplay()
    {
        if (catVariants.Count == 0 || catNameText == null) return;

        // Set top label string text asset
        catNameText.text = catVariants[currentCatIndex].catName;

        // Toggle active status across your 3D models setup inside the scene viewport
        for (int i = 0; i < catVariants.Count; i++)
        {
            if (catVariants[i].modelObject3D != null)
            {
                catVariants[i].modelObject3D.SetActive(i == currentCatIndex);
            }
        }
    }

    // --- Tab Selector Switcher ---
    public void ChangeCategoryTab(int categoryIndex)
    {
        currentActiveCategory = (ItemCategory)categoryIndex;
        GenerateWardrobeGrid(); // Re-render target cells instantly
    }

    // --- Dynamic Generation System Loop ---
    public void GenerateWardrobeGrid()
    {
        if (gridContentParent == null || itemSlotPrefab == null) return;

        // Wipe existing temporary visual garbage inside content cell listings first
        foreach (Transform child in gridContentParent)
        {
            Destroy(child.gameObject);
        }

        // Run the Foreach Generator matching active category filters
        foreach (WardrobeItem item in totalWardrobeDatabase)
        {
            if (item.category != currentActiveCategory) continue;

            // Spawn grid slot prefab
            GameObject newSlot = Instantiate(itemSlotPrefab, gridContentParent);
            
            // Fetch internal visual bindings
            Image slotImageBackplate = newSlot.GetComponent<Image>();
            Button slotButton = newSlot.GetComponent<Button>();
            TextMeshProUGUI labelText = newSlot.GetComponentInChildren<TextMeshProUGUI>();
            
            // Locate an image layer child inside template to host cosmetic sprite icons
            Image itemIconImage = null;
            Transform iconTransform = newSlot.transform.Find("Item_Icon");
            if (iconTransform != null) itemIconImage = iconTransform.GetComponent<Image>();

            // Apply locked text visual formatting rules
            if (labelText != null) labelText.text = item.itemTitle;
            if (itemIconImage != null && item.itemIcon != null) itemIconImage.sprite = item.itemIcon;

            // Mock check: Determine if item is unlocked (integrates with your save system layer later)
            bool isUnlocked = item.isUnlockedByDefault; 

            if (isUnlocked)
            {
                // State A: Unlocked Configuration Tints
                if (slotImageBackplate != null) slotImageBackplate.color = new Color32(0x0B, 0x0B, 0x0C, 0xFF); // Charcoal Black #0B0B0C
                if (itemIconImage != null) itemIconImage.color = Color.white; // Pure crisp visuals
                if (slotButton != null) slotButton.interactable = true;
            }
            else
            {
                // State B: Locked Shadow Realization
                if (slotImageBackplate != null) slotImageBackplate.color = new Color32(0x0B, 0x0B, 0x0C, 0xFF); 
                if (itemIconImage != null) itemIconImage.color = Color.black; // Solid Blackened Silhouette Filter Mask
                if (slotButton != null) slotButton.interactable = false; // Block pointer inputs entirely
            }
        }
    }
}
