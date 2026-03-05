using UnityEngine;

public class PlayerPush : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // Optional: prevent diagonal movement
        if (moveInput.x != 0) moveInput.y = 0;
    }

    void FixedUpdate()
    {
        // Move the player
        Vector2 newPos = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);

        // Only attempt to push if player is actually moving
        if (moveInput != Vector2.zero)
        {
            // Check one tile ahead for rocks
            Vector2 checkPos = rb.position + moveInput; // one unit ahead
            Collider2D hit = Physics2D.OverlapBox(checkPos, Vector2.one * 0.8f, 0f);

            if (hit != null && hit.CompareTag("PushableRock"))
            {
                hit.GetComponent<PushableRock>().Push(moveInput);
            }
        }
    }
}