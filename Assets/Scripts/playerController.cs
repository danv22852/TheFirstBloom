using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayerController : MonoBehaviour
{
    [SerializeField] private float BASE_SPEED = 5f;

    private Rigidbody2D rb;
    private float currentSpeed;

    private Vector2 movementInput;
    public Transform Aim;

    // Optional speed boost coroutine
    public IEnumerator SpeedChange(float newSpeed, float timeInSecs)
    {
        currentSpeed = newSpeed;
        yield return new WaitForSeconds(timeInSecs);
        currentSpeed = BASE_SPEED;
    }

    private void Start()
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
        currentSpeed = BASE_SPEED;
    }
    void Update()
    {
        // Get raw input (allows multiple keys at once)
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (horizontal > 0)
        transform.localScale = new Vector3(1, 1, 1); // Facing right
    else if (horizontal < 0)
        transform.localScale = new Vector3(-1, 1, 1); // Facing left

        // Allow diagonal movement
        movementInput = new Vector2(horizontal, vertical).normalized;

        Vector2 direction = movementInput;

     
    }

    void FixedUpdate()
    {
        // Apply movement using physics
        rb.linearVelocity = movementInput * currentSpeed;

    }
}