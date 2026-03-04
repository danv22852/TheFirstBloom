using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyEncounter : MonoBehaviour
{
    
    public string combatSceneName = "CombatUI";

    private void OnTriggerEnter2D(Collider2D other)
    {
        // We check if the object that bumped into us has the "Player" tag
        if (other.CompareTag("Player"))
        {
            // Prints a message to the Console so you know it worked
            Debug.Log("Enemy Encountered! Switching to Combat Scene...");

            // This line actually loads the new scene
            SceneManager.LoadScene(combatSceneName);
        }
    }
}