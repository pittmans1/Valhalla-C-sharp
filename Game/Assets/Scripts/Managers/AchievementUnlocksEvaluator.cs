using UnityEngine;

public class AchievementUnlocksEvaluator : MonoBehaviour
{
    public static AchievementUnlocksEvaluator Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Checks global scores and automatically unlocks milestone rewards in your JSON save file.
    /// </summary>
    public void EvaluateProgressionMilestones(int freshlyAddedPoints)
    {
        if (SaveSystem.Instance == null) return;

        // Update the accumulated data structure inside the running save profile tracker
        SaveSystem.Instance.currentProgress.totalVasesSmashed++;
        int activeSessionPoints = GameModeManager.Instance.totalCatDestructionPoints;

        // Milestone A: Score over 2,000 points in a single match run
        if (activeSessionPoints >= 2000)
        {
            UnlockRewardItem("WizardHat", "Achievement: Chaos Sorcerer! Wizard Hat unlocked!");
        }

        // Milestone B: Score over 10,000 points across the career save file progress
        if (SaveSystem.Instance.currentProgress.totalVasesSmashed >= 50)
        {
            UnlockRewardItem("GoldCollar", "Achievement: Demolition Royalty! Gold Collar item unlocked!");
            SaveSystem.Instance.UnlockCatBreed("Calico"); // Unlocks a completely selectable new character breed variant!
        }
    }

    private void UnlockRewardItem(string itemID, string debugLogText)
    {
        if (!SaveSystem.Instance.currentProgress.unlockedClothesIDs.Contains(itemID))
        {
            SaveSystem.Instance.currentProgress.unlockedClothesIDs.Add(itemID);
            SaveSystem.Instance.SaveGameData(); // Serialize data shifts immediately to disk safely
            Debug.Log(debugLogText);
        }
    }
}
