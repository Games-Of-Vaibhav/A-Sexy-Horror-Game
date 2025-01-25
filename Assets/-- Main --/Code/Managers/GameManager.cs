using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject deathPanel;  // Reference to the death panel UI
    public Button restartButton;   // Button to restart the level
    public Button mainMenuButton;  // Button to go to the main menu

    private SceneManager sceneManager; // Reference to the SceneManager script

    private void Start()
    {
        // Ensure the death panel is initially hidden
        deathPanel.SetActive(false);

        // Get the SceneManager reference
        sceneManager = FindObjectOfType<SceneManager>();

        if (sceneManager == null)
        {
            Debug.LogError("SceneManager script not found in the scene.");
        }

        // Assign button listeners for actions
        restartButton.onClick.AddListener(RestartLevel);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    private void OnEnable()
    {
        // Subscribe to the player's death event (ensure PlayerHealth script is sending it)
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.onDeath.AddListener(ShowDeathPanel); // Show death panel when player dies
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from the player's death event
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.onDeath.RemoveListener(ShowDeathPanel);
        }
    }

    // Method to show the death panel when the player dies
    private void ShowDeathPanel()
    {
        deathPanel.SetActive(true);  // Activate the death panel
    }

    // Method to restart the level
    private void RestartLevel()
    {
        sceneManager.RestartLevel();  // Call the RestartLevel method from SceneManager
    }

    // Method to return to the main menu
    private void ReturnToMainMenu()
    {
        sceneManager.LoadMainMenu();  // Call the LoadMainMenu method from SceneManager
    }
}
