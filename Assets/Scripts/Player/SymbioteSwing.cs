using UnityEngine;

public class SymbioteSwing : MonoBehaviour
{
    public GameObject Melee;
    public Transform Aim; // assign in inspector

    bool isSwinging = false;
    float swingDuration = 0.5f;
    float swingTimer = 0f;

    private Vector2 lastDirection = Vector2.right; // default facing right

    public float attackOffsetDistance = 0.5f; // tweak in inspector

    void Start()
    {
        Melee.SetActive(false);
        Aim.localPosition = Vector3.zero; // ensure clean default
    }

    void Update()
    {
        CheckMeleeTimer();

        if (GameManager.Instance.playerData.hasAlien)
        {
            // Update lastDirection whenever moving
            Vector2 moveInput = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            ).normalized;

            if (moveInput != Vector2.zero)
            {
                lastDirection = moveInput;
            }

            // Attack input
            if (Input.GetKeyDown(KeyCode.O) || Input.GetKeyDown(KeyCode.Z))
            {
                OnAttack();

                Vector2 attackDirection = lastDirection;

                // Rotate only
                float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
                Aim.rotation = Quaternion.Euler(0f, 0f, angle + 90f);

                // Default = no offset
                Vector3 offset = Vector3.zero;

                // ONLY apply offset when attacking downward
                if (attackDirection.y < -0.5f)
                {
                    // push downward
                    offset += Vector3.down * attackOffsetDistance;

                    // slight horizontal influence for diagonals
                    offset += new Vector3(
                        attackDirection.x * attackOffsetDistance * 0.5f,
                        0f,
                        0f
                    );
                }

                Aim.localPosition = offset;
            }
        }
    }

    void OnAttack()
    {
        if (!isSwinging)
        {
            isSwinging = true;
            swingTimer = swingDuration;
            Melee.SetActive(true);
        }
    }

    void CheckMeleeTimer()
    {
        if (isSwinging)
        {
            swingTimer -= Time.deltaTime;

            if (swingTimer <= 0f)
            {
                isSwinging = false;
                Melee.SetActive(false);

                // Reset position after attack
                Aim.localPosition = Vector3.zero;
            }
        }
    }
}