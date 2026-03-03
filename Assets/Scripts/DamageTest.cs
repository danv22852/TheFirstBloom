using UnityEngine;
using UnityEngine.SceneManagement;

public class DamageTest : MonoBehaviour
{   private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager is NULL!");
            return;
        }

        GameManager.Instance.TakeDamage(1);
    }
}
}

