using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CombatTutorial : MonoBehaviour
{
    [Header("UI References")]
    public GameObject tutorialPanel;
    public Button closeButton; 

    private bool isTutorialActive = false;

    private void Start()
    { 
        Debug.Log("Current floor: " + GameManager.Instance.playerData.floorName);
        if (GameManager.Instance.playerData.floorName == "firstFloor")
        {
            Debug.Log("checked floor: " + GameManager.Instance.playerData.floorName);
            ShowTutorial();
        }
        else
        {
            // If it's not the first floor or they've seen it, delete this logic
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // 2. Hardcoded Key Check (Z or O)
        if (isTutorialActive)
        {
            if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.O))
            {
                OnDismiss();
            }
        }
    }

    void ShowTutorial()
    {
        isTutorialActive = true;
        Time.timeScale = 0f; // Freeze game
        tutorialPanel.SetActive(true);
        
        // This makes the button "Selected" (Red) immediately
        if (closeButton != null)
        {
            closeButton.Select();
        }
    }

    // 3. The Click Handler (Must be PUBLIC)
    public void OnDismiss()
    {
        // Save progress so it doesn't show again
        PlayerPrefs.SetInt("FirstFloorTutorialDone", 1);
        PlayerPrefs.Save();
        
        // Resume game and cleanup
        Time.timeScale = 1f;
        isTutorialActive = false;
        tutorialPanel.SetActive(false);
        Destroy(gameObject);
    }
}