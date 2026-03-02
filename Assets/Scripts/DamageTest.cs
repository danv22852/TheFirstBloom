using UnityEngine;
using UnityEngine.SceneManagement;

public class DamageTest : MonoBehaviour
{   private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.TakeDamage(1);
        }
    }
}