using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    public float speed = 2.0f;
    [Tooltip("How many tiles the enemy will walk before turning around.")]
    public float patrolDistance = 3.0f; 
    
    private float startX;
    private bool movingRight = true;

    void Start()
    {
        // Memorize the exact tile the enemy starts on
        startX = transform.position.x;
    }

    void Update()
    {
        if (movingRight)
        {
            // Walk Right
            transform.Translate(Vector2.right * speed * Time.deltaTime);
            
            // If we have walked 3 full tiles to the right, turn around
            if (transform.position.x >= startX + patrolDistance)
            {
                // Snap to the exact distance to prevent drifting over time
                transform.position = new Vector3(startX + patrolDistance, transform.position.y, transform.position.z);
                FlipSprite();
            }
        }
        else
        {
            // Walk Left
            transform.Translate(Vector2.left * speed * Time.deltaTime);
            
            // If we have arrived back at our starting tile, turn around
            if (transform.position.x <= startX)
            {
                // Snap to the exact starting tile
                transform.position = new Vector3(startX, transform.position.y, transform.position.z);
                FlipSprite();
            }
        }
    }

    private void FlipSprite()
    {
        // Swap our internal direction boolean
        movingRight = !movingRight;

        // Physically flip the sprite 
        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1; 
        transform.localScale = currentScale;
    }
}