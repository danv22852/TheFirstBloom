using UnityEngine;
using System.Collections;

public class OverworldBloomManager : MonoBehaviour
{
    [Header("Dependencies")]
    public PlayerController  playerMovement; 
    public AudioSource hallucinationAudio; 
    public AudioSource lashAudio; 
    public GameObject symbioteArmVisual; 

    [Header("Vision Restriction")]
    public GameObject visionMask; 
    public Vector3 highBloomVisionScale = new Vector3(15f, 15f, 1f); 
    public Vector3 totalBloomVisionScale = new Vector3(8f, 8f, 1f); 

    [Header("Lash Settings")]
    public float medLashRadius = 3.0f;  // Normal reach for Medium Bloom
    public float highLashRadius = 6.0f; // Massive reach for High/Total Bloom
    public float autoEngageRadius = 4.0f; 
    public LayerMask enemyLayer;

    private BloomState lastKnownState = BloomState.Stable;
    
    // Coroutine Trackers
    private Coroutine twitchRoutine;
    private Coroutine lashRoutine;

    private Vector3 initialArmScale;
    private Vector3 initialArmPosition;
    private bool isDraggingToEnemy = false;

    private void Start()
    {
        if (symbioteArmVisual != null) 
        {
            // --- NEW: Memorize the starting size and distance before turning it off! ---
            initialArmScale = symbioteArmVisual.transform.localScale;
            initialArmPosition = symbioteArmVisual.transform.localPosition;
            
            symbioteArmVisual.SetActive(false);
        }
        
        if (visionMask != null) visionMask.SetActive(false);
        UpdateOverworldEffects();
    }

    private void Update()
    {
        // --- QUICK DEBUG TEST (Press T for Total Bloom, Y for Stable) ---
        if (Input.GetKeyDown(KeyCode.T))
        {
            lastKnownState = BloomState.Total;
            UpdateOverworldEffects();
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            lastKnownState = BloomState.Stable;
            UpdateOverworldEffects();
        }
        // ----------------------------------------------------------------

        if (GameManager.Instance == null || GameManager.Instance.playerData == null) return;

        BloomState currentState = GameManager.Instance.playerData.currentBloomState;
        
        if (currentState != lastKnownState)
        {
            lastKnownState = currentState;
            UpdateOverworldEffects();
        }

        // --- 100% BLOOM: Auto-Engage ---
        if (lastKnownState == BloomState.Total)
        {
            CheckAutoEngage();
        }
    }

    private void UpdateOverworldEffects()
    {
        // 1. Reset everything to baseline
        StopAllCoroutines();
        if (hallucinationAudio != null) hallucinationAudio.Stop();
        if (visionMask != null) visionMask.SetActive(false); 

        // 2. Apply effects based on current state
        switch (lastKnownState)
        {
            case BloomState.Stable:
                // Safe!
                break;

            case BloomState.Low:
                // Twitches every 10 to 15 seconds
                twitchRoutine = StartCoroutine(TwitchRoutine(10f, 15f));
                break;

            case BloomState.Medium:
                // NEW: Twitches every 5 to 9 seconds
                twitchRoutine = StartCoroutine(TwitchRoutine(5f, 9f));
                lashRoutine = StartCoroutine(LashOutRoutine(15f, 25f)); 

                if (visionMask != null) 
                {
                    visionMask.SetActive(true);
                    visionMask.transform.localScale = highBloomVisionScale;
                }
                break;

            case BloomState.High:
                // NEW: Violent twitches every 2 to 5 seconds!
                twitchRoutine = StartCoroutine(TwitchRoutine(2f, 5f));
                // NEW: Lashes out every 4 to 8 seconds!
                lashRoutine = StartCoroutine(LashOutRoutine(4f, 8f)); 
                
                if (hallucinationAudio != null && !hallucinationAudio.isPlaying) hallucinationAudio.Play();
                if (visionMask != null) 
                {
                    visionMask.SetActive(true);
                    visionMask.transform.localScale = totalBloomVisionScale;
                }
                break;

            case BloomState.Total:
                // Same aggressive timers as High Bloom
                twitchRoutine = StartCoroutine(TwitchRoutine(2f, 5f));
                lashRoutine = StartCoroutine(LashOutRoutine(4f, 8f)); 
                
                if (hallucinationAudio != null && !hallucinationAudio.isPlaying) hallucinationAudio.Play();

                if (visionMask != null) 
                {
                    visionMask.SetActive(true);
                    visionMask.transform.localScale = totalBloomVisionScale;
                }
                
                break;
        }
    }

