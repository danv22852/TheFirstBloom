using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System;
public enum BloomState
{
    Stable, // 0-24
    Low,    // 25-49
    Medium, // 50-74
    High,   // 75-99
    Total     // 100
}

public class CombatSystem : MonoBehaviour
{
    private UnityEngine.EventSystems.EventSystem cachedEventSystem;

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

    [Header("Bloom Hijack UI")]
    public TextMeshProUGUI attackButtonText; // To visually change "Attack" to "Symbiote Swipe"
    private bool isAttackHijacked = false;   // Tracks if the current turn is hijacked

    // This function automatically categorizes the bloom number into a state
    public void UpdateBloomState()
    {
        // Cap the Bloom at 100 so it doesn't break the UI
        if (currentBloom > 100) 
        {
            currentBloom = 100;
        }

        // --- NEW 100-POINT THRESHOLD LOGIC ---
        if (currentBloom >= 100)
        {
            currentBloomState = BloomState.Total;
        }
        else if (currentBloom >= 75)
        {
            currentBloomState = BloomState.High;
        }
        else if (currentBloom >= 50)
        {
            currentBloomState = BloomState.Medium;
        }
        else if (currentBloom >= 25)
        {
            currentBloomState = BloomState.Low;
        }
        else
        {
            currentBloomState = BloomState.Stable;
        }

        Debug.Log("Current Bloom: " + currentBloom + " | State: " + currentBloomState);
        // (Update your UI text/sliders here if you have them!)
    }

    // --- ENEMY STATS ---
    [Header("Enemy Stats")]
    public EnemyData currentEnemy;
    private int enemyHealth;
    private int enemySpeed; // Runtime copy — modifications here won't affect the asset

    // --- COMBAT STATE ---
    private bool isPlayerTurn = false;
    private bool hasUsedItemThisTurn = false;

    // --- UI ELEMENTS ---
    [Header("UI Elements")]
    public TextMeshProUGUI playerHP;
    public TextMeshProUGUI enemyHP;
    public TextMeshProUGUI bloomText;

    [Header("Keyboard Navigation Defaults")]
    public GameObject mainDefaultButton;  // E.g., The Attack Button
    public GameObject skillDefaultButton; // E.g., Symbiote Swipe
    public GameObject itemDefaultButton;  // E.g., The Heal Button

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

    [Header("Player Stats (Testing Fallbacks)")]
    public int testPlayerMaxHealth = 100;
    public int testPlayerStrength = 15;
    public int testPlayerSpeed = 10;
    public int testPlayerDefense = 5;

