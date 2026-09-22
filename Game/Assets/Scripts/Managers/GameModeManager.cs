using System.Collections.Generic;
using UnityEngine;

public enum GameModeType { Story, ChaosPvP, BustedMode, CoOpVsAI }

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;

    [Header("Game Mode Configuration")]
    public GameModeType activeMode;
    public float matchTimer = 180f; // 3 minutes default
    
    [Header("Busted Mode Settings")]
    public int totalCatDestructionPoints;
    
    [Header("Co-Op Target List")]
    public List<string> destructionTargetList = new List<string>() { "Vase", "TV", "Chandelier", "Couch" };
    private List<string> destroyedItems = new List<string>();

    [Header("References")]
    [SerializeField] private GameObject yarnBallGodPrefab;
    [SerializeField] private Transform godSpawnPoint;

    private List<CatBrainController> activeCats = new List<CatBrainController>();
    private bool isMatchActive = true;

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

    private void Update()
    {
        if (!isMatchActive) return;

        matchTimer -= Time.deltaTime;
        if (matchTimer <= 0)
        {
            EndMatch();
        }
    }

    public void RegisterCat(CatBrainController cat)
    {
        if (!activeCats.Contains(cat))
        {
            activeCats.Add(cat);
        }
    }

    public void OnItemDestroyed(string itemTag, int points)
    {
        totalCatDestructionPoints += points;
        Debug.Log($"Item Smashed! Tag: {itemTag} | Points Awarded: {points} | Total Score: {totalCatDestructionPoints}");

        if (activeMode == GameModeType.CoOpVsAI && destructionTargetList.Contains(itemTag))
        {
            if (!destroyedItems.Contains(itemTag))
            {
                destroyedItems.Add(itemTag);
                Debug.Log($"Co-Op Objective Cleared: {itemTag} destroyed!");
            }

            if (destroyedItems.Count == destructionTargetList.Count)
            {
                CatsWin("All target items destroyed!");
            }
        }
    }

    public void CheckCatDownStates()
    {
        bool allCatsDown = true;
        foreach (var cat in activeCats)
        {
            if (cat != null && !cat.isSleeping)
            {
                allCatsDown = false;
            }
        }

        if (allCatsDown)
        {
            HumanWins("All cats have been knocked out cold!");
        }
    }

    private void EndMatch()
    {
        isMatchActive = false;
        
        if (activeMode == GameModeType.BustedMode)
        {
            // In Busted mode, if time runs out and cats are alive, cats win by points
            CatsWin($"Time ran out! Cats score: {totalCatDestructionPoints}");
        }
    }

    private void HumanWins(string reason)
    {
        Debug.Log($"HUMAN WINS: {reason}");
        isMatchActive = false;
        TriggerYarnBallGodPunishment();
    }

    private void CatsWin(string reason)
    {
        Debug.Log($"CATS WIN: {reason}");
        isMatchActive = false;
    }

    private void TriggerYarnBallGodPunishment()
    {
        Debug.Log("The Cats failed... Summoning the Yarn Ball God!");
        if (yarnBallGodPrefab != null && godSpawnPoint != null)
        {
            Instantiate(yarnBallGodPrefab, godSpawnPoint.position, Quaternion.identity);
        }
    }
}
