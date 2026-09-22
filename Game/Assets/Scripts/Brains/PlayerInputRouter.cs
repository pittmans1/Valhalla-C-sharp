using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CatBrainController))]
public class PlayerInputRouter : MonoBehaviour
{
    private CatBrainController brain;

    private void Awake()
    {
        brain = GetComponent<CatBrainController>();
    }

    // Called automatically by the PlayerInput component messaging system
    public void OnMove(InputValue value)
    {
        Vector2 movementVector = value.Get<Vector2>();
        brain.ReceiveMoveInput(movementVector);
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed) brain.ReceiveJumpInput();
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed) brain.ReceiveAttackInput();
    }

    public void OnGrab(InputValue value)
    {
        if (value.isPressed) brain.ReceiveGrabInput();
    }
}
