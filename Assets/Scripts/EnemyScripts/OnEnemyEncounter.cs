using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyEncounter : MonoBehaviour
{
    [Header("1. Map Identity (For Graveyard)")]
    public string uniqueEnemyID;

    [Header("2. Combat Data")]
    public EnemyData enemyType;

    private bool engaged = false;

    private void OnTriggerEnter2D(Collider2D collision)
{
    // Check if the thing hitting us is the Player
    if (collision.CompareTag("Player"))
    {
        Engage(collision.transform);    
    }
    }
    private void Start()
    {
        // 1) Graveyard Check: If we already killed this exact enemy, delete it immediately.
        if (GameManager.Instance != null &&
            GameManager.Instance.playerData.defeatedEnemies.Contains(uniqueEnemyID))
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// Call this from chase logic when the enemy "catches" the player, 
    /// or when the player's attack hitbox strikes the enemy.
    /// </summary>
    public void Engage(Transform player)
    {
        Debug.Log("ENEMY ENGAGED: " + uniqueEnemyID);
        if (engaged || player == null) return;

        engaged = true;

        // Save the player's exact safe position to drop them back here after combat
        GameManager.lastPlayerPosition = player.position;
        GameManager.isReturningFromCombat = true;

        // Direct hand-off of the enemy's stat block
        if (enemyType != null)
            GameManager.pendingEnemyData = enemyType;

        // Tell the GameManager which specific enemy we are fighting so it can be added to the Graveyard if we win
        GameManager.encounteredInstanceID = uniqueEnemyID;
        Debug.Log("Encountered Enemy: " + uniqueEnemyID);
        // Load correct scene
        if (GameManager.Instance != null &&
            GameManager.Instance.playerData.finishedTutorial == false)
        {
            SceneManager.LoadScene("TutorialBattle");
        }
        else
        {
            SceneManager.LoadScene("CombatUI");
        }
    }

    // --- COLLISION DETECTION (AMBUSH TRIGGERS) ---

    // 1. If the enemy's collider is set to "Is Trigger"

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            Engage(other.transform);
    }

    // 2. If the enemy's collider is solid (Physical bump)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            Engage(collision.transform);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            Engage(collision.transform);
    }
}