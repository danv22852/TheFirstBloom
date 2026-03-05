using UnityEngine;

public class AlienPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.hasAlien = true;
            gameObject.SetActive(false);
        }
    }
}