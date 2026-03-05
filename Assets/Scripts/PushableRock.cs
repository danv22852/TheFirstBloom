using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PushableRock : MonoBehaviour
{
    public float moveTime = 0.1f; // Time to move one tile
    public Vector3 initialPosition;

    private bool isMoving = false;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        initialPosition = transform.position;
    }

    // Called by the player to push
    public void Push(Vector2 direction)
    {
        if (!isMoving)
        {
            // Only allow cardinal directions
            if (Mathf.Abs(direction.x) > 0) direction.y = 0;
            else if (Mathf.Abs(direction.y) > 0) direction.x = 0;
            else return;

            Vector3 targetPos = transform.position + (Vector3)direction;

            // Check if space is free
            if (CanMoveTo(targetPos))
            {
                StartCoroutine(Move(targetPos));
            }
        }
    }

    private bool CanMoveTo(Vector3 target)
    {
        // Overlap check to prevent moving into walls or other rocks
        Collider2D hit = Physics2D.OverlapBox(target, Vector2.one * 0.8f, 0f);
        if (hit == null || hit.CompareTag("Player"))
            return true;

        return false; // blocked
    }

    private IEnumerator Move(Vector3 target)
    {
        isMoving = true;
        Vector3 start = transform.position;
        float elapsed = 0f;

        while (elapsed < moveTime)
        {
            transform.position = Vector3.Lerp(start, target, elapsed / moveTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
        isMoving = false;
    }

    public void ResetPosition()
    {
        transform.position = initialPosition;
    }
}