using UnityEngine;
using UnityEngine.SceneManagement;

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

    // --- BUTTON FUNCTIONS ---

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

    // You will link this to your actual Save Slot buttons later!
    public void LoadSaveSlot(int slotNumber)
    {
        Debug.Log("Loading Save Slot: " + slotNumber);
        // We will add the JSON save loading logic here later
        SceneManager.LoadScene("firstFloor"); 
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}