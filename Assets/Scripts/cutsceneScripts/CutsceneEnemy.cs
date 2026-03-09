using UnityEngine;

public class CutsceneEnemy : MonoBehaviour
{
    public float speed = 7f;
    public bool isChasing = false;
    private Transform playerTransform;

    public Vector2 targetCoordinates;

    

    void Start()
    {
        // Find the player first
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        // Check the tutorial status immediately when the scene loads
        if (GameManager.finishedTutorial)
        {
            // If the tutorial is already done, don't hide! Just start chasing.
            if(gameObject.name == "Symbiote") {
               
                StartChasing();
            }

            if(gameObject.name == "CutsceneEnemy") {
                Debug.Log("Tutorial already finished, placing CutsceneEnemy at target coordinates.");
                gameObject.SetActive(true); 
                transform.position = targetCoordinates;
            }

        }
        else
        {
            // Only hide the enemy if the tutorial is NOT finished yet
            gameObject.SetActive(false); 
        }
    }

    void Update()
    {
        if (isChasing && playerTransform != null)
        {
            // Move towards the player position
            transform.position = Vector2.MoveTowards(
                transform.position, 
                playerTransform.position, 
                speed * Time.deltaTime
            );
        }
    }

    public void Appear()
    {
        gameObject.SetActive(true);
    }

    public void StartChasing()
    {
        isChasing = true;
    }
}