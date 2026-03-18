using UnityEngine;
using UnityEngine.UI;

public class CombatTutorial : MonoBehaviour
{
    [Header("UI References")]
    public GameObject tutorialPanel; // The visual popup
    public Button closeButton;       // The button to dismiss it

    [Header("Settings")]
    public string saveKey = "FirstTimeCombat";

    private void Start()
    {
        // If the key exists and is set to 1, we've seen this. Destroy the tutorial object.
        if (PlayerPrefs.GetInt(saveKey, 0) == 1)
        {
            Destroy(gameObject);
            return;
        }

        // Otherwise, show the tutorial and pause the game
        ShowTutorial();
    }

    void ShowTutorial()
    {
        tutorialPanel.transform.SetAsLastSibling(); 
    
        tutorialPanel.SetActive(true);
        Time.timeScale = 0f; // Freeze combat animations/logic

        // Ensure the close button is the one selected for controller/keyboard users
        closeButton.Select();
        
        closeButton.onClick.AddListener(OnDismiss);
    }

    void OnDismiss()
    {
        // Save that we've seen it
        PlayerPrefs.SetInt(saveKey, 1);
        PlayerPrefs.Save();

        // Resume game and clean up
        Time.timeScale = 1f;
        tutorialPanel.SetActive(false);
        Destroy(gameObject); 
    }

    [ContextMenu("Reset Tutorial Flag")]
    public void ResetTutorial()
    {
    PlayerPrefs.DeleteKey(saveKey);
    Debug.Log("Tutorial Reset! It will show up next time you play.");
    }
}