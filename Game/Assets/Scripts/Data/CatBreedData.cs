using UnityEngine;

[System.Serializable]
public struct CatAttributes
{
    public float weightModifier;      // e.g., 1.5f for a fat cat to make it heavy
    public float speedModifier;       // e.g., 0.8f for slower or faster movement
    public float destructionPower;    // Boosts smash force calculation vectors
    public float healthModifier;

}

[CreateAssetMenu(fileName = "NewCatBreed", menuName = "Cat Breed Data")]
public class CatBreedData : ScriptableObject
{
    [Header("Identity Mapping")]
    public int catID;                // Must match the integers in your unlock list!
    public string catName;           // e.g., "Fat Tuxedo", "Orange Chaos"
    
    [Header("Visual Asset Reference")]
    public GameObject catMeshPrefab; // The specific 3D model for this breed

    [Header("Gameplay Tuning Profiles")]
    public CatAttributes attributes;
}