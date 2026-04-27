using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Movement")]
    public float wanderSpeed = 2.0f;
    public float chaseSpeed = 2.8f;
    public float stopDistance = 0.85f;

    [Header("Aggro")]
    public string playerTag = "Player";
    public float aggroRadius = 4.0f;
    public float deaggroRadius = 6.0f;

    [Header("AUTO-ENGAGE (Combat Trigger Distance)")]
    public float catchDistance = 0.9f;

    [Header("Wander Timing")]
    public float minChangeTime = 0.6f;
    public float maxChangeTime = 1.6f;

    [Header("Collision Avoidance")]
    public LayerMask obstacleMask;
    public float probeRadius = 0.15f;
    public float probeDistance = 0.35f;

    [Header("Optional Patrol Area")]
    public BoxCollider2D patrolArea;
    public float areaPadding = 0.2f;

    private Rigidbody2D rb;
    private Transform player;
    private EnemyEncounter encounter;

    private Vector2 moveDir;
    private float timer;

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
        player = GameObject.FindGameObjectWithTag(playerTag)?.transform;

        encounter = GetComponent<EnemyEncounter>();
        if (encounter == null)
            encounter = GetComponentInParent<EnemyEncounter>();

        PickNewDirection();
    }

    void FixedUpdate()
    {
        if (player == null)
        {
            state = State.Wander;
            WanderStep();
            return;
        }

        float distance = Vector2.Distance(rb.position, player.position);

        // STATE SWITCHING
        if (state == State.Wander && distance <= aggroRadius)
            state = State.Chase;

        if (state == State.Chase && distance >= deaggroRadius)
        {
            state = State.Wander;
            PickNewDirection();
        }

        // BEHAVIOR
        if (state == State.Chase)
            ChaseStep(distance);
        else
            WanderStep();

        if (patrolArea != null)
            ClampToArea();
    }

    void WanderStep()
    {
        timer -= Time.fixedDeltaTime;

        if (timer <= 0f || WillHitObstacle(moveDir))
            PickNewDirection();

        rb.linearVelocity = moveDir * wanderSpeed;
        FlipSprite(moveDir.x);
    }

    void ChaseStep(float distance)
    {
        Debug.Log("Distance: " + distance + " | Encounter: " + encounter);
        Vector2 toPlayer = (Vector2)player.position - rb.position;

        // 🟥 COMBAT TRIGGER (ONLY PLACE COMBAT STARTS)
        if (distance * distance <= catchDistance * catchDistance)
        {
            rb.linearVelocity = Vector2.zero;

            if (encounter != null)
            {
                encounter.Engage(player);
            }
            else
            {
                Debug.LogError("EnemyEncounter missing on this enemy!");
            }

            return;
        }

        // STOP IF TOO CLOSE
        if (distance <= stopDistance)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dir = toPlayer.normalized;

        if (WillHitObstacle(dir))
        {
            Vector2 left = new Vector2(-dir.y, dir.x);
            Vector2 right = new Vector2(dir.y, -dir.x);

            if (!WillHitObstacle(left)) dir = left;
            else if (!WillHitObstacle(right)) dir = right;
        }

        rb.linearVelocity = dir * chaseSpeed;
        FlipSprite(dir.x);
    }

    bool WillHitObstacle(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.001f) return false;

        Vector2 origin = rb.position + dir.normalized * 0.05f;

        RaycastHit2D hit = Physics2D.CircleCast(
            origin,
            probeRadius,
            dir,
            probeDistance,
            obstacleMask
        );

        return hit.collider != null;
    }

    void PickNewDirection()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 d = Random.insideUnitCircle.normalized;

            if (patrolArea != null)
            {
                Bounds b = patrolArea.bounds;
                Vector2 future = rb.position + d * 1.5f;

                if (future.x < b.min.x + areaPadding ||
                    future.x > b.max.x - areaPadding ||
                    future.y < b.min.y + areaPadding ||
                    future.y > b.max.y - areaPadding)
                {
                    continue;
                }
            }

            moveDir = d;
            timer = Random.Range(minChangeTime, maxChangeTime);
            return;
        }

        moveDir = Vector2.zero;
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

        Vector2 p = rb.position;

        Vector2 clamped = new Vector2(
            Mathf.Clamp(p.x, b.min.x + areaPadding, b.max.x - areaPadding),
            Mathf.Clamp(p.y, b.min.y + areaPadding, b.max.y - areaPadding)
        );

        if (clamped != p)
        {
            rb.position = clamped;
            rb.linearVelocity = Vector2.zero;

            if (state == State.Wander)
                PickNewDirection();
        }
    }
}