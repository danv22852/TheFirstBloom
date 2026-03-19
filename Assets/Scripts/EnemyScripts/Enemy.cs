using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [Header("Patrol Area")]
    public BoxCollider2D area;                 // Assign PatrolArea (BoxCollider2D, IsTrigger)
    public float padding = 0.2f;               // keep away from edges
    public LayerMask obstacleMask;             // Walls + Barrels/Obstacles (anything with colliders)

    [Header("Movement")]
    public float speed = 2.0f;
    public float arriveDistance = 0.25f;

    [Header("Wander Timing")]
    public float minRetargetTime = 0.8f;
    public float maxRetargetTime = 2.0f;

    [Header("Obstacle Probe")]
    public float probeRadius = 0.18f;
    public float probeDistance = 0.35f;

    [Header("Chase (only if player is inside patrol area)")]
    public string playerTag = "Player";
    public float chaseRadius = 4.0f;           // begin chasing when player is within this distance
    public float stopDistance = 1.1f;          // stop this far from player

    private Rigidbody2D rb;
    private Transform player;

    private Vector2 target;
    private float retargetTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null) player = p.transform;

        PickNewTarget();
    }

    void FixedUpdate()
    {
        if (area == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Chase only if the player is inside the patrol area AND within chase radius
        if (player != null && IsInsideArea(player.position) && InRange(rb.position, player.position, chaseRadius))
        {
            Chase();
        }
        else
        {
            Patrol();
        }

        ClampToArea();
    }

    void Chase()
    {
        Vector2 pos = rb.position;
        Vector2 playerPos = player.position;
        Vector2 toPlayer = playerPos - pos;

        if (toPlayer.magnitude <= stopDistance)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dir = toPlayer.normalized;

        // If obstacle ahead, don't freeze: pick a new patrol target and patrol instead
        if (Physics2D.OverlapCircle(pos + dir * probeDistance, probeRadius, obstacleMask))
        {
            PickNewTarget();
            Patrol();
            return;
        }

        rb.linearVelocity = dir * speed;
    }

    void Patrol()
    {
        retargetTimer -= Time.fixedDeltaTime;

        Vector2 pos = rb.position;
        Vector2 toTarget = target - pos;

        // If reached target or timer expired -> new target
        if (toTarget.magnitude <= arriveDistance || retargetTimer <= 0f)
        {
            PickNewTarget();
            toTarget = target - pos;
        }

        if (toTarget.magnitude <= arriveDistance)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dir = toTarget.normalized;

        // If obstacle ahead, repick target (no pausing)
        if (Physics2D.OverlapCircle(pos + dir * probeDistance, probeRadius, obstacleMask))
        {
            PickNewTarget();
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = dir * speed;
    }

    void PickNewTarget()
    {
        // Try multiple points inside the area that aren't inside obstacles
        for (int i = 0; i < 40; i++)
        {
            Vector2 candidate = RandomPointInArea();
            if (!Physics2D.OverlapCircle(candidate, probeRadius, obstacleMask))
            {
                target = candidate;
                retargetTimer = Random.Range(minRetargetTime, maxRetargetTime);
                return;
            }
        }

        // Fallback so it doesn't freeze
        target = RandomPointInArea();
        retargetTimer = 0.5f;
    }

    Vector2 RandomPointInArea()
    {
        Bounds b = area.bounds;

        float minX = b.min.x + padding;
        float maxX = b.max.x - padding;
        float minY = b.min.y + padding;
        float maxY = b.max.y - padding;

        return new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
    }

    bool IsInsideArea(Vector2 p)
    {
        Bounds b = area.bounds;

        return (p.x >= b.min.x + padding && p.x <= b.max.x - padding &&
                p.y >= b.min.y + padding && p.y <= b.max.y - padding);
    }

    bool InRange(Vector2 a, Vector2 b, float r)
    {
        return (a - b).sqrMagnitude <= r * r;
    }

    void ClampToArea()
    {
        Bounds b = area.bounds;

        float minX = b.min.x + padding;
        float maxX = b.max.x - padding;
        float minY = b.min.y + padding;
        float maxY = b.max.y - padding;

        Vector2 p = rb.position;
        Vector2 clamped = new Vector2(
            Mathf.Clamp(p.x, minX, maxX),
            Mathf.Clamp(p.y, minY, maxY)
        );

        if (clamped != p)
        {
            rb.position = clamped;
            rb.linearVelocity = Vector2.zero;
            PickNewTarget();
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (area != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(area.bounds.center, area.bounds.size);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(target, 0.08f);
    }
#endif

    public void TakeDamage(float damage)
    {
        Debug.Log("Enemy took " + damage + " damage!");
    }

    public void TakeDamage(int damage)
    {
        TakeDamage((float)damage);
    }
}