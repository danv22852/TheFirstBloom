using UnityEngine;
using Unity.Cinemachine;
public class AlienPickup : MonoBehaviour
{
    // The color you want the player to turn (e.g., a glowy green or blue)
    public Color pickupTint = Color.gray;

    [SerializeField] PolygonCollider2D targetTransition;
    private CinemachineConfiner2D confiner;

    private void Awake() { 
        confiner = FindFirstObjectByType<CinemachineConfiner2D>();
    }


     void Start()
    {
       
        // Check the tutorial status immediately when the scene loads
        if (GameManager.Instance.playerData.finishedTutorial && GameObject.Find("Symbiote") != null)
        {
            
             UpdateCameraBoundary(targetTransition);
        }
        {
            UpdateCameraBoundary(targetTransition);
        }
    }

    private void UpdateCameraBoundary(PolygonCollider2D newBoundary) {
        if (confiner != null) {
            GameManager.currentMapBoundaryName = targetTransition.gameObject.name;
            confiner.BoundingShape2D = newBoundary;
            confiner.InvalidateBoundingShapeCache();
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        GameManager.Instance.playerData.hasAlien = true;

        // Tell the player to update their look immediately
        var playerScript = other.GetComponent<PlayerController>();
        if (playerScript != null)
        {
            playerScript.UpdateAppearance();
        }

        gameObject.SetActive(false);
    }
}
}   