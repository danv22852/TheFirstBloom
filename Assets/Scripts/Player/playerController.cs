using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine; 

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float BASE_SPEED = 5f;
    [SerializeField] private Color alienTint = Color.black;
    

    private bool _canMove = true;
    public bool canMove
    {
        get => _canMove;
        set
        {
            if (value == false)
                Debug.Log("canMove set to FALSE\n" + StackTraceUtility.ExtractStackTrace());
            _canMove = value;
        }
    }

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer; 
    private float currentSpeed;

    private Vector2 movementInput;
    public Transform Aim;

    private bool isRunning = false;
    private bool alreadyTinted = false; 

    [Header("Bloom Decay (Step-Based)")]
    public bool enableBloomDecay = true;
    [Tooltip("How many units/tiles the player must walk to lose 1 Bloom when under 75")]
    public float fastDecayDistance = 15f;
    [Tooltip("How many units/tiles the player must walk to lose 1 Bloom when >= 75")]
    public float slowDecayDistance = 40f;

    private float distanceTraveled = 0f;
    private Vector3 lastPosition;

    [Header("Symbiote Twitch")]
    public float twitchForce = 12.0f; 
    public float twitchDuration = 0.15f; 

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

        // Cleaned up merge artifact here!
        if (GameManager.isReturningFromCombat)
        {
            lastPosition = transform.position;
            
            // 1. Teleport Player
            transform.position = GameManager.lastPlayerPosition;

            // 2. Restore Camera Boundary
            RestoreCameraBoundary();

            GameManager.isReturningFromCombat = false;
        }

        ForceResetMovement();
    }

    public void UpdateAppearance()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerData.hasAlien && !alreadyTinted)
        {
            if (spriteRenderer != null)
            {
                alreadyTinted = true; 
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

    void Update()
    {
        // TEMP for debug
        if (!canMove)
            Debug.Log("canMove is false. TimeScale: " + Time.timeScale + " | Stack: " + StackTraceUtility.ExtractStackTrace());

        if (Time.timeScale == 0 && canMove)
        {
            Time.timeScale = 1f;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // THE MISSING LINK: Actually assign the input!
        movementInput = new Vector2(horizontal, vertical).normalized;

        if (movementInput != Vector2.zero && canMove)
        {
            if (horizontal > 0)
                spriteRenderer.flipX = false;
            else if (horizontal < 0)
                spriteRenderer.flipX = true;
                
            isRunning = true;
            animator.SetBool("isRunning", isRunning);
        }
        else
        {
            isRunning = false;
            animator.SetBool("isRunning", false);
        }

        if (enableBloomDecay && GameManager.Instance != null && GameManager.Instance.playerData != null)
        {
            var pd = GameManager.Instance.playerData;

            if (pd.currentBloom > pd.decayFloor)
            {
                float distanceThisFrame = Vector3.Distance(lastPosition, transform.position);
                distanceTraveled += distanceThisFrame;

                float currentTargetDistance = (pd.currentBloom >= 75) ? slowDecayDistance : fastDecayDistance;

                if (distanceTraveled >= currentTargetDistance)
                {
                    pd.currentBloom--;
                    if (pd.currentBloom < pd.decayFloor) pd.currentBloom = pd.decayFloor;
                    distanceTraveled = 0f;
                }
            }
        }

        lastPosition = transform.position;
    }

    public void ForceResetMovement()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

            // if(!GameManager.Instance.playerData.hasAlien)
            //     return;

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

    void FixedUpdate()
    {
        if (isTwitching || isMovementLocked) return;

        if (canMove)
        {
            if (rb.constraints != RigidbodyConstraints2D.FreezeRotation)
            {
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }

            rb.linearVelocity = movementInput * currentSpeed;
        }
        else
        {
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

    public void SetMovementLock(bool isLocked)
    {
        isMovementLocked = isLocked;
        if (isLocked && rb != null)
        {
            rb.linearVelocity = Vector2.zero; 
        }
    }

    private System.Collections.IEnumerator TwitchRoutine(Vector2 direction)
    {
        isTwitching = true;
        rb.linearVelocity = direction * twitchForce;
        yield return new WaitForSeconds(twitchDuration);
        rb.linearVelocity = Vector2.zero;
        isTwitching = false;
    }
}