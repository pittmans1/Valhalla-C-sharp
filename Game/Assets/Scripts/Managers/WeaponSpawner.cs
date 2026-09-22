using System.Collections.Generic;
using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private List<GameObject> weaponPrefabs;
    [SerializeField] private float minSpawnDelay = 5f;
    [SerializeField] private float maxSpawnDelay = 15f;
    [SerializeField] private float spawnForwardOffset = 15f;

    [Header("References")]
    [SerializeField] private Transform playerTrackingTarget;

    private void Start()
    {
        // Only spin up the spawning loop if we are playing Chaos PvP mode
        if (GameModeManager.Instance != null && GameModeManager.Instance.activeMode == GameModeType.ChaosPvP)
        {
            StartCoroutine(SpawnWeaponLoop());
        }
    }

    private System.Collections.IEnumerator SpawnWeaponLoop()
    {
        while (true)
        {
            float randomWait = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(randomWait);

            if (playerTrackingTarget != null && weaponPrefabs.Count > 0)
            {
                SpawnRandomWeapon();
            }
        }
    }

    private void SpawnRandomWeapon()
    {
        int randomIndex = Random.Range(0, weaponPrefabs.Count);
        
        // Project the weapon spawn ahead of the traveling player tracking vector point
        Vector3 spawnPos = playerTrackingTarget.position + (playerTrackingTarget.forward * spawnForwardOffset);
        spawnPos.y = 5f; // Drop it clean from the ceiling layout

        Instantiate(weaponPrefabs[randomIndex], spawnPos, Quaternion.identity);
        Debug.Log($"Chaos Weapon spawned into arena lane at coordinate: {spawnPos}");
    }
}
