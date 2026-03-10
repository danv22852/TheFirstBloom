using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyEncounter : MonoBehaviour // (Or whatever you named this script)
{
    [Header("Enemy Identity")]
    // You MUST type a unique name for every single enemy in the Unity Inspector!
    public string uniqueEnemyID = "Slime_01";

    private void Start()
    {
        // When the Overworld loads, check if this enemy's ID is in the graveyard
        if (GameManager.Instance.playerData.defeatedEnemies.Contains(uniqueEnemyID))
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

            // --- NEW: Tell the GameManager exactly who we are fighting ---
            GameManager.currentEnemyID = this.uniqueEnemyID;
            if (!GameManager.Instance.playerData.finishedTutorial){
                Debug.Log("Starting tutorial battle...");
                SceneManager.LoadScene("TutorialBattle");
            }
            else SceneManager.LoadScene("CombatUI");
        }
    }
}