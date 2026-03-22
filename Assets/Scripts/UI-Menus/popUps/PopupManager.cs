using UnityEngine;
using UnityEngine.UI; // Required for UI components
using TMPro; // Required if using TextMeshPro

public class PopupManager : MonoBehaviour
{
    public GameObject popupPanel; // Reference to the PopupPanel GameObject
    public TextMeshProUGUI messageText; // Reference to the message Text component (if using TextMeshPro)
    // Add references to buttons if needed

    // Call this method to show the popup
    public void ShowPopup(string message)
{
    if (popupPanel != null)
    {
        messageText.text = message;
        popupPanel.SetActive(true); 
        Time.timeScale = 0f; // This is why your character stops!
    }
}

    // Call this method to hide the popup
    public void HidePopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false); // Disable the panel to hide it
            // You can also resume the game here: Time.timeScale = 1f;
            Time.timeScale = 1f;
        }
    }
}
