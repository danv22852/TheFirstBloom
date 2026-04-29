using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainPanel;
    public GameObject optionsPanel;
    // Removed the saveSelectPanel reference entirely

    private void Start()
    {
        ShowMainPanel();
    }

    // --- PANEL NAVIGATION ---

    public void ShowMainPanel()
    {
        mainPanel.SetActive(true);
        optionsPanel.SetActive(false);
    }

    public void ShowOptions()
    {
        mainPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    // --- SINGLE FILE JSON SAVE LOGIC ---

    public void ContinueGame()
    {
        Debug.Log("Attempting to load saved game...");

        // 1. Point directly to one master save file
        string savePath = Application.persistentDataPath + "/saveData.json";

        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);

            if (GameManager.Instance != null)
            {
                JsonUtility.FromJsonOverwrite(json, GameManager.Instance.playerData);
                
                string floorToLoad = GameManager.Instance.playerData.floorName;
                SceneManager.LoadScene(string.IsNullOrEmpty(floorToLoad) ? "firstFloor" : floorToLoad);
            }
            else
            {
                SceneManager.LoadScene("firstFloor");
            }
        }
        else
        {
            Debug.Log("No save file found! Starting a new game instead.");
            StartNewGame();
        }
    }

    public void StartNewGame()
{
    Debug.Log("Starting New Game...");

    if (GameManager.Instance != null)
    {
        var pd = GameManager.Instance.playerData;

        // 🔥 HARD RESET FIRST
        pd.ResetForNewRun();

        // 🔥 THEN immediately wipe save file so it cannot override reset
        string savePath = Application.persistentDataPath + "/saveData.json";
        if (System.IO.File.Exists(savePath))
        {
            System.IO.File.Delete(savePath);
        }
    }

    SceneManager.LoadScene("firstFloor");
}

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}