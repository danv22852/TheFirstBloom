using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [Header("Wander Area")]
    public BoxCollider2D area;                 // Drag PatrolArea (BoxCollider2D, IsTrigger) here
    public float arriveDistance = 0.25f;

    [Header("Movement")]
    public float speed = 2.0f;
    public float waitAtPointSeconds = 0.2f;

    [Header("Random Timing")]
    public float minChooseTime = 1.0f;
    public float maxChooseTime = 2.5f;

    [Header("Wall Avoidance")]
    public LayerMask wallMask;                 // Set this to your Walls layer
    public float probeRadius = 0.15f;          // roughly your enemy "radius"
    public float probeDistance = 0.25f;        // how far ahead to check

    [Header("Approach Player")]
    public string playerTag = "Player";
    public float noticeRadius = 4.0f;          // start approaching when player is within this radius
    public float stopDistance = 1.2f;          // stop this far from player
    public float forgetRadius = 6.0f;          // stop approaching if player goes beyond this radius

    private Rigidbody2D rb;
    private Transform player;

    private Vector2 target;
    private float chooseTimer = 0f;
    private float waitTimer = 0f;
    private bool approaching = false;

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

        // Optional pause
        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Decide if we should approach the player
        if (player != null)
        {
            float dToPlayer = Vector2.Distance(rb.position, (Vector2)player.position);

            if (!approaching && dToPlayer <= noticeRadius)
                approaching = true;

            if (approaching && dToPlayer >= forgetRadius)
                approaching = false;

            if (approaching)
            {
                ApproachPlayer();
                return;
            }
        }

        Wander();
    }

    void ApproachPlayer()
    {
        Vector2 pos = rb.position;
        Vector2 toPlayer = (Vector2)player.position - pos;

        // Close enough -> stop
        if (toPlayer.magnitude <= stopDistance)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dir = toPlayer.normalized;

        // If we'd run into a wall, stop approaching and wander instead
        if (Physics2D.OverlapCircle(pos + dir * probeDistance, probeRadius, wallMask))
        {
            approaching = false;
            PickNewTarget();
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = dir * speed;
    }

    void Wander()
    {
        chooseTimer -= Time.fixedDeltaTime;

        Vector2 pos = rb.position;
        Vector2 toTarget = target - pos;

        // Pick a new target if arrived or timer expired
        if (toTarget.magnitude <= arriveDistance || chooseTimer <= 0f)
        {
            PickNewTarget();
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dir = toTarget.normalized;

        // If we'd run into a wall, pick another target
        if (Physics2D.OverlapCircle(pos + dir * probeDistance, probeRadius, wallMask))
        {
            PickNewTarget();
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = dir * speed;
    }

    void PickNewTarget()
    {
        // Try multiple random points so we don't pick inside walls
        for (int i = 0; i < 25; i++)
        {
            Vector2 candidate = RandomPointInArea();

            // Candidate must not be inside a wall
            if (!Physics2D.OverlapCircle(candidate, probeRadius, wallMask))
            {
                target = candidate;
                chooseTimer = Random.Range(minChooseTime, maxChooseTime);

                if (waitAtPointSeconds > 0f)
                    waitTimer = waitAtPointSeconds;

                return;
            }
        }

        // Fallback: stay put briefly
        target = rb.position;
        chooseTimer = 0.5f;
        waitTimer = 0.1f;
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
        if (area != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(area.bounds.center, area.bounds.size);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(target, 0.08f);

        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, noticeRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, stopDistance);
        }
    }
#endif

    public void TakeDamage(float damage)
    {
        Debug.Log("Enemy took " + damage + " damage!");
    }
}