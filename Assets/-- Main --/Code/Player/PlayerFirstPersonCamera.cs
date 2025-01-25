using UnityEngine;

public class PlayerFirstPersonCamera : MonoBehaviour
{
    [Header("Targets")]
    public Transform cameraTransform;       // The transform of the camera (for vertical rotation)
    public Transform playerBody;           // The transform of the player body (for horizontal rotation)

    [Header("Sensitivity")]
    public float lookSpeedX = 2f;           // Mouse X sensitivity
    public float lookSpeedY = 2f;           // Mouse Y sensitivity

    [Header("Limits")]
    public float upperLookLimit = -60f;     // Limit for upward looking
    public float lowerLookLimit = 60f;      // Limit for downward looking

    private float rotationX = 0f;           // Current X rotation of the camera
    private bool canLook = true;            // Track whether the player can look around

    private void Start()
    {
        // Check if the targets are assigned
        if (cameraTransform == null || playerBody == null)
        {
            Debug.LogError("Please assign both 'cameraTransform' and 'playerBody' in the Inspector.");
        }
    }

    private void Update()
    {
        if (cameraTransform == null || playerBody == null) return;

        // Only allow rotation if the player can look
        if (canLook)
        {
            RotateCamera();
        }
    }

    private void RotateCamera()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * lookSpeedX;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeedY;

        // Apply vertical rotation (camera pitch)
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, upperLookLimit, lowerLookLimit);
        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

        // Apply horizontal rotation (player body yaw)
        playerBody.Rotate(Vector3.up * mouseX);
    }

    // Freeze the player's camera look (disable looking around)
    public void FreezeLook()
    {
        canLook = false;  // Disable looking around
    }

    // Unfreeze the player's camera look (enable looking around)
    public void UnfreezeLook()
    {
        canLook = true;  // Enable looking around
    }
}
