using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine; // Needed for the Confiner

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float BASE_SPEED = 5f;
    // Added this so you can set the color in the Inspector once
    [SerializeField] private Color alienTint = Color.black; 

    public bool canMove = true;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer; // Added reference
    private float currentSpeed;

    private Vector2 movementInput;
    public Transform Aim;

    private bool isRunning = false;

    private bool alreadyTinted = false; // To prevent multiple tints

    void Start()
{
    rb = GetComponent<Rigidbody2D>();
    animator = GetComponent<Animator>();
    spriteRenderer = GetComponent<SpriteRenderer>();
    currentSpeed = BASE_SPEED;

    UpdateAppearance();

    if (GameManager.isReturningFromCombat)
    {
        transform.position = GameManager.lastPlayerPosition;
        RestoreCameraBoundary();
        GameManager.isReturningFromCombat = false;
    }

    ForceResetMovement();
}

    // Logic to check if the player should look like an alien
    public void UpdateAppearance()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerData.hasAlien && !alreadyTinted)
        {
            if (spriteRenderer != null)
            {
                alreadyTinted = true; // Set this to true to prevent future tints
                spriteRenderer.color = alienTint;
            }
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

    void FixedUpdate()
{
    if (!canMove)
    {
        rb.linearVelocity = Vector2.zero;
        return;
    }

    rb.linearVelocity = movementInput * currentSpeed;
}

    void Update()
    {
        if (Time.timeScale == 0 && canMove)
    {
        Debug.LogWarning("Time.timeScale was 0! Forcing it to 1.");
        Time.timeScale = 1f;
    }
         
       
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        movementInput = new Vector2(horizontal, vertical).normalized;

        if(movementInput != Vector2.zero && canMove)
        {
              if (horizontal > 0)
            spriteRenderer.flipX= false;
        else if (horizontal < 0)
            spriteRenderer.flipX = true;
        isRunning = movementInput.magnitude > 0;
        animator.SetBool("isRunning", isRunning);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }
      

         
    }


public void ForceResetMovement()
{
    if (rb == null)
        rb = GetComponent<Rigidbody2D>();

    canMove = true;
    currentSpeed = BASE_SPEED;

    rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    rb.linearVelocity = Vector2.zero;

    Debug.Log("Player movement HARD reset");
}
public void EnableMovement()
{
    if (rb == null)
        rb = GetComponent<Rigidbody2D>();

    canMove = true;
    currentSpeed = BASE_SPEED;

    rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    rb.linearVelocity = Vector2.zero;
}
}