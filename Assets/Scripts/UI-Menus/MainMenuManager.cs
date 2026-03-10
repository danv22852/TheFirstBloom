using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management

public class MainMenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        // Loads the next scene in the build settings (your game scene)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        // Quits the game (only works in a built application, not the Unity editor)
        Application.Quit();
        Debug.Log("Game Quit"); // Optional: for testing in the editor
    }

    // Add other functions for Options, etc.
    public void OpenOptionsPanel(GameObject optionsPanel)
    {
        optionsPanel.SetActive(true);
    }
}
