using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [Header("Path")]
    public Transform[] points;
    public bool loop = true;          // true = 0->1->2->0..., false = ping-pong
    public float arriveDistance = 0.2f;

    [Header("Movement")]
    public float speed = 2.0f;
    public float waitAtPointSeconds = 0.0f;

    private Rigidbody2D rb;
    private int index = 0;
    private int dir = 1;              // used for ping-pong
    private float waitTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        if (points == null || points.Length == 0)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 target = points[index].position;
        Vector2 pos = rb.position;
        Vector2 toTarget = target - pos;

        if (toTarget.magnitude <= arriveDistance)
        {
            if (waitAtPointSeconds > 0f) waitTimer = waitAtPointSeconds;

            // Advance to next point
            if (loop)
            {
                index = (index + 1) % points.Length;
            }
            else
            {
                if (index == points.Length - 1) dir = -1;
                else if (index == 0) dir = 1;
                index += dir;
            }

            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = toTarget.normalized * speed;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (points == null || points.Length < 2) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < points.Length - 1; i++)
            if (points[i] && points[i + 1]) Gizmos.DrawLine(points[i].position, points[i + 1].position);

        if (loop && points[0] && points[^1])
            Gizmos.DrawLine(points[^1].position, points[0].position);
    }
#endif
    public void TakeDamage(float damage)
    {
        Debug.Log("Enemy took " + damage + " damage!");
    }
}