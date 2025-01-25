using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerFirstPersonMovement : MonoBehaviour
{
    public Transform target;                // The target transform to move (e.g., Player GameObject)
    public float moveSpeed = 5f;            // Movement speed
    public float sprintSpeed = 8f;          // Sprint speed
    public float gravity = -9.81f;          // Gravity strength

    private CharacterController characterController;
    private Vector3 velocity;
    private bool isGrounded;
    private bool freezeMovement = false;    // Controls whether player can move

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("Target is not assigned in PlayerFirstPersonMovement script.");
            return;
        }

        characterController = target.GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("Target does not have a CharacterController component.");
        }
    }

    private void Update()
    {
        if (characterController == null || freezeMovement) return; // Skip movement if frozen

        isGrounded = characterController.isGrounded;

        MovePlayer();
    }

    private void MovePlayer()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = target.right * horizontal + target.forward * vertical;

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
        velocity = moveDirection * currentSpeed;

        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime; // Apply gravity when not grounded
        }

        characterController.Move(velocity * Time.deltaTime);
    }

    // Freeze the player's movement
    public void FreezeMovement()
    {
        freezeMovement = true; // Disable movement
    }

    // Unfreeze the player's movement
    public void UnfreezeMovement()
    {
        freezeMovement = false; // Enable movement
    }
}
