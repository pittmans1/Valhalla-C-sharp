using UnityEngine;

// In a full online system, this inherits from Unity's standard NetworkBehaviour asset
public class NetworkLobbyInitialization : MonoBehaviour
{
    public static NetworkLobbyInitialization Instance;

    [Header("Server Connection Status")]
    public bool isNetworkServerActive = false;
    public string activeNetworkRoomToken = "";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Spins up a local host server instance and flags the connection permissions.
    /// </summary>
    public void StartPrivateOnlineHost(string generatedRoomCode)
    {
        activeNetworkRoomToken = generatedRoomCode;
        isNetworkServerActive = true;
        
        Debug.Log($"Netcode Server initialized. Listening for remote connections on Room Token: {activeNetworkRoomToken}");
        
        // TODO: Insert absolute Netcode framework startup command here:
        // Unity.Netcode.NetworkManager.Singleton.StartHost();
    }

    /// <summary>
    /// Points a connecting remote client machine toward a friend's host server room token.
    /// </summary>
    public void JoinPrivateOnlineMatch(string incomingRoomCode)
    {
        activeNetworkRoomToken = incomingRoomCode;
        Debug.Log($"Attempting remote handshake linking to network address target code: {activeNetworkRoomToken}...");
        
        // TODO: Insert absolute Netcode framework remote client joining command here:
        // Unity.Netcode.NetworkManager.Singleton.StartClient();
    }
}
