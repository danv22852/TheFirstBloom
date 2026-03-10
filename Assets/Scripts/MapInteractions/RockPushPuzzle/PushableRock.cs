using UnityEngine;
using System.Collections;

public class PushableRock : MonoBehaviour
{
    public float moveSpeed = 5f;
    public LayerMask blockingLayer; // Set this to "Walls" or "Obstacles"
    private bool isMoving = false;

    public Vector2 targetCoordinates;

    void Start()
    {
        // Check if the tutorial is already done (using finishedTutorial based on your GameManager)
        if (GameManager.Instance.playerData.finishedTutorial)
        {
       
            transform.position = targetCoordinates;
        }
    }

    public void Push(Vector2 direction)
    {
        if (isMoving) return;

        // Check if the path ahead is clear
        if (CanMove(direction))
        {
            StartCoroutine(MoveRoutine(direction));
        }
        else 
        {
            // WALL HIT: Try to bounce back in the opposite direction
            Vector2 bounceDir = -direction;
            
            // Only bounce if the space behind the rock is empty
            if (CanMove(bounceDir))
            {
                StartCoroutine(MoveRoutine(bounceDir));
                Debug.Log("Rock bounced back!");
            }
        }
    }

    private bool CanMove(Vector2 dir)
{
    // 1. Move the starting point of our check to the center of the NEXT tile
    Vector2 targetPos = (Vector2)transform.position + dir;

    // 2. Check if there is anything in that 1x1 square area
    // We use a small radius (0.4) so it doesn't accidentally hit diagonal walls
    Collider2D hit = Physics2D.OverlapCircle(targetPos, 0.4f, blockingLayer);

    // 3. If the "hit" is THIS rock, ignore it and say the path is clear
    if (hit != null && hit.gameObject == gameObject)
    {
        return true;
    }

    // 4. If we hit something else, the path is blocked
    return hit == null;
}
    private IEnumerator MoveRoutine(Vector2 dir)
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + (Vector3)dir;
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        transform.position = endPos; // Ensure perfect grid snapping
        isMoving = false;
    }
}