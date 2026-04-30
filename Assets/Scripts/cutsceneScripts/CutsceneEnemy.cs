using UnityEngine;
using System.Collections;

public class CutsceneEnemy : MonoBehaviour
{
    public float speed = 7f;
    public bool isChasing = false;
    private Transform playerTransform;
    
    // --- NEW: Reference to your PlayerController ---
    private PlayerController playerController;

    public Vector2 targetCoordinates;

    [Header("Cutscene Dialogue")]
    public DialogueLine[] encounterDialogue;

    void Start()
    {
        // Find the player and their controller script first
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerController = playerObj.GetComponent<PlayerController>(); // Get the script
        }

        // Check the tutorial status immediately when the scene loads
        if (GameManager.Instance.playerData.finishedTutorial)
        {
            if(gameObject.name == "Symbiote") 
            {
               // Trigger the dialogue
               PlayCutscene();
            }

            if(gameObject.name == "CutsceneEnemy") 
            {
                Debug.Log("Tutorial already finished, placing CutsceneEnemy at target coordinates.");
                gameObject.SetActive(true); 
                transform.position = targetCoordinates;
            }
        }
        else
        {
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

    private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        PlayCutscene();
    }
}

    public void Appear()
    {
        gameObject.SetActive(true);
    }

    private void PlayCutscene()
    {
        // --- NEW: Lock player movement when the cutscene starts ---
        playerController.canMove = false; // Disable player movement
        Time.timeScale = 0f; // Pause the game

        if (encounterDialogue != null && encounterDialogue.Length > 0)
        {
            // Start dialogue and pass StartChasing to run when it finishes
            DialogueManager.instance.StartDialogue(encounterDialogue, StartChasing);
        }
        
    }

    public void StartChasing()
    {
        isChasing = true;

       
    }
}