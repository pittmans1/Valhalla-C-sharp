using UnityEngine;

public enum ItemCategory { Hats, Outfits, Collars }

[CreateAssetMenu(fileName = "NewWardrobeItem", menuName = "Wardrobe Item")]
public class WardrobeItem : ScriptableObject
{
    public string itemID;
    public string itemTitle;
    public Sprite itemIcon;
    public ItemCategory category;
    
    [Header("Unlocking Mechanics")]
    public bool isUnlockedByDefault = false;
}
