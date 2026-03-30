using UnityEngine;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Core Setup")]
    public GameObject pauseCanvas;
    private bool isPaused = false;

    [Header("Pause Tabs")]
    public GameObject navigationPanel; // The main buttons
    public GameObject statsPanel;
    public GameObject bloomPanel;
    public GameObject inventoryPanel;
    public GameObject optionsPanel;

    void Update()
    {
        // Toggle the menu when the player hits Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        pauseCanvas.SetActive(true);
        Time.timeScale = 0f; // Freezes the game world completely!
        
        // Default to showing just the navigation buttons
        ShowNavigationOnly(); 
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseCanvas.SetActive(false);
        Time.timeScale = 1f; // Unfreezes the game!
    }

    // --- TAB SWITCHING ---
    public void ShowNavigationOnly()
    {
        statsPanel.SetActive(false);
        bloomPanel.SetActive(false);
        inventoryPanel.SetActive(false);
        optionsPanel.SetActive(false);
    }

    public void OpenStatsTab() { ShowNavigationOnly(); statsPanel.SetActive(true); }
    public void OpenBloomTab() { ShowNavigationOnly(); bloomPanel.SetActive(true); }
    public void OpenInventoryTab() { ShowNavigationOnly(); inventoryPanel.SetActive(true); }
    public void OpenOptionsTab() { ShowNavigationOnly(); optionsPanel.SetActive(true); }
}