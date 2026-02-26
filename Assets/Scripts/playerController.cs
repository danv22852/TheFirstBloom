using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float BASE_SPEED = 5f;
    private Rigidbody2D rb;
    private float currentSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = BASE_SPEED;
    }

    // Optional speed boost coroutine (kept from your original script)
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

        // Disable diagonal movement
        if (horizontal != 0)
        {
            vertical = 0;
        }

        Vector2 movement = new Vector2(horizontal, vertical).normalized;
        rb.linearVelocity = movement * currentSpeed;

        // Flip sprite left/right
        if (horizontal < 0)
        {
            transform.rotation = new Quaternion(0, -1, 0, 0);
        }
        else if (horizontal > 0)
        {
            transform.rotation = new Quaternion(0, 0, 0, 0);
        }
    }
}