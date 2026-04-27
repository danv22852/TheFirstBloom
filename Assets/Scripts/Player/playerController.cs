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

    [Header("Bloom Decay (Step-Based)")]
    public bool enableBloomDecay = true;

    [Tooltip("How many units/tiles the player must walk to lose 1 Bloom when under 75")]
    public float fastDecayDistance = 15f;

    [Tooltip("How many units/tiles the player must walk to lose 1 Bloom when >= 75")]
    public float slowDecayDistance = 40f;

    private float distanceTraveled = 0f;
    private Vector3 lastPosition;

    [Header("Symbiote Twitch")]
    public float twitchForce = 12.0f; // Increased so it hits much harder!
    public float twitchDuration = 0.15f; // How long they lose control (a quick, violent shove)
    
    // This flag tells the rest of the script if the Symbiote currently has control
    private bool isTwitching = false;
    private bool isMovementLocked = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void Start()
{
    rb = GetComponent<Rigidbody2D>();
    animator = GetComponent<Animator>();
    spriteRenderer = GetComponent<SpriteRenderer>();
    currentSpeed = BASE_SPEED;

    UpdateAppearance();

    if (GameManager.isReturningFromCombat)
    {
<<<<<<< Updated upstream
        transform.position = GameManager.lastPlayerPosition;
        RestoreCameraBoundary();
        GameManager.isReturningFromCombat = false;
=======
        lastPosition = transform.position;

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // Initialize reference
        currentSpeed = BASE_SPEED;

        // --- Persistent State Checks ---
        UpdateAppearance();

        if (GameManager.isReturningFromCombat)
        {
            // 1. Teleport Player
            transform.position = GameManager.lastPlayerPosition;

            // 2. Restore Camera Boundary
            RestoreCameraBoundary();

            GameManager.isReturningFromCombat = false;
        }
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
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
=======
        if (Time.timeScale == 0)
        {
            Debug.LogWarning("Time.timeScale was 0! Forcing it to 1.");
            Time.timeScale = 1f;
        }
        Debug.Log("Player canMove: " + canMove); // Debug statement to check movement state

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
>>>>>>> Stashed changes

    float horizontal = Input.GetAxisRaw("Horizontal");
    float vertical = Input.GetAxisRaw("Vertical");

<<<<<<< Updated upstream
    movementInput = new Vector2(horizontal, vertical).normalized;

    // Flip sprite left/right
    if (horizontal > 0)
        spriteRenderer.flipX = false;
    else if (horizontal < 0)
        spriteRenderer.flipX = true;

    // Better animation detection
    isRunning = horizontal != 0 || vertical != 0;
    animator.SetBool("isRunning", isRunning);
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
=======
        if (movementInput != Vector2.zero && canMove)
        {
            if (horizontal > 0)
                spriteRenderer.flipX = false;
            else if (horizontal < 0)
                spriteRenderer.flipX = true;
            isRunning = movementInput.magnitude > 0;
            animator.SetBool("isRunning", isRunning);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }

        if (enableBloomDecay && GameManager.Instance != null && GameManager.Instance.playerData != null)
        {
            var pd = GameManager.Instance.playerData;

            // 1. Only track steps if we actually have Bloom to lose!
            // 1. Change the check from 0 to decayFloor
            if (pd.currentBloom > pd.decayFloor)
            {
                float distanceThisFrame = Vector3.Distance(lastPosition, transform.position);
                distanceTraveled += distanceThisFrame;

                float currentTargetDistance = (pd.currentBloom >= 75) ? slowDecayDistance : fastDecayDistance;

                if (distanceTraveled >= currentTargetDistance)
                {
                    pd.currentBloom--;

                    // 2. Change the hard limit from 0 to decayFloor
                    if (pd.currentBloom < pd.decayFloor) pd.currentBloom = pd.decayFloor;

                    distanceTraveled = 0f;
                }
            }
        }

        // ALWAYS update the last position at the very end of the frame!
        lastPosition = transform.position;

    }

    void FixedUpdate()
    {
        if (isTwitching || isMovementLocked) return;
        
        if (canMove)
        {
            // 1. Ensure constraints are ONLY freezing rotation
            if (rb.constraints != RigidbodyConstraints2D.FreezeRotation)
            {
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }

            // 2. Apply movement
            rb.linearVelocity = movementInput * currentSpeed;
        }
        else
        {
            // 3. When NOT moving, we freeze everything to prevent sliding
            rb.linearVelocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    public void ApplySymbioteTwitch(Vector2 direction)
    {
        if (rb != null && !isTwitching)
        {
            Debug.Log("Violent Symbiote Twitch!");
            StartCoroutine(TwitchRoutine(direction));
        }
    }

    

    // The Bloom Manager will call this to freeze/unfreeze the player
    public void SetMovementLock(bool isLocked)
    {
        isMovementLocked = isLocked;
        if (isLocked && rb != null) 
        {
            rb.linearVelocity = Vector2.zero; // Stop them in their tracks
        }
    }

    private System.Collections.IEnumerator TwitchRoutine(Vector2 direction)
    {
        // 1. Lock the player out of normal movement
        isTwitching = true;

        // 2. Apply a massive burst of speed in the random direction
        rb.linearVelocity = direction * twitchForce;

        // 3. Wait for a split second while they slide
        yield return new WaitForSeconds(twitchDuration);

        // 4. Slam on the brakes and give control back to the player
        rb.linearVelocity = Vector2.zero;
        isTwitching = false;
    }
>>>>>>> Stashed changes
}