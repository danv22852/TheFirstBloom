using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System;
public enum BloomState
{
    Stable, // 0-24
    Low,    // 25-49
    Medium, // 50-74
    High,   // 75-99
    Max     // 100
}
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
    public int currentBloom = 0;
    public BloomState currentBloomState = BloomState.Stable;

    // This function automatically categorizes the bloom number into a state
    private void UpdateBloomState()
    {
        // Clamp the bloom so it never accidentally drops below 0 or goes above 100
        currentBloom = Mathf.Clamp(currentBloom, 0, 100);

        if (currentBloom >= 100)
            currentBloomState = BloomState.Max;
        else if (currentBloom >= 75)
            currentBloomState = BloomState.High;
        else if (currentBloom >= 50)
            currentBloomState = BloomState.Medium;
        else if (currentBloom >= 25)
            currentBloomState = BloomState.Low;
        else
            currentBloomState = BloomState.Stable;
    }

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
    public TextMeshProUGUI bloomText;

    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject skillMenuPanel;
    public GameObject itemMenuPanel;

    [Header("Animation Settings")]
    public Transform playerTransform;
    public Transform enemyTransform;
    public float moveSpeed = 15f;

    private void Start()
    {
        UpdateBloomState();
        UpdateHealthUI();
        DetermineFirstTurn();
    }

    private void DetermineFirstTurn()
    {
        if (playerSpeed >= enemySpeed)
        {
            Debug.Log("Player goes first.");
            PlayerStartTurn();
        }
        else
        {
            Debug.Log("Enemy goes first.");
            EnemyTurn();
        }
    }

    private void PlayerStartTurn()
    {
        isPlayerTurn = true;
        hasUsedItemThisTurn = false;
        Debug.Log("It is now the Player's turn.");
        // Check for 100% Bloom takeover logic here in the future [cite: 45]
    }

    // --- PLAYER ACTIONS ---

    public void OnAttackButton()
    {
        if (!isPlayerTurn) return;

        // Disable turn immediately so player can't spam click while animating
        isPlayerTurn = false;

        Debug.Log("Player uses Basic Attack!");

        StartCoroutine(PerformMeleeAttack(playerTransform, enemyTransform,
            onHit: () =>
            {
                // Damage from the attack scales with the player's strength stat[cite: 77].
                var damage = playerStrength;
                var actualDamage = Mathf.Max(1, damage);

                enemyHealth -= actualDamage;
                UpdateHealthUI();
                Debug.Log("Dealt " + actualDamage + " damage to the enemy.");
            },
            onComplete: () =>
            {
                CheckWinConditionOrContinue();
            }));
    }

    public void OnSkillButton()
    {
        OpenSkillMenu();
    }

    public void UseSymbioteSwipe()
    {
        isPlayerTurn = false; // Prevent spam clicking
        BackToMainMenu();     // Close the menu automatically

        Debug.Log("Player uses Symbiote Swipe!");

        var bloomCost = 1;
        var baseSkillDamage = 10;

        currentBloom += bloomCost;
        UpdateBloomState();
        Debug.Log("Bloom increased by " + bloomCost + ". State is now: " + currentBloomState);

        StartCoroutine(PerformMeleeAttack(playerTransform, enemyTransform,
            onHit: () =>
            {
                enemyHealth -= baseSkillDamage;
                UpdateHealthUI();
            },
            onComplete: () =>
            {
                CheckWinConditionOrContinue();
            }));
    }

    public void OnItemButton()
    {
        OpenItemMenu();
    }
    public void UseHealItem()
    {
        if (hasUsedItemThisTurn)
        {
            Debug.Log("You have already used an item this turn!");
            return;
        }

        Debug.Log("Player uses a Healing Item!");
        var healAmount = 20;
        playerHealth += healAmount;

        hasUsedItemThisTurn = true;
        UpdateHealthUI();
        BackToMainMenu(); // Close the menu
    }

    public void OnRunButton()
    {
        if (!isPlayerTurn) return;

        // The player loses the ability to run from combat at low bloom.
        if (currentBloomState >= BloomState.Low)
        {
            Debug.Log("You are in " + currentBloomState + " Bloom! The symbiote won't let you run!");
            return;
        }

        var escapeChance = UnityEngine.Random.Range(0, 100);
        if (escapeChance > 50)
        {
            Debug.Log("Escaped successfully!");
            SceneManager.LoadScene("firstFloor");
        }
        else
        {
            Debug.Log("Failed to escape!");
            isPlayerTurn = false;
            EnemyTurn();
        }
    }

    private void CheckWinConditionOrContinue()
    {
        if (enemyHealth <= 0)
        {
            Debug.Log("Enemy defeated!");

            // Add the current enemy's ID to the graveyard list
            if (!GameManager.defeatedEnemies.Contains(GameManager.currentEnemyID))
            {
                GameManager.defeatedEnemies.Add(GameManager.currentEnemyID);
                Debug.Log(GameManager.currentEnemyID + " added to the graveyard.");
            }

            SceneManager.LoadScene("firstFloor");
        }
        else
        {
            EnemyTurn();
        }
    }

    // --- ENEMY LOGIC ---

    private void EnemyTurn()
    {
        Debug.Log("Enemy attacks!");

        StartCoroutine(PerformMeleeAttack(enemyTransform, playerTransform,
            onHit: () =>
            {
                var damage = enemyStrength;
                var actualDamage = Mathf.Max(1, damage - playerDefense);

                playerHealth -= actualDamage;
                UpdateHealthUI();
                Debug.Log("Player takes " + actualDamage + " damage.");
            },
            onComplete: () =>
            {
                if (playerHealth <= 0)
                {
                    Debug.Log("Player died. Game Over.");
                    SceneManager.LoadScene("MainMenu");
                }
                else
                {
                    PlayerStartTurn();
                }
            }));
    }

    private void UpdateHealthUI()
    {
        playerHP.text = "Player HP: " + playerHealth;
        enemyHP.text = "Enemy HP: " + enemyHealth;
        bloomText.text = "Bloom: " + currentBloom;
    }

    private IEnumerator PerformMeleeAttack(Transform attacker, Transform target, Action onHit, Action onComplete)
    {
        var startPos = attacker.position;
        var targetPos = Vector3.Lerp(startPos, target.position, 0.6f);

        while (Vector3.Distance(attacker.position, targetPos) > 0.05f)
        {
            attacker.position = Vector3.MoveTowards(attacker.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        onHit?.Invoke();
        yield return new WaitForSeconds(0.1f);

        while (Vector3.Distance(attacker.position, startPos) > 0.05f)
        {
            attacker.position = Vector3.MoveTowards(attacker.position, startPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        attacker.position = startPos;
        onComplete?.Invoke();
    }

    // --- MENU NAVIGATION ---

    public void OpenSkillMenu()
    {
        if (!isPlayerTurn) return;
        mainMenuPanel.SetActive(false);
        skillMenuPanel.SetActive(true);
    }

    public void OpenItemMenu()
    {
        if (!isPlayerTurn) return;
        mainMenuPanel.SetActive(false);
        itemMenuPanel.SetActive(true);
    }

    public void BackToMainMenu()
    {
        skillMenuPanel.SetActive(false);
        itemMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
}