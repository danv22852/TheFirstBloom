using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyEncounter : MonoBehaviour
{
    [Header("1. Map Identity (For Graveyard)")]
    public string uniqueEnemyID;

    [Header("2. Combat Data (For Sprite/Stats)")]
    public EnemyData enemyType;

    [Header("Auto-Engage")]
    public float gracePeriodSeconds = 1.5f;

    private bool engaged = false;

    private void Start()
    {
        // 1) Graveyard Check
        if (GameManager.Instance != null &&
            GameManager.Instance.playerData.defeatedEnemies.Contains(uniqueEnemyID))
        {
            Destroy(gameObject);
            return;
        }

        // 2) Grace period after returning from combat (prevents instant re-trigger)
        if (GameManager.isReturningFromCombat)
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            Invoke(nameof(EnableCollider), gracePeriodSeconds);
        }
    }

    private void EnableCollider()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        GameManager.isReturningFromCombat = false;
    }

    /// <summary>
    /// Call this from chase logic when the enemy "catches" the player.
    /// </summary>
    public void Engage(Transform player)
    {
        if (engaged) return;
        if (player == null) return;

        // If we are still in grace period, don't engage
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.enabled) return;

        engaged = true;

        // Save the player's exact safe position (your "wall fix")
        GameManager.lastPlayerPosition = player.position;
        GameManager.isReturningFromCombat = true;

        // Direct hand-off
        if (enemyType != null)
            GameManager.pendingEnemyData = enemyType;

        GameManager.encounteredInstanceID = uniqueEnemyID;

        // Load scene
        if (!GameManager.Instance.playerData.finishedTutorial)
            SceneManager.LoadScene("TutorialBattle");
        else
            SceneManager.LoadScene("CombatUI");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            Engage(other.transform);
    }

    // Optional: makes it even more reliable (fast movement / already overlapping)
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            Engage(other.transform);
    }
}