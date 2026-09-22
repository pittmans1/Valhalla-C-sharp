using UnityEngine;

public class CatGrabber : MonoBehaviour
{
    private Rigidbody grabbedItem;
    private Vector3 lastPosition;
    private Vector3 calculatedVelocity;
    private bool isGrabbing = false;

    [Header("Grab Physics Layout")]
    [SerializeField] private Transform holdAnchor; 
    [SerializeField] private float grabRadius = 1.5f;
    [SerializeField] private LayerMask grabbableLayer;
    
    [Header("Momentum Throw Settings")]
    [SerializeField] private float baseThrowForce = 3.3f;
    [SerializeField] private int velocityFrameBuffer = 4;

    public bool IsHoldingWeapon => grabbedItem != null; // Placeholder hook for custom weapons

    public void ToggleGrabState()
    {
        if (!isGrabbing) TryGrabObject();
        else ReleaseObject(1.0f);
    }

    private void TryGrabObject()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, grabRadius, grabbableLayer);
        if (targets.Length > 0 && targets[0].TryGetComponent<Rigidbody>(out Rigidbody itemRb))
        {
            grabbedItem = itemRb;
            isGrabbing = true;
            
            grabbedItem.useGravity = false;
            grabbedItem.linearVelocity = Vector3.zero;
            grabbedItem.angularVelocity = Vector3.zero;
            
            lastPosition = holdAnchor.position;
        }
    }

    public void ReleaseObject(float forceMultiplier)
    {
        if (!isGrabbing || grabbedItem == null) return;

        grabbedItem.useGravity = true;
        // Combines character look forward projection + physical frame momentum buffer tracking
        grabbedItem.linearVelocity = (transform.forward * baseThrowForce * forceMultiplier) + calculatedVelocity;

        grabbedItem = null;
        isGrabbing = false;
    }

    public void UseHeldWeapon()
    {
        Debug.Log("Triggered customized weapon function on grabbed prop!");
    }

    private void FixedUpdate()
    {
        if (!isGrabbing || grabbedItem == null) return;

        grabbedItem.MovePosition(holdAnchor.position);
        grabbedItem.MoveRotation(holdAnchor.rotation);

        // Frame calculations tracking momentum accurately
        Vector3 instantVelocity = (holdAnchor.position - lastPosition) / Time.fixedDeltaTime;
        calculatedVelocity = Vector3.Lerp(calculatedVelocity, instantVelocity, 1f / velocityFrameBuffer);
        
        lastPosition = holdAnchor.position;
    }
}
