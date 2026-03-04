using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Random;
using TMPro;
using System.Collections;
using System;

public class CombatSystem : MonoBehaviour
{
    // --- PLAYER STATS ---
    [Header("Player Stats")]
    public int playerHealth = 100; // [cite: 2]
    public int playerStrength = 15; // [cite: 3]
    public int playerSpeed = 10; // [cite: 4]
    public int playerDefense = 5; // [cite: 5]

    // --- BLOOM SYSTEM ---
    [Header("Bloom System")]
    public int currentBloom = 0; // [cite: 11]
    public int lowBloomThreshold = 30; // Defined arbitrary threshold for testing

    // --- ENEMY STATS ---
    [Header("Enemy Stats")]
    public int enemyHealth = 50;
    public int enemyStrength = 8;
    public int enemySpeed = 8;

    // --- COMBAT STATE ---
    private bool isPlayerTurn = false;
    private bool hasUsedItemThisTurn = false;

    // --- UI ELEMENTS ---
    [Header("UI Elements")]
    public TextMeshProUGUI playerHP;
    public TextMeshProUGUI enemyHP;

    [Header("Animation Settings")]
    public Transform playerTransform;
    public Transform enemyTransform;
    public float moveSpeed = 15f;

    private void Start()
    {
        UpdateHealthUI();
        DetermineFirstTurn();
    }

    // The entity with the higher speed stat will have the first turn.
    private void DetermineFirstTurn()
    {
        if (playerSpeed >= enemySpeed)
        {
            Debug.Log("Player is faster! Player goes first.");
            PlayerStartTurn();
        }
        else
        {
            Debug.Log("Enemy is faster! Enemy goes first.");
            EnemyTurn();
        }
    }

    private void PlayerStartTurn()
    {
        isPlayerTurn = true;
        hasUsedItemThisTurn = false; // Reset item usage for the new turn
        Debug.Log("It is now the Player's turn. Choose an action.");

        // Check for 100% Bloom takeover logic here in the future [cite: 45]
    }

    // --- PLAYER ACTIONS ---

    public void OnAttackButton()
    {
        if (!isPlayerTurn) return;

        Debug.Log("Player uses Basic Attack!");

        // Damage from the attack scales with the player's strength stat[cite: 77].
        var damage = playerStrength;

        // Apply defense reduction (simple formula for testing)
        var actualDamage = Mathf.Max(1, damage);

        enemyHealth -= actualDamage;
        UpdateHealthUI();
        Debug.Log("Dealt " + actualDamage + " damage to the enemy.");

        EndPlayerTurn();
    }

    public void playerAttackButton()
    {
        Debug.Log("Player has chosen to ATTACK");

        // Start the animation
        StartCoroutine(PerformMeleeAttack(playerTransform, enemyTransform,
            onHit: () =>
            {
                // This happens exactly when the player touches the enemy
                var playerTrueDmg = UnityEngine.Random.Range(playerStrength - 1, playerStrength + 2);
                enemyHealth -= playerTrueDmg;
                UpdateHealthUI();
            },
            onComplete: () =>
            {
                // This happens when the player returns to their starting spot
                if (enemyHealth <= 0)
                {
                    Debug.Log("The player has won!");
                    SceneManager.LoadScene("FirstLevel");
                }
                else
                {
                    enemyTurn();
                }
            }));
    }

    public void enemyTurn()
    {
        Debug.Log("Enemy is Attacking.");

        // Start the animation for the enemy
        StartCoroutine(PerformMeleeAttack(enemyTransform, playerTransform,
            onHit: () =>
            {
                // This happens exactly when the enemy touches the player
                var enemyTrueDmg = UnityEngine.Random.Range(enemyStrength - 1, enemyStrength + 2);
                playerHealth -= enemyTrueDmg;
                UpdateHealthUI();
            },
            onComplete: () =>
            {
                // This happens when the enemy returns to their starting spot
                if (playerHealth <= 0)
                {
                    Debug.Log("The player has lost!");
                    SceneManager.LoadScene("MainMenu");
                }
            }));
    }

