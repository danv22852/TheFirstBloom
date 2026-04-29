using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyEncounter : MonoBehaviour
{
    [Header("1. Map Identity (For Graveyard)")]
    public string uniqueEnemyID;

    [Header("2. Combat Data")]
    public EnemyData enemyType;

    private bool engaged = false;

    private void Start()
    {
        // 🪦 Graveyard check: remove enemy if already defeated
        if (GameManager.Instance != null &&
            GameManager.Instance.playerData.defeatedEnemies.Contains(uniqueEnemyID))
        {
            Destroy(gameObject);
            return;
        }

        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            Engage(collision.transform);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            Engage(other.transform);
    }

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

    /// <summary>
    /// Starts combat encounter
    /// </summary>
    public void Engage(Transform player)
    {
        if (engaged || player == null) return;

        engaged = true;

        Debug.Log("ENEMY ENGAGED: " + uniqueEnemyID);

        // Store return position after combat
        GameManager.lastPlayerPosition = player.position;
        GameManager.isReturningFromCombat = true;

        // Pass enemy data into combat system
        if (enemyType != null)
            GameManager.pendingEnemyData = enemyType;

        // Store ID for graveyard system
        GameManager.encounteredInstanceID = uniqueEnemyID;

        Debug.Log("Encountered Enemy: " + uniqueEnemyID);

        // Load correct combat scene
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
}