using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCatProfile", menuName = "Cat Profile Data")]
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
[Serializable]
public class StatData
{
    public int totalGashaponSpins;
    public int totalPropsSmashed;
    public float highestVelocityImpact;
    public int catResurrections;
    
}
[Serializable]
public class MissionTracker
{
    public int missionID;
    public int currentProgress;
    public string completeType;
    public int? completeAmount;
    public bool isCompleted;
}

[Serializable]
public class PlayerSaveProfile
{
    public StatData playerStats = new StatData();
    public List<int> unlockedMapIDs = new List<int>();
    public List<int> unlockedClothingIDs = new List<int>();
    public List<MissionTracker> activeMissions = new List<MissionTracker>();
    public List<string> unlockedAchievements = new List<string>();
}
