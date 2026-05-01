using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;
    public PlayerController player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        menuCanvas.SetActive(false);
        Time.timeScale = 1f;
        if (player != null)
            player.canMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("Menu toggled. Active: " + menuCanvas.activeSelf);
            menuCanvas.SetActive(!menuCanvas.activeSelf);
            if(menuCanvas.activeSelf)
            {
                Time.timeScale = 0f; // Pause the game
                if (player != null)
                 {
            // Turn movement off when menu is open, on when closed
                  player.canMove = false;
                 }
            }
            else
            {
                Time.timeScale = 1f; // Resume the game
                player.canMove = true;
            }
        }
    }
}
