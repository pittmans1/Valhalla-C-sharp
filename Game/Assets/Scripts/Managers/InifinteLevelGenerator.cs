using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelThemeZone
{
    public string themeName; // e.g., "MilitaryBase", "OfficeWorker", "SciFi"
    public List<GameObject> themeSpecificPrefabs;
}

public class InfiniteLevelGenerator : MonoBehaviour
{
    [Header("Categorized Theme Configuration")]
    [SerializeField] private List<LevelThemeZone> themeZones;
    [SerializeField] private string currentActiveTheme = "StandardHome";

    [Header("Generation Elements")]
    [SerializeField] private List<GameObject> roomPrefabs; // Fallback global list
    [SerializeField] private Transform playerTrackingTarget;
    
    [Header("Generation Settings")]
    [SerializeField] private int initialRoomsToSpawn = 5;
    [SerializeField] private float roomLength = 30.0f;
    [SerializeField] private float spawnThresholdDistance = 45.0f;

    private Queue<GameObject> activeRooms = new Queue<GameObject>();
    private float nextSpawnZPosition = 0.0f;
    public static InfiniteLevelGenerator Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Spawns the clean starting platform layout blocks sequentially on load
        for (int i = 0; i < initialRoomsToSpawn; i++)
        {
            SpawnNextRoomChunk();
        }
    }

    private void Update()
    {
        if (playerTrackingTarget == null) return;

        // Tracks player location to build ahead and recycle modules behind
        if (Vector3.Distance(playerTrackingTarget.position, new Vector3(0, 0, nextSpawnZPosition)) < spawnThresholdDistance)
        {
            SpawnNextRoomChunk();
            RemoveOldestRoomChunk();
        }
    }

    private void SpawnNextRoomChunk()
    {
        List<GameObject> poolToUse = GetActiveThemePool();
        if (poolToUse == null || poolToUse.Count == 0) return;

        int randomIndex = Random.Range(0, poolToUse.Count);
        Vector3 spawnPosition = new Vector3(0, 0, nextSpawnZPosition);
        
        GameObject newRoom = Instantiate(poolToUse[randomIndex], spawnPosition, Quaternion.identity);
        activeRooms.Enqueue(newRoom);
        
        nextSpawnZPosition += roomLength;
    }

    private void RemoveOldestRoomChunk()
    {
        if (activeRooms.Count > initialRoomsToSpawn)
        {
            GameObject oldRoom = activeRooms.Dequeue();
            Destroy(oldRoom);
        }
    }

    private List<GameObject> GetActiveThemePool()
    {
        // Search through grouped data zones first
        LevelThemeZone activeZone = themeZones.Find(x => x.themeName == currentActiveTheme);
        if (activeZone != null && activeZone.themeSpecificPrefabs.Count > 0)
        {
            return activeZone.themeSpecificPrefabs;
        }
        return roomPrefabs; // Fall back to base list if name isn't found
    }

    public void InjectExpansionMaps(List<string> mapPrefabResourcePaths)
    {
        foreach (string path in mapPrefabResourcePaths)
        {
            GameObject mapPrefab = Resources.Load<GameObject>(path);
            if (mapPrefab != null && !roomPrefabs.Contains(mapPrefab))
            {
                roomPrefabs.Add(mapPrefab);
                Debug.Log($"Injected dynamic room variant: '{path}' into generation matrices!");
            }
        }
    }

    public void SetCurrentTheme(string themeZoneName)
    {
        currentActiveTheme = themeZoneName;
        Debug.Log($"Active level theme pool shifted to: {themeZoneName}");
    }

    public void ForceSpawnRoomOverride(GameObject roomPrefab)
    {
        if (roomPrefab == null) return;
        
        Vector3 spawnPosition = new Vector3(0, 0, nextSpawnZPosition);
        GameObject newRoom = Instantiate(roomPrefab, spawnPosition, Quaternion.identity);
        activeRooms.Enqueue(newRoom);
        
        nextSpawnZPosition += roomLength;
        Debug.Log($"Special transition room gateway injected manually: {roomPrefab.name}");
    }

}
