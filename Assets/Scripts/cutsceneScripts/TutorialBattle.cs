using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System;
public class TutorialBattle : MonoBehaviour
{
    // --- PLAYER STATS ---
    // All player stats are read from PlayerData at the start of combat.
    // Modify these runtime copies during combat — never write back to PlayerData directly.
    private int playerHealth;
    private int playerMaxHealth;
    public int playerStrength;
    public int playerSpeed;
    public int playerDefense;

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
    public EnemyData currentEnemy;
    private int enemyHealth;
    private int enemySpeed; // Runtime copy — modifications here won't affect the asset



    // --- COMBAT STATE ---
    private bool isPlayerTurn = false;
    private bool hasUsedItemThisTurn = false;
    private int enemyTurnCount = 0;

    // --- UI ELEMENTS ---
    [Header("UI Elements")]
    public TextMeshProUGUI playerHP;
    public TextMeshProUGUI enemyHP;
    public TextMeshProUGUI bloomText;

    [Header("Keyboard Navigation Defaults")]
    public GameObject mainDefaultButton;  // E.g., The Attack Button
    public GameObject itemDefaultButton;  // E.g., The Heal Button

    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
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
        // Read player stats from PlayerData
        var pd = GameManager.Instance.playerData;
        playerHealth = pd.currentHP;
        playerMaxHealth = pd.maxHP;
        playerStrength = pd.strength;
        playerSpeed = pd.speed;
        playerDefense = pd.defense;

        enemyHealth = currentEnemy.maxHP;
        enemySpeed = currentEnemy.speed;
        UpdateBloomState();
        UpdateHealthUI();
        DetermineFirstTurn();

        // Snap focus to the Main Menu's default button when combat starts!
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(mainDefaultButton);
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
                var actualDamage = Mathf.Max(1, playerStrength - currentEnemy.defense);
                enemyHealth -= actualDamage;
                UpdateHealthUI();
                Debug.Log("Dealt " + actualDamage + " damage to the enemy.");
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
            BackToMainMenu();
            return;
        }

        if (GameManager.Instance.playerData.healthPotions <= 0)
        {
            Debug.Log("No potions left!");
            BackToMainMenu();
            return;
        }

        // --- DEDUCT THE ITEM FROM INVENTORY ---
        GameManager.Instance.playerData.healthPotions--;
        isPlayerTurn = false; 
        BackToMainMenu(); 

        StartCoroutine(PerformItemAnimation(playerTransform, 
            onComplete: () => 
            {
                Debug.Log("Player uses a Healing Item!");
                var healAmount = 20; 
                playerHealth = Mathf.Min(playerHealth + healAmount, playerMaxHealth);
                
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

            // Persist remaining HP back to PlayerData
            GameManager.Instance.playerData.currentHP = playerHealth;

            // Add the current enemy's ID to the graveyard list
            if (!GameManager.Instance.playerData.defeatedEnemies.Contains(GameManager.currentEnemyID))
            {
                GameManager.Instance.playerData.defeatedEnemies.Add(GameManager.currentEnemyID);
                Debug.Log(GameManager.currentEnemyID + " added to the graveyard.");
            }

            SceneManager.LoadScene("firstFloor");
        }
        else
        {
            EnemyTurn();
        }
    }

    private void EnemyTurn()
    {
        enemyTurnCount++;
        Debug.Log(currentEnemy.enemyName + "'s turn! (Turn " + enemyTurnCount + ")");

        // --- THE SCRIPTED EVENT: TURN 4 ---
        if (enemyTurnCount == 4)
        {
            Debug.Log("Enemy prepares a devastating scripted skill!");
            
            StartCoroutine(PerformSkillAnimation(enemyTransform, playerTransform,
                onHit: () =>
                {
                    Debug.Log("DEVASTATING BLOW! Player drops to 1 HP!");
                    playerHealth = 1;
                    UpdateHealthUI();
                },
                onComplete: () =>
                {
                    // Persist HP and mark tutorial complete
                    GameManager.Instance.playerData.currentHP = playerHealth;
                    GameManager.Instance.playerData.finishedTutorial = true;
                    SceneManager.LoadScene("firstFloor");
                    PlayerStartTurn();
                }));
                
            return;     
        }

        // --- NORMAL COMBAT LOGIC ---
        else
        {
            Debug.Log("Enemy uses a Basic Attack!");
            StartCoroutine(PerformMeleeAttack(enemyTransform, playerTransform,
                onHit: () =>
                {
                    var actualDamage = Mathf.Max(1, currentEnemy.strength - playerDefense);
                    playerHealth -= actualDamage;
                    UpdateHealthUI();
                    Debug.Log("Player takes " + actualDamage + " damage.");
                },
                onComplete: () =>
                {
                    // Straight back to the player!
                    PlayerStartTurn();
                }));
        }
    }

    private SkillBase PickSkill()
    {
        if (currentEnemy.skills == null || currentEnemy.skills.Count == 0) return null;

        int totalWeight = 0;
        foreach (var skill in currentEnemy.skills)
            totalWeight += skill.weight;

        int roll = UnityEngine.Random.Range(0, totalWeight);
        int cumulative = 0;
        foreach (var skill in currentEnemy.skills)
        {
            cumulative += skill.weight;
            if (roll < cumulative) return skill;
        }

        return currentEnemy.skills[0];
    }

    // Called by skills
    public void DealDamageToPlayer(int amount, bool ignoreDefense)
    {
        var actualDamage = ignoreDefense ? amount : Mathf.Max(1, amount - playerDefense);
        playerHealth -= actualDamage;
        UpdateHealthUI();
    }

    public void EndEnemyTurn() { }

    private void UpdateHealthUI()
    {
        playerHP.text = "Player HP: " + playerHealth + " / " + playerMaxHealth;
        enemyHP.text = "Enemy HP: " + enemyHealth + " / " + currentEnemy.maxHP;
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



    public void OpenItemMenu()
    {
        if (!isPlayerTurn) return;
        UpdateItemUI(); 
        
        mainMenuPanel.SetActive(false);
        itemMenuPanel.SetActive(true);

        // Use the new coroutine to highlight the button safely
        StartCoroutine(HighlightButtonSafe(itemDefaultButton));
    }

    public void BackToMainMenu()
    {
        itemMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);

        // Use the new coroutine to highlight the button safely
        StartCoroutine(HighlightButtonSafe(mainDefaultButton));
    }

    // This forces Unity to wait one frame so the button is fully awake before highlighting it
    private IEnumerator HighlightButtonSafe(GameObject buttonToHighlight)
    {
        EventSystem.current.SetSelectedGameObject(null); // Clear the old selection
        yield return null; // Wait exactly one frame
        EventSystem.current.SetSelectedGameObject(buttonToHighlight); // Highlight the new button
    }

    private void UpdateItemUI()
    {
        var potions = GameManager.Instance.playerData.healthPotions;
        if (potions > 0)
        {
            healItemText.text = "Heal +20 (x" + potions + ")";
            healItemButton.interactable = true;
        }
        else
        {
            healItemText.text = "Out of Potions!";
            healItemButton.interactable = false;
        }
    }
    private void Update()
    {
        // If it is the player's turn and they press the p or x key...
        if (isPlayerTurn && (Input.GetKeyDown(KeyCode.P)) || (Input.GetKeyDown(KeyCode.X)))
        {
            // If either sub-menu is currently open, close it and go back!
            if ( itemMenuPanel.activeSelf)
            {
                BackToMainMenu();
            }
        }
    }
}