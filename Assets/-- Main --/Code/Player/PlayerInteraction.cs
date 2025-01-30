using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Key Settings")]
    public int totalKeysRequired = 3; // Total number of keys needed to escape
    private int keysCollected = 0; // Tracks collected keys
    public LayerMask keyLayer; // Layer to detect keys

    [Header("UI Settings")]
    public GameObject interactionUI; // UI to show when near a key or door
    public TMP_Text interactionText; // TextMeshPro text to describe the interaction
    public TMP_Text keysCollectedText; // TextMeshPro text to display collected keys
    public GameObject victoryPanel; // Victory panel to show on escape

    [Header("Escape Door Settings")]
    public Transform escapeDoor; // Reference to the escape door
    public float doorInteractionDistance = 2f; // Distance within which player can interact with the door

    [Header("Player Components")]
    public PlayerFirstPersonMovement playerMovement; // Reference to the movement script
    public PlayerFirstPersonCamera playerCamera; // Reference to the camera script
    private bool isInteracting = false; // Whether the player is currently interacting

    private GameManager gameManager; // Reference to the GameManager script
    private Camera playerCameraComponent; // Player's camera for raycasting
    private GameObject currentKey; // The key the player is currently looking at

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        playerCameraComponent = Camera.main;

        UpdateKeysCollectedUI();
        interactionUI.SetActive(false); // Hide interaction UI by default
        victoryPanel.SetActive(false);
    }

    private void Update()
    {
        if (!isInteracting)
        {
            bool keyDetected = DetectKey();
            bool doorDetected = false;

            // Only check door detection if no key is being interacted with
            if (!keyDetected)
            {
                doorDetected = DetectEscapeDoor();
            }

            // Hide UI if neither key nor door is detected
            if (!keyDetected && !doorDetected)
            {
                SetInteractionUI(false, string.Empty);
            }
        }
    }

    private bool DetectKey()
    {
        Ray ray = playerCameraComponent.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, 10f, keyLayer))
        {
            if (hit.collider.CompareTag("Key"))
            {
                currentKey = hit.collider.gameObject;
                SetInteractionUI(true, "Press [E] to collect the key");

                if (Input.GetKeyDown(KeyCode.E))
                {
                    CollectKey();
                }

                return true;
            }
        }

        return false;
    }

    private void CollectKey()
    {
        keysCollected++;
        UpdateKeysCollectedUI();
        Destroy(currentKey);
        currentKey = null;
    }

    private bool DetectEscapeDoor()
    {
        float distanceToDoor = Vector3.Distance(transform.position, escapeDoor.position);
        if (distanceToDoor <= doorInteractionDistance)
        {
            if (keysCollected >= totalKeysRequired)
            {
                SetInteractionUI(true, "Press [E] to escape");
                if (Input.GetKeyDown(KeyCode.E))
                {
                    Escape();
                }
            }
            else
            {
                SetInteractionUI(true, "You need more keys to escape!");
            }

            return true;
        }

        return false;
    }

    private void Escape()
    {
        isInteracting = true;
        SetInteractionUI(false, string.Empty);

        // Freeze player movement and camera
        playerMovement.FreezeMovement();
        playerCamera.FreezeLook();

        // Show victory panel
        victoryPanel.SetActive(true);
    }

    private void UpdateKeysCollectedUI()
    {
        keysCollectedText.text = $"Keys Collected: {keysCollected}/{totalKeysRequired}";
    }

    private void SetInteractionUI(bool isVisible, string message)
    {
        interactionUI.SetActive(isVisible);
        interactionText.text = message;
    }
}
