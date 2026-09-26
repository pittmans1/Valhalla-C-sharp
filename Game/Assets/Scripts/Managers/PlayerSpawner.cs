using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviour 
{
    public static PlayerSpawner Instance { get; private set; }
    
    [Header("Spawn Layout Configurations")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    [SerializeField] private GameObject fallbackCatPrefab;
    private static List<CatProfileData> lastSelectedPlayers;
    private static bool restartPending;
    private static GameModeType modeToRestore;

    private void Awake() 
    { 
        if (Instance == null) 
        {
            Instance = this;
        } 
        else 
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (!restartPending) return;

        restartPending = false;
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.activeMode = modeToRestore;
            GameModeManager.Instance.ResetMatch();
        }

        SpawnAllPlayers(lastSelectedPlayers);
    }

    public void RestartCurrentMatch()
    {
        var currentScene = SceneManager.GetActiveScene();
        if (currentScene.buildIndex < 0)
        {
            Debug.LogError("Cannot restart the match: the active scene is not in Build Settings.");
            return;
        }

        if (GameModeManager.Instance != null)
        {
            modeToRestore = GameModeManager.Instance.activeMode;
        }

        restartPending = true;
        SceneManager.LoadScene(currentScene.buildIndex);
    }
    
    public void SpawnAllPlayers(List<CatProfileData> selectedPlayersData) 
    {
        lastSelectedPlayers = selectedPlayersData == null
            ? new List<CatProfileData>()
            : new List<CatProfileData>(selectedPlayersData);

        if (selectedPlayersData == null || selectedPlayersData.Count == 0) 
        { 
            SpawnSinglePlayerFallback(); 
            return; 
        }

        int spawnCount = Mathf.Min(selectedPlayersData.Count, spawnPoints.Count);
        
        for (int i = 0; i < spawnCount; i++) 
        {
            CatProfileData profile = selectedPlayersData[i];
            
            // FIX: Changed from profile.catPrefabRef to your actual variable: uniqueBasePrefabModel
            if (profile == null || profile.uniqueBasePrefabModel == null) continue;

            Transform spawnPoint = spawnPoints[i];
            
            // FIX: Instantiating your actual variable: uniqueBasePrefabModel
            GameObject spawnedCat = Instantiate(profile.uniqueBasePrefabModel, spawnPoint.position, spawnPoint.rotation);
            
            // Accessing components globally with zero namespace issues
            if (spawnedCat.TryGetComponent(out CatMovement movementWorker)) 
            {
                movementWorker.BoostSpeed(profile.movementVelocitySpeed / 8.0f);
            }
            
            if (GameModeManager.Instance != null && spawnedCat.TryGetComponent(out CatBrainController brain)) 
            {
                GameModeManager.Instance.RegisterCat(brain);
            }
        }
    }

    private void SpawnSinglePlayerFallback() 
    { 
        if (fallbackCatPrefab != null) 
        {
            GameObject fallbackCat = Instantiate(fallbackCatPrefab, Vector3.up, Quaternion.identity);
            if (GameModeManager.Instance != null && fallbackCat.TryGetComponent(out CatBrainController brain))
            {
                GameModeManager.Instance.RegisterCat(brain);
            }
        }
    }
}
