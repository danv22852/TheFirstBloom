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
        // Remove enemy if already defeated
        if (GameManager.Instance != null &&
            GameManager.Instance.playerData.defeatedEnemies.Contains(uniqueEnemyID))
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// Called ONLY by EnemyPatrol when player is actually caught
    /// </summary>
    public void Engage(Transform player)
    {
        Debug.Log("ENEMY ENGAGED: " + uniqueEnemyID);
        if (engaged || player == null) return;

        engaged = true;

        // Save position for return
        GameManager.lastPlayerPosition = player.position;
        GameManager.isReturningFromCombat = true;

        // Pass enemy data
        GameManager.pendingEnemyData = enemyType;
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
}