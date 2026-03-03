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
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = BASE_SPEED;
    }

    // Optional speed boost coroutine
    public IEnumerator SpeedChange(float newSpeed, float timeInSecs)
    {
        currentSpeed = newSpeed;
        yield return new WaitForSeconds(timeInSecs);
        currentSpeed = BASE_SPEED;
    }

    void Update()
    {
        // Get raw input (allows multiple keys at once)
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

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