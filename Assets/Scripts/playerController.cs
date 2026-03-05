using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine; // Needed for the Confiner

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
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentSpeed = BASE_SPEED;

        if (GameManager.isReturningFromCombat)
        {
            // 1. Teleport Player
            transform.position = GameManager.lastPlayerPosition;

            // 2. Restore Camera Boundary
            RestoreCameraBoundary();

            GameManager.isReturningFromCombat = false;
        }
    }

    private void RestoreCameraBoundary()
    {
        if (!string.IsNullOrEmpty(GameManager.currentMapBoundaryName))
        {
            GameObject boundaryObj = GameObject.Find(GameManager.currentMapBoundaryName);
            if (boundaryObj != null)
            {
                PolygonCollider2D poly = boundaryObj.GetComponent<PolygonCollider2D>();
                CinemachineConfiner2D confiner = FindFirstObjectByType<CinemachineConfiner2D>();
                
                if (confiner != null && poly != null)
                {
                    confiner.BoundingShape2D = poly;
                    confiner.InvalidateBoundingShapeCache();
                }
            }
        }
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

        movementInput = new Vector2(horizontal, vertical).normalized;

        if (horizontal > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (horizontal < 0)
            transform.localScale = new Vector3(-1, 1, 1);

        bool isRunning = movementInput.magnitude > 0;
        animator.SetBool("isRunning", isRunning);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movementInput * currentSpeed;
    }
}