    public void OnSkillButton1(int bloomCost, int baseSkillDamage)
    {
        if (!isPlayerTurn) return;

        Debug.Log("Player uses a Symbiote Skill!");

        // Each skill has a bloom cost, which increases the player's bloom meter[cite: 80].
        currentBloom += bloomCost;
        Debug.Log("Bloom increased by " + bloomCost + ". Current Bloom: " + currentBloom);

        enemyHealth -= baseSkillDamage;
        UpdateHealthUI();

        EndPlayerTurn();
    }

    public void OnSkillButton()
    {
        if (!isPlayerTurn) return;

        Debug.Log("Player uses Symbiote Swipe!");

        // Setting the stats locally inside the function 
        var bloomCost = 1; // Symbiote Swipe costs 1 Bloom 
        var baseSkillDamage = 10; // Example damage

        // Each skill has a bloom cost, which increases the player's bloom meter. [cite: 80]
        currentBloom += bloomCost;
        Debug.Log("Bloom increased by " + bloomCost + ". Current Bloom: " + currentBloom);

        enemyHealth -= baseSkillDamage;
        UpdateHealthUI();

        // End the turn
        EndPlayerTurn();
    }

    public void OnItemButton()
    {
        if (!isPlayerTurn) return;

        // Only one item can be used per turn.
        if (hasUsedItemThisTurn)
        {
            Debug.Log("You have already used an item this turn!");
            return;
        }

        Debug.Log("Player uses an Item! Health restored/Bloom reduced.");
        UpdateHealthUI();
        hasUsedItemThisTurn = true;

        // Does not take a turn to use the item.
        // Notice we do NOT call EndPlayerTurn() here.
    }

    public void OnRunButton()
    {
        if (!isPlayerTurn) return;

        // The player loses the ability to run from combat at low bloom[cite: 36].
        // If the player is under the low bloom threshold, they have a chance to escape[cite: 84].
        if (currentBloom >= lowBloomThreshold)
        {
            Debug.Log("Your Bloom is too high! The symbiote won't let you run!");
            return;
        }

        var escapeChance = UnityEngine.Random.Range(0, 100);
        if (escapeChance > 50) // 50% chance to escape
        {
            Debug.Log("Escaped successfully!");
            SceneManager.LoadScene("firstFloor");
        }
        else
        {
            Debug.Log("Failed to escape!");
            EndPlayerTurn();
        }
    }

    private void EndPlayerTurn()
    {
        isPlayerTurn = false;

        if (enemyHealth <= 0)
        {
            Debug.Log("Enemy defeated!");
            SceneManager.LoadScene("firstFloor");
            return;
        }

        EnemyTurn();
    }

    // --- ENEMY LOGIC ---

    private void EnemyTurn()
    {
        Debug.Log("Enemy attacks!");

        var damage = enemyStrength;
        var actualDamage = Mathf.Max(1, damage - playerDefense);

        playerHealth -= actualDamage;
        UpdateHealthUI();
        Debug.Log("Player takes " + actualDamage + " damage.");

        if (playerHealth <= 0)
        {
            Debug.Log("Player died. Game Over.");
            // Handle Game Over
            return;
        }

        PlayerStartTurn();
    }

    private void UpdateHealthUI()
    {
        playerHP.text = "Player HP: " + playerHealth;
        enemyHP.text = "Enemy HP: " + enemyHealth;
    }

    private IEnumerator PerformMeleeAttack(Transform attacker, Transform target, Action onHit, Action onComplete)
    {
        var startPos = attacker.position;

        // Safely calculate a position that is exactly 60% of the distance to the target
        var targetPos = Vector3.Lerp(startPos, target.position, 0.6f);

        // 1. Move forward
        while (Vector3.Distance(attacker.position, targetPos) > 0.05f)
        {
            attacker.position = Vector3.MoveTowards(attacker.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // 2. We reached the target, apply damage now!
        onHit?.Invoke();

        // Pause for a split second for impact
        yield return new WaitForSeconds(0.1f);

        // 3. Move back
        while (Vector3.Distance(attacker.position, startPos) > 0.05f)
        {
            attacker.position = Vector3.MoveTowards(attacker.position, startPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // Snap perfectly back to the start
        attacker.position = startPos;

        // 4. Animation finished, move to the next phase
        onComplete?.Invoke();
    }
}