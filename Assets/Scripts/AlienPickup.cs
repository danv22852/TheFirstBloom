using UnityEngine;

public class AlienPickup : MonoBehaviour
{
    // The color you want the player to turn (e.g., a glowy green or blue)
    public Color pickupTint = Color.gray;

    private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        GameManager.Instance.hasAlien = true;

        // Tell the player to update their look immediately
        var playerScript = other.GetComponent<PlayerController>();
        if (playerScript != null)
        {
            playerScript.UpdateAppearance();
        }

        gameObject.SetActive(false);
    }
}
}   