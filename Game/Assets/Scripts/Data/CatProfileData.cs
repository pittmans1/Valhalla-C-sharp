using UnityEngine;

[CreateAssetMenu(fileName = "NewCatProfile", menuName = "ChaosCat/Cat Profile Data")]
public class CatProfileData : ScriptableObject
{
    [Header("Identity Customization")]
    public string catBreedID = "Orange";
    public string displayBreedName = "Orange Chaos Menace";
    public Sprite characterSelectionIcon;
    public GameObject uniqueBasePrefabModel;
    

    [Header("Locomotion Tuning Balancing")]
    public float movementVelocitySpeed = 9.5f;
    public float jumpImpulseForce = 10.0f;
    public bool allowsDoubleJumping = true;

    [Header("Chaos Attribute Shifting")]
    public float clawSwipeForceModifier = 1.2f;
    public float projectileLaunchVelocityScale = 1.4f;
    public float maximumHealthPoints = 120.0f;
}
