using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NetworkCatController : NetworkBehaviour
{
    [Header("Movement Tweaks")]
    public float movementForce = 20f;
    public float topSpeed = 10f;

    [Header("Synchronized Identity Profile")]
    // Synchronizes the selected breed index across the network pipeline to all clients instantly
    public NetworkVariable<int> activeCatBreedID = new NetworkVariable<int>(0);

    private Rigidbody catBody;
    private CatBreedData activeBreedProfile;

    private void Awake()
    {
        catBody = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            catBody.interpolation = RigidbodyInterpolation.Interpolate;
        }
        else
        {
            catBody.interpolation = RigidbodyInterpolation.None;
            catBody.isKinematic = !IsServer; 
        }

        // Initialize and lock down active visual profile configurations
        ApplyCatBreedProfileSettings();
    }

    private void ApplyCatBreedProfileSettings()
    {
        // Query our safe master database lookup engine to find this specific profile asset
        if (SaveSystem.Instance != null)
        {
            activeBreedProfile = SaveSystem.Instance.GetCatBreedByID(activeCatBreedID.Value);
            
            if (activeBreedProfile != null)
            {
                Debug.Log($"[IDENTITY] Network spawned cat successfully configured variant: {activeBreedProfile.catName}");
                
                // Programmatically multiply your engine vectors by the cat's unique attribute modifiers!
                movementForce *= activeBreedProfile.attributes.speedModifier;
                catBody.mass *= activeBreedProfile.attributes.weightModifier;
            }
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector3 rawInputDirection = new Vector3(moveX, 0f, moveZ).normalized;

        if (rawInputDirection.magnitude > 0.1f)
        {
            MoveCatServerRpc(rawInputDirection);
        }
    }

    [ServerRpc]
    private void MoveCatServerRpc(Vector3 direction)
    {
        if (catBody.linearVelocity.magnitude < topSpeed)
        {
            catBody.AddForce(direction * movementForce, ForceMode.Acceleration);
        }

        Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
        catBody.MoveRotation(Quaternion.Slerp(catBody.rotation, lookRotation, Time.deltaTime * 12f));
    }
}
