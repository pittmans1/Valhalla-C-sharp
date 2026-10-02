using UnityEngine;

public class WorldDestructionManager : MonoBehaviour
{
    public static WorldDestructionManager Instance { get; private set; }

    private DestructionEventBus destructionEventBus;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        destructionEventBus = FindAnyObjectByType<DestructionEventBus>();
        if (destructionEventBus == null)
        {
            GameObject busObject = new GameObject("DestructionEventBus");
            destructionEventBus = busObject.AddComponent<DestructionEventBus>();
        }

        destructionEventBus.PropDestroyed += OnPropDestroyed;
    }

    private void OnDestroy()
    {
        if (destructionEventBus != null)
        {
            destructionEventBus.PropDestroyed -= OnPropDestroyed;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnPropDestroyed(DestructionEvent destructionEvent)
    {
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.OnItemDestroyed(destructionEvent.ItemTypeTag, destructionEvent.PointValue);
        }

        UpdateSaveProgress(destructionEvent);
    }

    private void UpdateSaveProgress(DestructionEvent destructionEvent)
    {
        if (SaveSystem.Instance == null || SaveSystem.Instance.GameData == null)
        {
            return;
        }

        if (SaveSystem.Instance.GameData.playerStats != null)
        {
            SaveSystem.Instance.GameData.playerStats.totalPropsSmashed++;
        }

        if (SaveSystem.Instance.GameData.activeMissions == null)
        {
            return;
        }

        foreach (var mission in SaveSystem.Instance.GameData.activeMissions)
        {
            if (mission == null || mission.completeType != "smashables" || mission.isCompleted)
            {
                continue;
            }

            if (mission.completeAmount <= 0)
            {
                Debug.LogError($"[DATA ERROR] Mission ID {mission.missionID} has an invalid completion target.");
                continue;
            }

            mission.currentProgress++;
            if (mission.currentProgress >= mission.completeAmount)
            {
                mission.isCompleted = true;
                Debug.Log($"[MISSION COMPLETED] Mission ID {mission.missionID} has been successfully cleared!");
            }
        }
    }
}