    private void Start()
    {
        cachedEventSystem = UnityEngine.EventSystems.EventSystem.current;


        if (GameManager.Instance != null && GameManager.Instance.playerData != null)
        {
            // The GameManager exists! We must be playing the full game.
            var pd = GameManager.Instance.playerData;
            playerHealth = pd.currentHP;
            playerMaxHealth = pd.maxHP;
            playerStrength = pd.strength;
            playerSpeed = pd.speed;
            playerDefense = pd.defense;
        }
        else
        {
            // The GameManager is missing! We must be testing the Combat Scene directly.
            Debug.Log("<color=cyan>TESTING MODE:</color> No GameManager found. Using Inspector Fallback Stats!");
            playerMaxHealth = testPlayerMaxHealth;
            playerHealth = testPlayerMaxHealth; // Start testing at full health
            playerStrength = testPlayerStrength;
            playerSpeed = testPlayerSpeed;
            playerDefense = testPlayerDefense;
        }

        // Load the enemy from GameManager if an ID has been set
        if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.currentEnemyID))
        {
            var found = GameManager.Instance.GetEnemyByID(GameManager.currentEnemyID);
            if (found != null) currentEnemy = found;
        }

        // Safety Check: Make sure we have an enemy to fight!
        if (currentEnemy == null)
        {
            Debug.LogError("CRASH AVOIDED: No Enemy Data! Drag an Enemy ScriptableObject into the Inspector!");
            return;
        }

        // 1. Memorize the EventSystem so the Killswitch never loses it!
        

        // Load the enemy from GameManager if an ID has been set
        if (!string.IsNullOrEmpty(GameManager.currentEnemyID))
        {
            var found = GameManager.Instance.GetEnemyByID(GameManager.currentEnemyID);
            if (found != null) currentEnemy = found;
        }

        var sr = enemyTransform.GetComponent<SpriteRenderer>();
        if (currentEnemy.enemySprite != null && sr != null)
        {
            sr.sprite = currentEnemy.enemySprite;
        }

        enemyHealth = currentEnemy.maxHP;
        enemySpeed = currentEnemy.speed;
        
        // Calculate your Bloom thresholds right away
        UpdateBloomState();
        UpdateHealthUI();
        
        DetermineFirstTurn();

        // --- THE TURN 1 FIX ---
        // If the player won the speed tie and goes first, run the menu setup.
        // This guarantees the Medium Bloom hijack dice is rolled on Turn 1!
        // --- TURN 1 ---
        if (isPlayerTurn)
        {
            PlayerStartTurn(); // This triggers the roll and opens the menu
        }
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

    public void PlayerStartTurn()
    {
        isPlayerTurn = true;
        hasUsedItemThisTurn = false;
        Debug.Log("It is now the Player's turn.");

        // 1. Reset the button to its safe, normal state
        isAttackHijacked = false;
        attackButtonText.text = "Attack"; 
        attackButtonText.color = Color.white; 

        // --- NEW: THE HIGH BLOOM GLASS CANNON (75-99) ---
        if (currentBloomState >= BloomState.High)
        {
            Debug.Log("High Bloom! The Symbiote completely takes over your basic attacks.");
            
            // 100% chance to hijack the attack button
            isAttackHijacked = true;
            attackButtonText.text = "Symbiote Swipe"; 
            attackButtonText.color = Color.red; // Red for danger/health cost!
        }
        // --- THE MEDIUM BLOOM HIJACK (50-74) ---
        else if (currentBloomState >= BloomState.Medium)
        {
            var hijackChance = UnityEngine.Random.Range(0, 100); 
            if (hijackChance < 30) 
            {
                isAttackHijacked = true;
                attackButtonText.text = "Symbiote Swipe"; 
                attackButtonText.color = Color.magenta; 
            }
        }

        // 3. Now that the math is done, safely bring up the menu
        BackToMainMenu();
    }

    // --- PLAYER ACTIONS ---

    public void OnAttackButton()
    {
        if (!isPlayerTurn) return;

        // Disable turn immediately so player can't spam click while animating

        if (isAttackHijacked)
        {
            Debug.Log("The Symbiote hijacked your attack!");
            UseSymbioteSwipe(); 
            return; // Stop reading this function completely!
        }

        HideAllMenus();
        isPlayerTurn = false;

        Debug.Log("Player uses Basic Attack!");

        StartCoroutine(PerformMeleeAttack(playerTransform, enemyTransform,
            onHit: () =>
            {
                var actualDamage = Mathf.Max(1, playerStrength - currentEnemy.defense);
                enemyHealth -= actualDamage;
                UpdateHealthUI();
                Debug.Log("Dealt " + actualDamage + " damage to the enemy.");

                // Shakes Enemy for 0.2 seconds, with a strength of 0.15
                StartCoroutine(ShakeSprite(enemyTransform, 0.2f, 0.15f));
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
        if (!isPlayerTurn) return; 
        isPlayerTurn = false; 
        HideAllMenus();     

        // 1. New Cost Rule: Exactly 3 Bloom
        var bloomCost = 3; 
        
        // 2. The 10% Strength Buff
        float statMultiplier = GetBloomStatMultiplier();
        var baseSkillDamage = UnityEngine.Random.Range(28, 33);
        
        // Apply the 10% buff and round it to a whole number
        int finalDamage = Mathf.RoundToInt(baseSkillDamage * statMultiplier); 

        // 3. The Self-Damage Penalty (Only applied if in High Bloom)
        if (currentBloomState >= BloomState.High)
        {
            // The symbiote feeds on the host! (Takes 5 to 10 damage)
            var selfDamage = UnityEngine.Random.Range(5, 11);
            playerHealth -= selfDamage;
            UpdateHealthUI();

            // player shake due to self damage
            StartCoroutine(ShakeSprite(playerTransform, 0.4f, 0.25f));

            Debug.Log("High Bloom Penalty! Player takes " + selfDamage + " damage to fuel the attack!");

            // Check if the symbiote just killed the player before the attack even lands!
            if (playerHealth <= 0)
            {
                Debug.Log("The host was consumed. Game Over.");
                // Run your Game Over logic here and 'return;' to stop the attack
            }
        }

        // Apply Bloom cost
        currentBloom += bloomCost;
        UpdateBloomState();
        
        StartCoroutine(PerformSkillAnimation(playerTransform, enemyTransform,
            onHit: () =>
            {
                enemyHealth -= finalDamage;
                UpdateHealthUI();
                Debug.Log("Symbiote Swipes for " + finalDamage + " damage!");

                StartCoroutine(ShakeSprite(enemyTransform, 0.3f, 0.3f));
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

        if (GameManager.Instance.playerData.healthPotions <= 0)
        {
            Debug.Log("No potions left!");
            return;
        }

        // --- DEDUCT THE ITEM FROM INVENTORY ---
        GameManager.Instance.playerData.healthPotions--;
        isPlayerTurn = false; 
        
        // --- INSTANTLY HIDE THE MENU ---
        HideAllMenus(); 

        StartCoroutine(PerformItemAnimation(playerTransform, 
            onComplete: () => 
            {
                Debug.Log("Player uses a Healing Item!");
                var healAmount = 20; 
                playerHealth = Mathf.Min(playerHealth + healAmount, playerMaxHealth);
                
                hasUsedItemThisTurn = true;
                UpdateHealthUI();
                
                isPlayerTurn = true; 
                
                // --- BRING THE MENU BACK NOW ---
                // The animation is done, so it's safe to give the player control again!
                BackToMainMenu(); 
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

        // --- THE PLAYER COMMITTED: HIDE THE MENUS ---
        HideAllMenus();

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
            EnemyTurn(); // The enemy's coroutine will naturally bring the menu back when it finishes!
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

    // Call this whenever you calculate damage, speed, or defense
    public float GetBloomStatMultiplier()
    {
        if (currentBloomState >= BloomState.High)
        {
            return 1.10f; // 10% Increase
        }
        return 1.0f; // Normal stats
    }

    private void EnemyTurn()
    {
        Debug.Log(currentEnemy.enemyName + "'s turn!");
        
        var skill = PickSkill();

        StartCoroutine(PerformMeleeAttack(enemyTransform, playerTransform,
            onHit: () =>
            {
                if (skill != null)
                {
                    skill.Execute(this, currentEnemy);
                    
                    // Shake the player even if it's a special skill!
                    StartCoroutine(ShakeSprite(playerTransform, 0.3f, 0.2f));
                }
                else
                {
                    // --- NEW: APPLY 10% HIGH BLOOM DEFENSE BUFF ---
                    float statMultiplier = GetBloomStatMultiplier();
                    int effectiveDefense = Mathf.RoundToInt(playerDefense * statMultiplier);

                    // Fallback basic attack using the buffed defense
                    var actualDamage = Mathf.Max(1, currentEnemy.strength - effectiveDefense);
                    playerHealth -= actualDamage;
                    UpdateHealthUI();
                    
                    Debug.Log("Player takes " + actualDamage + " damage. (Effective Defense: " + effectiveDefense + ")");

                    // --- SHAKE THE PLAYER ---
                    StartCoroutine(ShakeSprite(playerTransform, 0.3f, 0.2f));
                }
            },
            onComplete: () =>
            {
                // --- NEW: TRIGGER THE PAUSE COROUTINE ---
                // Wait 1 second before doing the Game Over check or passing the turn
                StartCoroutine(WaitAndPassTurn(1.0f));
            }));
    }

    private IEnumerator WaitAndPassTurn(float delayTime)
    {
        // 1. Give the player a second to process the damage/shake they just took
        yield return new WaitForSeconds(delayTime);

        // 2. Now check if they survived
        if (playerHealth <= 0)
        {
            Debug.Log("Player died. Game Over.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
        else
        {
            // 3. Bring the menu back up
            PlayerStartTurn();
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

    private IEnumerator ShakeSprite(Transform targetTransform, float duration, float magnitude)
    {
        // 1. Remember the exact starting position
        Vector3 originalPos = targetTransform.localPosition;
        float elapsed = 0.0f;

        // 2. Vibrate the sprite until the timer runs out
        while (elapsed < duration)
        {
            // Pick a tiny random direction
            float x = originalPos.x + UnityEngine.Random.Range(-1f, 1f) * magnitude;
            float y = originalPos.y + UnityEngine.Random.Range(-1f, 1f) * magnitude;

            targetTransform.localPosition = new Vector3(x, y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // 3. Snap back to the exact starting position so they don't drift away!
        targetTransform.localPosition = originalPos;
    }

    // --- MENU NAVIGATION ---

    public void OpenSkillMenu()
    {
        if (!isPlayerTurn) return;
        mainMenuPanel.SetActive(false);
        skillMenuPanel.SetActive(true);

        // Use the new coroutine to highlight the button safely
        StartCoroutine(HighlightButtonSafe(skillDefaultButton));
    }

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
        // 1. Turn the EventSystem back on
        if (cachedEventSystem != null)
        {
            cachedEventSystem.enabled = true; 
        }

        // 2. Just swap the panels
        skillMenuPanel.SetActive(false);
        itemMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);

        // 3. Highlight the default button
        StartCoroutine(HighlightButtonSafe(mainDefaultButton));
    }

    private void HideAllMenus()
    {
        // 1. Unplug the keyboard/controller so no hidden buttons can be spammed
        if (cachedEventSystem != null)
        {
            cachedEventSystem.enabled = false;
        }

        // 2. Hide all the visual UI panels
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (skillMenuPanel != null) skillMenuPanel.SetActive(false);
        if (itemMenuPanel != null) itemMenuPanel.SetActive(false);
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
            if (skillMenuPanel.activeSelf || itemMenuPanel.activeSelf)
            {
                BackToMainMenu();
            }
        }
    }
}