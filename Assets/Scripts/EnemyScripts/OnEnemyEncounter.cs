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
        if (engaged || player == null) return;

        engaged = true;

        // Save position for return
        GameManager.lastPlayerPosition = player.position;
        GameManager.isReturningFromCombat = true;

        // Pass enemy data
        GameManager.pendingEnemyData = enemyType;
        GameManager.encounteredInstanceID = uniqueEnemyID;

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