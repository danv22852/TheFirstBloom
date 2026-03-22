using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [Header("Wander Area")]
    public BoxCollider2D area;         // Drag PatrolArea here
    public float arriveDistance = 0.2f;

    [Header("Timing")]
    public float minChooseTime = 1.0f;
    public float maxChooseTime = 2.5f;

    [Header("Wall Avoidance")]
    public LayerMask wallMask;
    public float probeRadius = 0.15f;

    [Header("Movement")]
    public float speed = 2.0f;
    public float waitAtPointSeconds = 0.0f;

    private Rigidbody2D rb;
    private Vector2 target;
    private float chooseTimer = 0f;
    private float waitTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }
    void FixedUpdate()
{
    if (area == null)
    {
        rb.linearVelocity = Vector2.zero;
        return;
    }

    // wait timer (optional pause)
    if (waitTimer > 0f)
    {
        waitTimer -= Time.fixedDeltaTime;
        rb.linearVelocity = Vector2.zero;
        return;
    }

    chooseTimer -= Time.fixedDeltaTime;

    Vector2 pos = rb.position;
    Vector2 toTarget = target - pos;

    // Pick a new target if: arrived OR time expired
    if (toTarget.magnitude <= arriveDistance || chooseTimer <= 0f)
    {
        PickNewTarget();
        rb.linearVelocity = Vector2.zero;
        return;
    }

    Vector2 dirMove = toTarget.normalized;
    rb.linearVelocity = dirMove * speed;

    // Optional: if about to hit a wall, pick a new target
    if (Physics2D.OverlapCircle(pos + dirMove * 0.25f, probeRadius, wallMask))
    {
        PickNewTarget();
        rb.linearVelocity = Vector2.zero;
    }
}

void PickNewTarget()
{
    // try multiple candidates so we don't pick inside a wall
    for (int i = 0; i < 20; i++)
    {
        Vector2 candidate = RandomPointInArea();
        if (!Physics2D.OverlapCircle(candidate, probeRadius, wallMask))
        {
            target = candidate;
            chooseTimer = Random.Range(minChooseTime, maxChooseTime);

            // optional pause at each new target
            if (waitAtPointSeconds > 0f) waitTimer = waitAtPointSeconds;

            return;
        }
    }

    // fallback if no valid point found
    target = rb.position;
    chooseTimer = 0.5f;
}

Vector2 RandomPointInArea()
{
    Bounds b = area.bounds;
    return new Vector2(
        Random.Range(b.min.x, b.max.x),
        Random.Range(b.min.y, b.max.y)
    );
}

#if UNITY_EDITOR
void OnDrawGizmosSelected()
{
    // Draw the wander area
    if (area != null)
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(area.bounds.center, area.bounds.size);
    }

    // Draw current target
    Gizmos.color = Color.green;
    Gizmos.DrawSphere(target, 0.08f);
}
#endif
    public void TakeDamage(float damage)
    {
        Debug.Log("Enemy took " + damage + " damage!");
    }
}