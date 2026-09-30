// using Unity.Netcode;
// using UnityEngine;

// public class SandboxStressTester : MonoBehaviour
// {
//     [Header("Stress Targets")]
//     public GameObject catPrefab;
//     public GameObject breakablePropPrefab;
    
//     [Header("Testing Volumes")]
//     public int totalCatsToSpawn = 20;
//     public int totalPropsToSpawn = 2000; // Push your hardware limits here!

//     private void OnGUI()
//     {
//         // Simple UI controls drawn directly onto your screen corner
//         GUILayout.BeginArea(new Rect(20, 20, 250, 200));

//         if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
//         {
//             if (GUILayout.Button("Launch Game as HOST (Player + Server)", GUILayout.Height(40)))
//             {
//                 NetworkManager.Singleton.StartHost();
//                 SpawnSandboxLoad();
//             }
//         }
//         else
//         {
//             GUILayout.Label($"Performance Status: Active Network Connection");
//             GUILayout.Label($"Running Physics Entities: {totalCatsToSpawn + totalPropsToSpawn}");
//         }

//         GUILayout.EndArea();
//     }

//     private void SpawnSandboxLoad()
//     {
//         // Only the Host server is legally allowed to spawn network objects
//         if (!NetworkManager.Singleton.IsServer) return;

//         // 1. Spawning the Cats
//         for( int i = 0; i < 20; i++) {}

//         // 2. Spawning Thousands of Destroyables
//         for (int i = 0; i < totalPropsToSpawn; i++)
//         {
//             Vector3 randomPropPos = new Vector3(
//                 Random.Range(-5.5f, 5.5f), 
//                 Random.Range(0.5f, 5.0f), // Stack them high so they crash down naturally
//                 Random.Range(-5.5f, 5.5f)
//             );

//             GameObject prop = Instantiate(breakablePropPrefab, randomPropPos, Random.rotation);
            
//             // Critical Unity 6 Optimization: Disable individual network tracking for debris!
//             // Let the local engine simulation drop frames instead of flooding your internet data pipeline.
//         }
//     }
// }
