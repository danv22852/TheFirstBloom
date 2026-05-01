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
            var playerData = GameManager.Instance.playerData;

            // 🔥 1. HARD RESET IN-MEMORY DATA FIRST
            if (playerData != null)
            {
                // Empty the Enemy Graveyard
                if (playerData.defeatedEnemies != null)
                {
                    playerData.defeatedEnemies.Clear();
                }

                // Empty the Collected Items list
                if (playerData.collectedItems != null)
                {
                    playerData.collectedItems.Clear();
                }

                // Reset the player's actual inventory and stats back to default!
                playerData.expSystem.level = 1;
                playerData.expSystem.currentEXP = 0;
                playerData.expSystem.expToNextLevel = 150;
                playerData.expSystem.availableSkillPoints = 0;
                
                playerData.currentHP = 100;
                playerData.maxHP = 100;
                playerData.strength = 10;
                playerData.speed = 10;
                playerData.defense = 10;
                playerData.luck = 0;
                
                playerData.healthPotions = 3;
                playerData.wiltPotions = 0;
                playerData.keys = 0;
                playerData.coins = 0;
                
                playerData.floorName = "firstFloor";
                playerData.hasAlien = false;
                playerData.finishedTutorial = false;

                // Reset Bloom Stats to be safe!
                playerData.currentBloom = 0;
                playerData.currentBloomState = BloomState.Stable;

                Debug.Log("Save Data completely reset! All enemies and items will respawn.");
            }
            else
            {
                Debug.LogWarning("Could not reset data: PlayerData is missing.");
            }

            // 🔥 2. THEN immediately wipe save file so it cannot override reset
            string savePath = Application.persistentDataPath + "/saveData.json";
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log("Old save file deleted from hard drive.");
            }
        }

        // 3. Load the starting floor
        SceneManager.LoadScene("firstFloor");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}