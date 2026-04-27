using UnityEngine;
using UnityEngine.EventSystems;

public class VendingTrigger : MonoBehaviour
{
    public PlayerController player;
    public GameObject vendingUI; // Drag the Vending Canvas here
    private bool playerInRange = false;

    void Update()
    {
        // Using 'Z' to open the menu
        if (playerInRange && (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.O)))
        {
            OpenVendingUI();
        }
    }

    public void OpenVendingUI()
{
    vendingUI.SetActive(true);
    Time.timeScale = 0f;

    // FIND THE FIRST BUTTON (e.g., Button A)
    // Make sure 'firstButton' is assigned in the Inspector!
    GameObject firstButton = vendingUI.transform.Find("Path/To/ButtonA").gameObject; 
    
    // Clear current selection and set the new one
    EventSystem.current.SetSelectedGameObject(null);
    EventSystem.current.SetSelectedGameObject(firstButton);
}

    public void CloseVendingUI()
    {
        vendingUI.SetActive(false);
        Time.timeScale = 1f; // Resume the game world
         player.canMove = true;
    
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }
}