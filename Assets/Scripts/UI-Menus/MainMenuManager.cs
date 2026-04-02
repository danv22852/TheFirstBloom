using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO; // --- NEW: Required for reading and writing JSON files! ---

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainPanel;
    public GameObject saveSelectPanel;
    public GameObject optionsPanel;

    private void Start()
    {
        // Ensure we start on the main screen
        ShowMainPanel();
    }

    // --- PANEL NAVIGATION ---

    public void ShowMainPanel()
    {
        mainPanel.SetActive(true);
        saveSelectPanel.SetActive(false);
        optionsPanel.SetActive(false);
    }

    public void ShowSaveSelect()
    {
        mainPanel.SetActive(false);
        saveSelectPanel.SetActive(true);
    }

    public void ShowOptions()
    {
        mainPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    // --- JSON SAVE SLOT LOGIC ---

    public void LoadSaveSlot(int slotNumber)
    {
        Debug.Log("Attempting to load Save Slot: " + slotNumber);

        // 1. Define exactly where the save file should live on the computer
        // Application.persistentDataPath is a safe folder Unity creates for save files (works on PC, Mac, Mobile, etc.)
        string savePath = Application.persistentDataPath + "/saveData_" + slotNumber + ".json";

        // 2. Check if the file actually exists
        if (File.Exists(savePath))
        {
            // 3. Read the JSON text
            string json = File.ReadAllText(savePath);

            // 4. Convert it back to PlayerData and give it to the GameManager!
            if (GameManager.Instance != null)
            {
                // We overwrite the current PlayerData with the saved JSON data
                JsonUtility.FromJsonOverwrite(json, GameManager.Instance.playerData);
                
                // Tell the GameManager which slot we are using, so auto-saves overwrite the correct file!
                // (Note: You will need to add "public int currentSaveSlot = 1;" to your GameManager script!)
                // GameManager.Instance.currentSaveSlot = slotNumber;

                // Load the exact floor they saved on!
                string floorToLoad = GameManager.Instance.playerData.floorName;
                SceneManager.LoadScene(string.IsNullOrEmpty(floorToLoad) ? "firstFloor" : floorToLoad);
            }
            else
            {
                // Fallback for testing
                SceneManager.LoadScene("firstFloor");
            }
        }
        else
        {
            Debug.Log("No save file found in slot " + slotNumber + "! Starting a new game instead.");
            
            // If there's no save file, we treat it as starting a brand new game in this slot
            StartNewGameInSlot(slotNumber);
        }
    }

    public void StartNewGameInSlot(int slotNumber)
    {
        Debug.Log("Starting New Game in Slot: " + slotNumber);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetSaveData(); 
            // GameManager.Instance.currentSaveSlot = slotNumber; // Remember which slot to save into!
        }
        
        SceneManager.LoadScene("firstFloor");
    }

    // Generic New Game Button (Defaults to Slot 1 if you don't want to use slots right away)
    public void OnNewGameButton()
    {
        StartNewGameInSlot(1);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}