    private IEnumerator TwitchRoutine(float minTime, float maxTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minTime, maxTime));
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            if (playerMovement != null) playerMovement.ApplySymbioteTwitch(randomDir);
        }
    }

    private IEnumerator LashOutRoutine(float minTime, float maxTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minTime, maxTime));

            // --- NEW: 1. Determine the radius FIRST ---
            float currentLashRadius = (lastKnownState >= BloomState.High) ? highLashRadius : medLashRadius;

            // --- NEW: 2. Scale the visual to match the new radius! ---
            if (symbioteArmVisual != null)
            {
                // Calculate the ratio (e.g., if radius is 6 and base is 3, the factor is 2.0)
                float scaleFactor = currentLashRadius / medLashRadius;

                // Scale the triangle up, AND push it further away from the player
                symbioteArmVisual.transform.localScale = initialArmScale * scaleFactor;
                symbioteArmVisual.transform.localPosition = initialArmPosition * scaleFactor;
            }

            // 3. WARNING PHASE (Only happens at Medium Bloom)
            if (lastKnownState < BloomState.High)
            {
                if (lashAudio != null) lashAudio.Play();
                
                float warningTimer = 2.0f; 
                float flickerTimer = 0f;
                bool isVisible = false;

                while (warningTimer > 0)
                {
                    warningTimer -= Time.deltaTime;
                    flickerTimer += Time.deltaTime;
                    
                    if (flickerTimer > 0.2f) 
                    {
                        isVisible = !isVisible;
                        if (symbioteArmVisual != null) symbioteArmVisual.SetActive(isVisible);
                        flickerTimer = 0f;
                    }
                    yield return null;
                }
            }
            else
            {
                // HIGH BLOOM: No warning, just sound!
                if (lashAudio != null) lashAudio.Play();
            }

            // 4. SWING PHASE
            if (symbioteArmVisual != null) symbioteArmVisual.SetActive(true);

            float swingDuration = 1.0f; 
            float timeElapsed = 0f;
            float spinSpeed = 360f; 

            while (timeElapsed < swingDuration)
            {
                if (symbioteArmVisual != null)
                {
                    symbioteArmVisual.transform.RotateAround(transform.position, Vector3.forward, spinSpeed * Time.deltaTime);
                }
                timeElapsed += Time.deltaTime;
                yield return null; 
            }
            
            if (symbioteArmVisual != null) symbioteArmVisual.SetActive(false);

            // 5. STRIKE PHASE 
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, currentLashRadius, enemyLayer);
            if (hitEnemies.Length > 0)
            {
                EnemyEncounter enemy = hitEnemies[0].GetComponent<EnemyEncounter>();
                if (enemy != null)
                {
                    Debug.Log("Symbiote lashed out and dragged an enemy into combat!");
                    enemy.Engage(transform);
                }
            }
        }
    }

    private void CheckAutoEngage()
    {
        // If we are already dragging, don't scan for more enemies!
        if (isDraggingToEnemy) return;

        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, autoEngageRadius, enemyLayer);
        foreach (Collider2D col in nearbyEnemies)
        {
            EnemyEncounter enemy = col.GetComponent<EnemyEncounter>();
            if (enemy != null)
            {
                Debug.Log("100% Bloom: Target locked! Dragging player to enemy!");
                StartCoroutine(DragToEnemyRoutine(enemy));
                break; 
            }
        }
    }

    private IEnumerator DragToEnemyRoutine(EnemyEncounter enemy)
    {
        isDraggingToEnemy = true;

        // 1. Freeze the player's controls
        if (playerMovement != null) playerMovement.SetMovementLock(true);

        // Optional: Play a terrifying roar or splash sound here!
        // if (lashAudio != null) lashAudio.Play();

        float dragSpeed = 10f; // How fast they get yanked across the room

        // 2. Violently pull the player toward the enemy!
        // We stop when the distance between them is less than 0.5 units
        while (enemy != null && Vector3.Distance(transform.position, enemy.transform.position) > 0.5f)
        {
            transform.position = Vector3.MoveTowards(transform.position, enemy.transform.position, dragSpeed * Time.deltaTime);
            yield return null;
        }

        // 3. CRASH! Now we start the combat sequence.
        if (enemy != null)
        {
            enemy.Engage(transform);
        }

        // 4. Unlock movement (so they can walk again after the battle ends)
        if (playerMovement != null) playerMovement.SetMovementLock(false);
        isDraggingToEnemy = false;
    }

    // --- NEW: Updated Gizmos so you can see all 3 radiuses in the Editor! ---
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, medLashRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, highLashRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, autoEngageRadius);
    }
}