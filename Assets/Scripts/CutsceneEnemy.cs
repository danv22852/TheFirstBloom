using UnityEngine;

public class CutsceneEnemy : MonoBehaviour
{
    public float speed = 7f;
    public bool isChasing = false;
    private Transform playerTransform;

    void Start()
    {
        // Start hidden if you want them to "appear" later
        gameObject.SetActive(false); 
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
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