using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyEncounter : MonoBehaviour 
{
    [Header("1. Map Identity (For Graveyard)")]
    public string uniqueEnemyID;

    [Header("2. Combat Data (For Sprite/Stats)")]
    public EnemyData enemyType;

    private void Start()
    {
        // 1. Graveyard Check
        if (GameManager.Instance != null && GameManager.Instance.playerData.defeatedEnemies.Contains(uniqueEnemyID))
        {
            Destroy(gameObject);
            return; // Stop running this script if the enemy is dead!
        }

        // 2. THE GRACE PERIOD FIX
        // If we just came back from a battle (like fleeing), disable this enemy's 
        // trigger collider for 1.5 seconds so the player has time to walk away!
        if (GameManager.isReturningFromCombat)
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
            
            Invoke(nameof(EnableCollider), 1.5f);
        }
    }

    private void EnableCollider()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
        
        // Reset the global flag so enemies act normal again
        GameManager.isReturningFromCombat = false; 
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // --- THE WALL FIX ---
            // No more pushback math! Just save the player's exact, physical location.
            // Since they weren't in a wall when they touched the enemy, this spot is guaranteed safe.
            GameManager.lastPlayerPosition = other.transform.position;
            GameManager.isReturningFromCombat = true;

            // Direct Hand-Off
            if (enemyType != null)
            {
                GameManager.pendingEnemyData = this.enemyType; 
            }
            GameManager.encounteredInstanceID = this.uniqueEnemyID; 

            // Load Scene
            if (!GameManager.Instance.playerData.finishedTutorial)
            {
                SceneManager.LoadScene("TutorialBattle");
            }
            else 
            {
                SceneManager.LoadScene("CombatUI");
            }
        }
    }
}