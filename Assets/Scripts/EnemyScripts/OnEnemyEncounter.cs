using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyEncounter : MonoBehaviour 
{
    [Header("1. Map Identity (For Graveyard)")]
    [Tooltip("MUST be unique for every single enemy on the map! e.g., Slime_Room1_A")]
    public string uniqueEnemyID;

    [Header("2. Combat Data (For Sprite/Stats)")]
    [Tooltip("Drag the ScriptableObject for this enemy type here!")]
    public EnemyData enemyType;

    private void Start()
    {
        // When the Overworld loads, check if this unique map ID is in the graveyard
        if (GameManager.Instance != null && GameManager.Instance.playerData.defeatedEnemies.Contains(uniqueEnemyID))
        {
            // If it is, destroy this object immediately before the player even sees it
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Calculate safe spawn zone
            Vector3 pushDirection = (other.transform.position - transform.position).normalized;
            float safeDistance = 1.5f;
            GameManager.lastPlayerPosition = other.transform.position + (pushDirection * safeDistance);
            GameManager.isReturningFromCombat = true;

            // --- THE NEW DIRECT HAND-OFF ---
            if (enemyType != null)
            {
                // We physically hand the ScriptableObject to the GameManager!
                GameManager.pendingEnemyData = this.enemyType; 
            }
            
            // We still pass the map ID so the graveyard works later
            GameManager.encounteredInstanceID = this.uniqueEnemyID; 

            // Load the correct scene
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