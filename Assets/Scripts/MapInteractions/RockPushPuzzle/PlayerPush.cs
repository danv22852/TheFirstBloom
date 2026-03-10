using UnityEngine;

public class PlayerPush : MonoBehaviour
{
    public float reachDistance = 1.1f;
    public LayerMask rockLayer; // Set this to "Rocks" or "Obstacles"
    
    private Vector2 lastFacingDir = Vector2.down;

    void Update()
    {
        UpdateFacingDirection();

        if (Input.GetKeyDown(KeyCode.O) || Input.GetKeyDown(KeyCode.Z)) // O key or X button on controller
        {
            TryPushRock();
        }
    }

    void UpdateFacingDirection()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        // Update direction only when moving to remember where we last looked
        if (x != 0 || y != 0)
        {
            if (Mathf.Abs(x) > Mathf.Abs(y))
                lastFacingDir = new Vector2(x, 0);
            else
                lastFacingDir = new Vector2(0, y);
        }
    }

    void TryPushRock()
    {
        // Shoot a raycast from the player in the direction they are facing
        RaycastHit2D hit = Physics2D.Raycast(transform.position, lastFacingDir, reachDistance, rockLayer);

        if (hit.collider != null)
        {
            PushableRock rock = hit.collider.GetComponent<PushableRock>();
            if (rock != null)
            {
                rock.Push(lastFacingDir);
            }
        }
    }
}