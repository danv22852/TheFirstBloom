using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetRun : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        Debug.Log("Reset for the New Run");

        GameManager.Instance.playerData.ResetForNewRun();

        // reset static game state
        GameManager.isReturningFromCombat = false;
        GameManager.lastPlayerPosition = Vector3.zero;
        GameManager.currentEnemyID = "";
        GameManager.currentMapBoundaryName = null;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
}
