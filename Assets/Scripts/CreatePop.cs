using UnityEngine;

public class CreatePop : MonoBehaviour
{
    private PopupManager popupManager;

    void Start()
    {
        // Find the manager on the same object (GameManager)
        popupManager = GetComponent<PopupManager>();

        if (popupManager != null)
        {
            // You MUST pass a string here because your PopupManager.cs requires it
            popupManager.ShowPopup("Welcome to the Tutorial!!\nMake it to the door on the right side.");
            
            // Optional: Pause the game so the player has to read it
            Time.timeScale = 0f; 
        }
    }
}