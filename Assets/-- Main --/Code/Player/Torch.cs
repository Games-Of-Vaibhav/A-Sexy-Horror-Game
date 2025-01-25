using UnityEngine;

public class Torch : MonoBehaviour
{
    [Header("Targets")]
    public Transform torchTransform;       // The Transform of the torch (e.g., Spotlight)
    public Transform followTarget;        // The Transform the torch should follow (position and rotation)

    [Header("Settings")]
    public float positionFollowSpeed = 5f; // Speed for position interpolation
    public float rotationFollowSpeed = 5f; // Speed for rotation interpolation

    private void Start()
    {
        // Ensure the torchTransform is detached from any parent
        if (torchTransform != null)
        {
            torchTransform.parent = null;
        }
        else
        {
            Debug.LogError("Torch Transform is not assigned in the Torch script.");
        }
    }

    private void LateUpdate()
    {
        // Ensure followTarget and torchTransform are assigned
        if (torchTransform == null || followTarget == null)
        {
            Debug.LogError("Torch or FollowTarget is not assigned in the Torch script.");
            return;
        }

        // Smoothly interpolate the torch's position to the followTarget's position
        torchTransform.position = Vector3.Lerp(torchTransform.position, followTarget.position, positionFollowSpeed * Time.deltaTime);

        // Smoothly interpolate the torch's rotation to the followTarget's rotation
        torchTransform.rotation = Quaternion.Lerp(torchTransform.rotation, followTarget.rotation, rotationFollowSpeed * Time.deltaTime);
    }
}
