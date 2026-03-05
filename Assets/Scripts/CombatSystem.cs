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

    [Header("Item Menu UI")]
    public UnityEngine.UI.Button healItemButton; 
    public TextMeshProUGUI healItemText;

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
        isPlayerTurn = false; 
        BackToMainMenu();     

        Debug.Log("Player uses Symbiote Swipe!");

        var bloomCost = 1; 
        var baseSkillDamage = 10; 

        currentBloom += bloomCost;
        UpdateBloomState();
        
        // CALLING THE NEW SKILL COROUTINE
        StartCoroutine(PerformSkillAnimation(playerTransform, enemyTransform,
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

        // Double check just in case!
        if (GameManager.healthPotions <= 0)
        {
            Debug.Log("No potions left!");
            return;
        }

        // --- DEDUCT THE ITEM FROM INVENTORY ---
        GameManager.healthPotions--;

        isPlayerTurn = false; 
        BackToMainMenu(); 

        StartCoroutine(PerformItemAnimation(playerTransform, 
            onComplete: () => 
            {
                Debug.Log("Player uses a Healing Item!");
                var healAmount = 20; 
                playerHealth += healAmount;
                
                hasUsedItemThisTurn = true;
                UpdateHealthUI();
                
                isPlayerTurn = true; 
            }));
    }

    public void OnRunButton()
    {
        if (!isPlayerTurn) return;

        if (currentBloomState >= BloomState.Low)
        {
            Debug.Log("You are in " + currentBloomState + " Bloom! The symbiote won't let you run!");
            return;
        }

        var escapeChance = UnityEngine.Random.Range(0, 100);
        if (escapeChance > 50) 
        {
            Debug.Log("Escaped successfully!");
            isPlayerTurn = false; // Lock controls
            
            // CALLING THE NEW RUN COROUTINE
            StartCoroutine(PerformRunAnimation(playerTransform, 
                onComplete: () => 
                {
                    // Load the overworld AFTER the player has run offscreen
                    SceneManager.LoadScene("firstFloor");
                }));
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

    // --- NEW ANIMATIONS ---

    private IEnumerator PerformItemAnimation(Transform actor, Action onComplete)
    {
        var startPos = actor.position;
        var peakPos = startPos + Vector3.up * 1.5f; // Move up 1.5 units

        // 1. Move straight up
        while (Vector3.Distance(actor.position, peakPos) > 0.05f)
        {
            actor.position = Vector3.MoveTowards(actor.position, peakPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // Pause for a tiny fraction of a second at the top
        yield return new WaitForSeconds(0.1f);

        // 2. Move straight down back to start
        while (Vector3.Distance(actor.position, startPos) > 0.05f)
        {
            actor.position = Vector3.MoveTowards(actor.position, startPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        actor.position = startPos;
        onComplete?.Invoke();
    }

    private IEnumerator PerformSkillAnimation(Transform attacker, Transform target, Action onHit, Action onComplete)
    {
        var startPos = attacker.position;
        var targetPos = Vector3.Lerp(startPos, target.position, 0.6f); // Target point slightly in front of enemy
        
        // Calculate the peak of the jump (halfway forward, and 2 units up)
        var midPoint = Vector3.Lerp(startPos, targetPos, 0.5f);
        var peakPos = midPoint + Vector3.up * 2f; 

        // 1. Leap diagonally up to the peak
        while (Vector3.Distance(attacker.position, peakPos) > 0.05f)
        {
            attacker.position = Vector3.MoveTowards(attacker.position, peakPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // 2. Dive diagonally down to the enemy
        while (Vector3.Distance(attacker.position, targetPos) > 0.05f)
        {
            attacker.position = Vector3.MoveTowards(attacker.position, targetPos, moveSpeed * 1.5f * Time.deltaTime); // Dive slightly faster
            yield return null;
        }

        // We hit the enemy! Apply damage.
        onHit?.Invoke();
        yield return new WaitForSeconds(0.1f);

        // 3. Return to the start position
        while (Vector3.Distance(attacker.position, startPos) > 0.05f)
        {
            attacker.position = Vector3.MoveTowards(attacker.position, startPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        attacker.position = startPos;
        onComplete?.Invoke();
    }

    private IEnumerator PerformRunAnimation(Transform actor, Action onComplete)
    {
        var startPos = actor.position;
        var offscreenPos = startPos + (Vector3.left * 10f); // Move 10 units to the left

        // 1. Sprint offscreen
        while (Vector3.Distance(actor.position, offscreenPos) > 0.05f)
        {
            // Multiplying speed by 1.5 so running away feels urgent
            actor.position = Vector3.MoveTowards(actor.position, offscreenPos, moveSpeed * 1.5f * Time.deltaTime);
            yield return null;
        }

        // 2. Transition scene
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
        
        UpdateItemUI(); // Refresh the numbers before showing the menu

        mainMenuPanel.SetActive(false);
        itemMenuPanel.SetActive(true);
    }

    private void UpdateItemUI()
    {
        // Check our global inventory
        if (GameManager.healthPotions > 0)
        {
            healItemText.text = "Heal +20 (x" + GameManager.healthPotions + ")";
            healItemButton.interactable = true; // Button is clickable
        }
        else
        {
            healItemText.text = "Out of Potions!";
            healItemButton.interactable = false; // Grays out the button
        }
    }
    

    public void BackToMainMenu()
    {
        skillMenuPanel.SetActive(false);
        itemMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
}