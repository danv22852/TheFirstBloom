using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float BASE_SPEED = 5f;

    private Rigidbody2D rb;
    private Animator animator;
    private float currentSpeed;

    private Vector2 movementInput;
    public Transform Aim;

    void Start()
    {
        // When the scene loads, check if we just finished a battle
        if (GameManager.isReturningFromCombat == true)
        {
            // Teleport the player to the saved coordinates!
            transform.position = GameManager.lastPlayerPosition;

            // Turn the switch back off so we don't accidentally teleport again later
            GameManager.isReturningFromCombat = false;

            Debug.Log("TELEPORTED PLAYER TO: " + transform.position);
        }
        
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentSpeed = BASE_SPEED;
    }

    public IEnumerator SpeedChange(float newSpeed, float timeInSecs)
    {
        currentSpeed = newSpeed;
        yield return new WaitForSeconds(timeInSecs);
        currentSpeed = BASE_SPEED;
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Calculate movement FIRST
        movementInput = new Vector2(horizontal, vertical).normalized;

        // Flip sprite
        if (horizontal > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (horizontal < 0)
            transform.localScale = new Vector3(-1, 1, 1);

        // Set Animator parameter
        bool isRunning = movementInput.magnitude > 0;
        animator.SetBool("isRunning", isRunning);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movementInput * currentSpeed;
    }
}