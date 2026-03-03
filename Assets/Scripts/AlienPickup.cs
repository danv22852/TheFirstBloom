using UnityEngine;

public class AlienPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // make sure your player has "Player" tag
        {
            GameManager.Instance.hasAlien = true;
            Debug.Log("Alien acquired!");
            
            // Optional: disable or destroy pickup
            gameObject.SetActive(false);
            // Or use Destroy(gameObject);
        }
    }
}