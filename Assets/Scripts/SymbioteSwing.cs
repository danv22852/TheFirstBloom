using UnityEngine;

public class SymbioteSwing : MonoBehaviour
{
    public GameObject Melee;
    public Transform Aim; // assign in inspector

    bool isSwinging = false;
    float swingDuration = 0.5f;
    float swingTimer = 0f;

    private Vector2 lastDirection = Vector2.right; // default facing right

    void Start()
    {
        Melee.SetActive(false);
    }

    void Update()
    {
        CheckMeleeTimer();

        if (GameManager.Instance.hasAlien)
        {
            // Update lastDirection whenever moving
            Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
            if (moveInput != Vector2.zero)
            {
                lastDirection = moveInput;
                
                Debug.Log("Last direction updated: " + lastDirection);
            }

            // Attack input
            if (Input.GetKeyDown(KeyCode.O))
            {
                OnAttack();
                Debug.Log("O key pressed!");
                Debug.Log("Current last direction: " + lastDirection);

                // Use lastDirection if stationary
                Vector2 attackDirection = lastDirection;
                float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
                Aim.rotation = Quaternion.Euler(0f, 0f, angle + 90f);
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
            }
        }
    }
}