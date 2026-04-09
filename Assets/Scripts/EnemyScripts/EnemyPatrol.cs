using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Movement")]
    public float wanderSpeed = 2.0f;
    public float chaseSpeed = 2.8f;
    public float stopDistance = 0.9f;

    [Header("Aggro")]
    public string playerTag = "Player";
    public float aggroRadius = 4.0f;      // start chasing
    public float deaggroRadius = 6.0f;    // stop chasing (hysteresis prevents flicker)

    [Header("AUTO-ENGAGE (Catch Distance)")]
    public float catchDistance = 0.85f;   // when within this distance, start combat immediately

    [Header("Wander Timing")]
    public float minChangeTime = 0.6f;
    public float maxChangeTime = 1.6f;

    [Header("Collision Avoidance")]
    public LayerMask obstacleMask;        // Walls + Obstacles layers
    public float probeRadius = 0.15f;
    public float probeDistance = 0.35f;

    [Header("Optional Patrol Area Clamp")]
    public BoxCollider2D patrolArea;      // assign if you want it to NEVER leave an area
    public float areaPadding = 0.2f;

    private Rigidbody2D rb;
    private Transform player;

    private Vector2 moveDir;
    private float timer;

    private EnemyEncounter encounter;     // <-- NEW: reference to encounter script

    private enum State { Wander, Chase }
    private State state = State.Wander;

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

        // <-- NEW: find EnemyEncounter on self/parent/child
        encounter = GetComponent<EnemyEncounter>();
        if (encounter == null) encounter = GetComponentInParent<EnemyEncounter>();
        if (encounter == null) encounter = GetComponentInChildren<EnemyEncounter>();

        // Keep catchDistance sane relative to stopDistance (optional safety)
        if (catchDistance > stopDistance) catchDistance = stopDistance;

        PickNewDirection();
    }

    void FixedUpdate()
    {
        // Update aggro state
        if (player != null)
        {
            float d = Vector2.Distance(rb.position, (Vector2)player.position);

            if (state == State.Wander && d <= aggroRadius)
                state = State.Chase;

            if (state == State.Chase && d >= deaggroRadius)
            {
                state = State.Wander;
                PickNewDirection();
            }
        }
        else
        {
            state = State.Wander;
        }

        if (state == State.Chase && player != null)
            ChaseStep();
        else
            WanderStep();

        // Keep inside optional patrol area
        if (patrolArea != null)
            ClampToArea();
    }

    void WanderStep()
    {
        timer -= Time.fixedDeltaTime;
        if (timer <= 0f)
            PickNewDirection();

        if (WillHitObstacle(moveDir))
            PickNewDirection();

        rb.linearVelocity = moveDir * wanderSpeed;
        FlipSprite(moveDir.x);
    }

    void ChaseStep()
    {
        Vector2 pos = rb.position;
        Vector2 toPlayer = (Vector2)player.position - pos;

        // <-- NEW: if "caught", engage combat immediately
        if (toPlayer.magnitude <= catchDistance)
        {
            rb.linearVelocity = Vector2.zero;

            if (encounter != null)
            {
                encounter.Engage(player);   // requires Engage(Transform) in EnemyEncounter
            }
            else
            {
                Debug.LogWarning("EnemyEncounter not found on this enemy. Add EnemyEncounter or place it on a parent/child.");
            }
            return;
        }

        // still keep your stopDistance behavior (optional)
        if (toPlayer.magnitude <= stopDistance)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 desired = toPlayer.normalized;

        // If direct path blocked, try to "steer" left/right around obstacle
        Vector2 chosen = desired;
        if (WillHitObstacle(desired))
        {
            Vector2 left = new Vector2(-desired.y, desired.x);
            Vector2 right = new Vector2(desired.y, -desired.x);

            bool leftClear = !WillHitObstacle(left);
            bool rightClear = !WillHitObstacle(right);

            if (leftClear && rightClear)
                chosen = (Random.value < 0.5f) ? left : right;
            else if (leftClear)
                chosen = left;
            else if (rightClear)
                chosen = right;
            else
            {
                PickNewDirection();
                rb.linearVelocity = Vector2.zero;
                return;
            }
        }

        rb.linearVelocity = chosen * chaseSpeed;
        FlipSprite(chosen.x);
    }

    bool WillHitObstacle(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.001f) return false;
        Vector2 origin = rb.position + dir.normalized * 0.05f;
        RaycastHit2D hit = Physics2D.CircleCast(origin, probeRadius, dir, probeDistance, obstacleMask);
        return hit.collider != null;
    }

    void PickNewDirection()
    {
        Vector2 d;
        do { d = Random.insideUnitCircle; }
        while (d.sqrMagnitude < 0.05f);

        moveDir = d.normalized;
        timer = Random.Range(minChangeTime, maxChangeTime);
    }

    void FlipSprite(float xDir)
    {
        if (Mathf.Abs(xDir) < 0.01f) return;
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (xDir < 0 ? -1 : 1);
        transform.localScale = s;
    }

    void ClampToArea()
    {
        Bounds b = patrolArea.bounds;

        float minX = b.min.x + areaPadding;
        float maxX = b.max.x - areaPadding;
        float minY = b.min.y + areaPadding;
        float maxY = b.max.y - areaPadding;

        Vector2 p = rb.position;
        Vector2 clamped = new Vector2(
            Mathf.Clamp(p.x, minX, maxX),
            Mathf.Clamp(p.y, minY, maxY)
        );

        if (clamped != p)
        {
            rb.position = clamped;
            rb.linearVelocity = Vector2.zero;
            if (state == State.Wander) PickNewDirection();
        }
    }
}