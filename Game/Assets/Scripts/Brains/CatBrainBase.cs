using UnityEngine;

// This replaces your unified script as the single point of entry for actions
public class CatBrainController : MonoBehaviour
{
    private CatMovement movementWorker;
    private CatGrabber grabberWorker;
    private CatPaws pawsWorker; // Presumed script tracking your original Swipe() layout

    [Header("Cat Identity Profile")]
    public string catVariantName = "Orange Chaos";
    public bool isSleeping = false;

    public int Health { get; private set; } = 100; // Default health points
    private void Awake()
    {
        movementWorker = GetComponent<CatMovement>();
        grabberWorker = GetComponent<CatGrabber>();
        pawsWorker = GetComponent<CatPaws>();
    }

    // Pure abstract API triggers isolated from raw device configurations
    public void ReceiveMoveInput(Vector2 inputs)
    {
        if (isSleeping) return;
        movementWorker.SetMoveDirection(new Vector3(inputs.x, 0, inputs.y));
    }

    public void ReceiveJumpInput()
    {
        if (!isSleeping) movementWorker.ExecuteJump();
    }

    public void TakeDamage(float damageAmount)
    {
        if (isSleeping) return;

        Health -= (int)damageAmount;
        if (Health <= 0)
        {
            isSleeping = true;
            // Trigger any death or sleep animations here
        }
    }

    public void ReceiveAttackInput()
    {
        if (isSleeping) return;

        if (grabberWorker.IsHoldingWeapon)
        {
            grabberWorker.UseHeldWeapon();
        }
        else
        {
            pawsWorker.Swipe();
        }
    }

    public void ReceiveGrabInput()
    {
        if (!isSleeping) grabberWorker.ToggleGrabState();
    }
}
