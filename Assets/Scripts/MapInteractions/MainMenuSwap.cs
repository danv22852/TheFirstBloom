using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButton : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public void GoToMainMenu()
    {
        // optional: save state before leaving
        if (GameManager.Instance != null)
        {
            GameManager.Instance.playerData.floorName = mainMenuSceneName;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }
}