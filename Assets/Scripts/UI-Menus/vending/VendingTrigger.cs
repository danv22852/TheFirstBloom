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
        GameManager.lastPlayerPosition = player.transform.position; // save position
        vendingUI.SetActive(true);
        Time.timeScale = 0f;
        Debug.Log("paused time " + Time.timeScale);
        player.canMove = false;
        EventSystem.current.SetSelectedGameObject(null);
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