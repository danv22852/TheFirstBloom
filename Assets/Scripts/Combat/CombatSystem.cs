using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;
using System.Collections.Generic;

public enum VignetteType { Low, Medium, High }

public class CombatSystem : MonoBehaviour
{
    private UnityEngine.EventSystems.EventSystem cachedEventSystem;

    // --- PLAYER STATS ---
    private int playerHealth;
    private int playerMaxHealth;
    public int playerStrength;
    public int playerSpeed;
    public int playerDefense;

    [Header("Bloom Hijack UI")]
    public TextMeshProUGUI attackButtonText; 
    private bool isAttackHijacked = false;   

    // --- ENEMY STATS ---
    [Header("Enemy Stats")]
    public EnemyData currentEnemy;
    private int enemyHealth;
    private int enemySpeed; 

    // --- COMBAT STATE ---
    private bool isPlayerTurn = false;
    private bool hasUsedItemThisTurn = false;
    private float guardDamageReduction = 0f;
    private int guardTurnsRemaining = 0;
    private bool enemyIsStunned = false;

    // --- UI ELEMENTS ---
    [Header("UI Elements")]
    public TextMeshProUGUI playerHP;
    public TextMeshProUGUI enemyHP;
    public TextMeshProUGUI bloomText;

    [Header("Visual Bars")]
    public Slider playerHpSlider;
    public Slider enemyHpSlider;
    public Slider bloomSlider;
    public Image bloomFillImage; // We need this specific piece to change the color!

    [Header("Bloom Colors")]
    public Color stableBloomColor = new Color(0.8f, 0.6f, 1f); // Light Lilac / Soft Purple
    public Color lowBloomColor = new Color(0.4f, 0f, 0.6f); // Dark Purple
    public Color mediumBloomColor = new Color(1f, 0f, 0.8f); // Hot Pink
    public Color highBloomColor = new Color(1f, 0f, 0f); // Blood Red

    [Header("Vignette Screen Effect")]
    public Image vignetteOverlay; // The full-screen image we just made
    public float vignetteFlashDuration = 0.5f; // How long the flash lasts
    public float vignetteMaxAlpha = 0.35f; // How dark the flash gets (0 to 1)

    [Header("Bloom Preview")]
    public Slider bloomPreviewSlider;

    [Header("Keyboard Navigation Defaults")]
    public GameObject mainDefaultButton;  
    public GameObject skillDefaultButton; 
    public GameObject itemDefaultButton;  

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
    public int testPlayerBloom = 0;
    public List<CoreTemplate> testCores = new List<CoreTemplate>();
    private BloomState testBloomState = BloomState.Stable;
    private BloomState lastKnownBloomState;

    [Header("UI Animation Trackers")]
    private Coroutine playerHpAnim;
    private Coroutine enemyHpAnim;
    private Coroutine bloomAnim;

    [Header("Tooltip UI")]
    public GameObject tooltipPanel;
    public TMPro.TextMeshProUGUI tooltipText; // Use standard Text if you aren't using TextMeshPro

    private Coroutine battleTextCoroutine;

    [Header("Symbiote Mechanics")]
    public CoreTemplate innateSymbioteSwipe;

    [Header("Level Up UI")]
    public GameObject levelUpPanel;
    public TextMeshProUGUI levelUpPointsText;
    public TextMeshProUGUI levelReadoutText;
    public TextMeshProUGUI warningText;
    
    // Text to show "Strength: 10 -> 11 (+1)"
    public TextMeshProUGUI hpReadoutText;
    public TextMeshProUGUI strReadoutText;
    public TextMeshProUGUI spdReadoutText;
    public TextMeshProUGUI defReadoutText;
    public TextMeshProUGUI luckReadoutText;

    // Snapshot variables to remember what the stats used to be!
    private int oldMaxHP, oldStr, oldSpd, oldDef, oldLuck;

    // --- SMART PROPERTIES FOR BLOOM ---
    // These automatically check if PlayerData exists. If it does, they read/write directly to it.
    // If not, they use the testing fallbacks so you can still test combat in isolation.
    private int ActiveBloom
    {
        get => GameManager.Instance != null ? GameManager.Instance.playerData.currentBloom : testPlayerBloom;
        set
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.playerData.currentBloom = value;
                GameManager.Instance.playerData.UpdateBloomState();
            }
            else
            {
                testPlayerBloom = value;
                UpdateTestBloomState();
            }
        }
    }

    private BloomState ActiveBloomState
    {
        get => GameManager.Instance != null ? GameManager.Instance.playerData.currentBloomState : testBloomState;
    }

    private void UpdateTestBloomState()
    {
        if (testPlayerBloom > 100) testPlayerBloom = 100;

        if (testPlayerBloom >= 100) testBloomState = BloomState.Total;
        else if (testPlayerBloom >= 75) testBloomState = BloomState.High;
        else if (testPlayerBloom >= 50) testBloomState = BloomState.Medium;
        else if (testPlayerBloom >= 25) testBloomState = BloomState.Low;
        else testBloomState = BloomState.Stable;
    }

    private IEnumerator Start() // Changed from 'void' to 'IEnumerator'
    {
        // --- 1. THE TUTORIAL PAUSE UI CHECK ---
        // If the tutorial script has set timeScale to 0, we sit in this loop.
        // We use WaitForSecondsRealtime because regular Time is frozen!
        while (Time.timeScale <= 0)
        {
            yield return new WaitForSecondsRealtime(0.1f);
        }

        // Brief 0.1s buffer to ensure the Tutorial object is fully destroyed 
        // and the EventSystem is ready for new selections.
        yield return new WaitForSecondsRealtime(0.1f);

        // --- 2. INITIALIZE SYSTEM ---
        cachedEventSystem = UnityEngine.EventSystems.EventSystem.current;

        // --- 3. LOAD PLAYER DATA ---
        if (GameManager.Instance != null && GameManager.Instance.playerData != null)
        {
            var pd = GameManager.Instance.playerData;
            playerHealth = pd.currentHP;
            playerMaxHealth = pd.maxHP;
            playerStrength = pd.strength;
            playerSpeed = pd.speed;
            playerDefense = pd.defense;
        }
        else
        {
            Debug.Log("<color=cyan>TESTING MODE:</color> No GameManager found. Using Inspector Fallback Stats!");
            playerMaxHealth = testPlayerMaxHealth;
            playerHealth = testPlayerMaxHealth;
            playerStrength = testPlayerStrength;
            playerSpeed = testPlayerSpeed;
            playerDefense = testPlayerDefense;
            UpdateTestBloomState();
        }

        if (GameManager.pendingEnemyData != null)
        {
            Debug.Log("<color=green>Direct Hand-off Successful! Loading: </color>" + GameManager.pendingEnemyData.enemyName);

            // Overwrite our fallback with the real Overworld enemy!
            currentEnemy = GameManager.pendingEnemyData;

            // Optional: Clear the GameManager's hands so it doesn't accidentally load the Hornet next time!
            GameManager.pendingEnemyData = null;
        }

        // --- 5. SET ENEMY VISUALS AND STATS ---
        if (enemyTransform == null)
        {
            Debug.LogError("ERROR: The 'Enemy Transform' slot in your CombatManager Inspector is empty!");
        }
        else
        {
            var sr = enemyTransform.GetComponent<SpriteRenderer>();
            if (currentEnemy.enemySprite != null && sr != null)
            {
                sr.sprite = currentEnemy.enemySprite;
                enemyTransform.localScale = new Vector3(currentEnemy.combatScale, currentEnemy.combatScale, 1f);
                sr.flipX = currentEnemy.flipSprite;
                Debug.Log("<color=cyan>3. Sprite assigned successfully!</color>");
            }
        }

        enemyHealth = currentEnemy.maxHP;
        enemySpeed = currentEnemy.speed;

        // Set the maximums for our sliders!
        if (playerHpSlider != null) playerHpSlider.maxValue = playerMaxHealth;
        if (enemyHpSlider != null) enemyHpSlider.maxValue = currentEnemy.maxHP;
        if (bloomSlider != null) bloomSlider.maxValue = 100;

        lastKnownBloomState = ActiveBloomState;

        // --- 5. START THE ROUND ---
        UpdateHealthUI(true);
        DetermineFirstTurn();

        // DetermineFirstTurn handles calling PlayerStartTurn() or EnemyTurn()
        // so we don't need to call PlayerStartTurn() again here.

        PopulateSkillMenu();
    }


    private void DetermineFirstTurn()
    {
        if (playerSpeed >= enemySpeed)
        {
            Debug.Log("Player goes first.");
            ShowBattleText("Speed Advantage! You strike first!", 2f);
            PlayerStartTurn();
        }
        else
        {
            Debug.Log("Enemy goes first.");
            ShowBattleText(currentEnemy.enemyName + " is faster! They attack first!", 2f);
            EnemyTurn();
        }
    }

    public void PlayerStartTurn()
    {
        isPlayerTurn = true;
        hasUsedItemThisTurn = false;
        
        isAttackHijacked = false;
        attackButtonText.text = "Attack"; 
        attackButtonText.color = Color.black; 

        // 1. Process Hijack Mechanics silently first
        if (ActiveBloomState >= BloomState.High)
        {
            isAttackHijacked = true;
            attackButtonText.text = "Symbiote Swipe"; 
            attackButtonText.color = Color.red; 
        }
        else if (ActiveBloomState >= BloomState.Medium)
        {
            var hijackChance = UnityEngine.Random.Range(0, 100); 
            if (hijackChance < 30) 
            {
                isAttackHijacked = true;
                attackButtonText.text = "Symbiote Swipe"; 
                attackButtonText.color = Color.magenta; 
            }
        }

        // 2. Determine what text to show!
        BloomState currentState = ActiveBloomState;

        // Did we cross a new threshold since our LAST turn?
        if (currentState != lastKnownBloomState) 
        {
            if (currentState > lastKnownBloomState)
            {
                if (currentState == BloomState.Low) ShowBattleText("The symbiote awakens. You can no longer run.", 3f);
                else if (currentState == BloomState.Medium) ShowBattleText("The symbiote is restless. It begins hijacking some of your attacks!", 3f);
                else if (currentState >= BloomState.High) ShowBattleText("The symbiote takes over! Attacks will now drain your HP!", 3.5f);
            }
            else
            {
                if (currentState == BloomState.Stable) ShowBattleText("You have regained full control.", 2.5f);
                else if (currentState == BloomState.Low) ShowBattleText("The symbiote's grip weakens.", 2.5f);
            }

            // Update the tracker so it doesn't spam the message next turn!
            lastKnownBloomState = currentState; 
        }
        else // If no threshold was crossed, show normal turn start text
        {
            if (isAttackHijacked && currentState >= BloomState.High)
            {
                ShowBattleText("The Symbiote hijacks your attacks!", 2f);
            }
            else if (isAttackHijacked)
            {
                ShowBattleText("The Symbiote surges and hijacks your attack!", 2f);
            }
            else
            {
                ShowBattleText("Your turn.", 1f);
            }
        }

        if (guardTurnsRemaining > 0)
        {
            guardTurnsRemaining--;
            if (guardTurnsRemaining == 0)
                ShowBattleText("Guard has worn off.", 2f);
        }

        BackToMainMenu();
    }

    private System.Collections.IEnumerator CombatIntroSequence()
    {
        // 1. Lock the UI so the player can't click "Attack" while the intro is playing
        if (cachedEventSystem != null) cachedEventSystem.enabled = false;

        // 2. Grab the speed stats (Change 'currentEnemy.speed' if your variable is named differently!)
        int playerSpeed = GameManager.Instance.playerData.speed;
        int enemySpeed = currentEnemy.speed; 

        // 3. Determine who is faster and announce it
        if (playerSpeed >= enemySpeed)
        {
            // Player is faster (or it's a tie)
            isPlayerTurn = true; 
            ShowBattleText("Speed Advantage! You strike first!", 2f);
        }
        else
        {
            // Enemy is faster
            isPlayerTurn = false;
            ShowBattleText(currentEnemy.enemyName + " is faster! They attack first!", 2f);
        }

        // 4. Wait for the player to read the text (2 seconds matches the text display time)
        yield return new WaitForSeconds(2f);

        // 5. Unlock the UI
        if (cachedEventSystem != null) cachedEventSystem.enabled = true;

        // 6. Officially start the correct turn
        if (isPlayerTurn)
        {
            // Call whatever function normally starts your player's turn 
            // (e.g., PlayerTurnSetup(), EnablePlayerUI(), etc.)
        }
        else
        {
            // Call your Enemy's attack
            EnemyTurn();
        }
    }

    public void OnAttackButton()
    {
        if (!isPlayerTurn) return;

        if (isAttackHijacked)
        {
            // The Symbiote doesn't care if it's equipped. It forces the attack!
            if (innateSymbioteSwipe != null)
            {
                UseCore(innateSymbioteSwipe);
            }
            else
            {
                Debug.LogError("CRITICAL: innate Symbiote Swipe Core is missing from the Inspector!");
            }
            return;
        }

        HideAllMenus();
        isPlayerTurn = false;

        // Text before the animation starts
        ShowBattleText("You attack!", 1.5f);

        StartCoroutine(PerformMeleeAttack(playerTransform, enemyTransform,
            onHit: () =>
            {
                var actualDamage = Mathf.Max(1, playerStrength - currentEnemy.defense);
                enemyHealth -= actualDamage;

                enemyHealth = Mathf.Max(0, enemyHealth);

                UpdateHealthUI();
                
                // Text the exact moment the hit connects!
                ShowBattleText("Dealt " + actualDamage + " damage to the enemy.", 2f);

                StartCoroutine(ShakeSprite(enemyTransform, 0.2f, 0.15f));
            },
            onComplete: () =>
            {
                if (enemyHealth <= 0) HandleEnemyDefeat(); 
                else CheckWinConditionOrContinue();
            }));
    }

    public void OnSkillButton()
    {
        OpenSkillMenu();
    }

    public void UseCore(CoreTemplate core)
    {
        if (!isPlayerTurn) return;
        isPlayerTurn = false;
        HideAllMenus();
        core.Execute(this);
    }
    
    // Quick helper function for the game over delay
    private void LoadMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void OnItemButton()
    {
        OpenItemMenu();
    }

    public void UseHealItem()
    {
        if (hasUsedItemThisTurn)
        {
            ShowBattleText("You already used an item this turn.", 1.5f);
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.playerData.healthPotions <= 0)
        {
            ShowBattleText("No potions left.", 1.5f);
            return;
        }
        else if (GameManager.Instance != null)
        {
            GameManager.Instance.playerData.healthPotions--;
        }

        isPlayerTurn = false; 
        HideAllMenus(); 
        
        ShowBattleText("You drink a Health Potion...", 1.5f);

        StartCoroutine(PerformItemAnimation(playerTransform, 
            onComplete: () => 
            {
                var healAmount = 20; 
                playerHealth = Mathf.Min(playerHealth + healAmount, playerMaxHealth);
                hasUsedItemThisTurn = true;
                UpdateHealthUI();
                
                ShowBattleText("Recovered " + healAmount + " HP!", 2f);
                
                isPlayerTurn = true; 
                BackToMainMenu(); 
            }));
    }

    public void OnRunButton()
    {
        if (!isPlayerTurn) return;

        // The Symbiote blocking the player from running
        if (ActiveBloomState >= BloomState.Low)
        {
            ShowBattleText("The Symbiote craves violence! You cannot flee!", 2f);
            return;
        }

        HideAllMenus();
        isPlayerTurn = false; // Lock the turn immediately so they can't spam buttons
        
        // Start the timed sequence!
        StartCoroutine(AttemptRunRoutine());
    }

    private System.Collections.IEnumerator AttemptRunRoutine()
    {
        // 1. Announce the attempt and WAIT
        ShowBattleText("You try to run away...", 1.5f);
        yield return new WaitForSeconds(1.5f);

        // 2. Roll the dice
        var escapeChance = UnityEngine.Random.Range(0, 100);
        if (escapeChance > 50) 
        {
            // SUCCESS! Run off screen.
            StartCoroutine(PerformRunAnimation(playerTransform, 
                onComplete: () => 
                {
                    ShowBattleText("Got away safely!", 1.5f);
                    PersistStatsToPlayerData();
                    
                    // Call our custom return function after a short delay so they can read the text!
                    Invoke(nameof(ReturnToOverworld), 1.5f); 
                }));
        }
        else
        {
            // FAILURE! 
            ShowBattleText("Failed to escape!", 1.5f); 
            
            // Wait for the player to read the failure text before the enemy hits them!
            yield return new WaitForSeconds(1.5f); 
            
            EnemyTurn(); 
        }
    }

    private void HandleEnemyDefeat()
    {
        int expGained = currentEnemy != null ? currentEnemy.expDrop : 0;
        bool leveledUp = false;

        if (GameManager.Instance != null && GameManager.Instance.playerData != null)
        {
            var pd = GameManager.Instance.playerData;
            
            // 1. TAKE THE SNAPSHOT BEFORE ADDING EXP!
            oldMaxHP = pd.maxHP;
            oldStr = pd.strength;
            oldSpd = pd.speed;
            oldDef = pd.defense;
            oldLuck = pd.luck;

            // 2. Add EXP and roll the random stats
            leveledUp = pd.expSystem.AddEXP(expGained, pd);
        }

        if (leveledUp)
        {
            ShowBattleText($"Victory!\nGained {expGained} EXP. LEVEL UP!", 2.5f);
            
            // If we leveled up, stop the battle text and open the menu after a delay!
            Invoke(nameof(OpenLevelUpMenu), 2.5f);
        }
        else
        {
            ShowBattleText($"Victory! {currentEnemy.enemyName} defeated.\nGained {expGained} EXP.", 2.5f);
            
            // If we didn't level up, just return to the map normally.
            Invoke(nameof(ReturnToOverworld), 2.5f);
        }

        PersistStatsToPlayerData(); 

        if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.encounteredInstanceID))
        {
            if (!GameManager.Instance.playerData.defeatedEnemies.Contains(GameManager.encounteredInstanceID))
                GameManager.Instance.playerData.defeatedEnemies.Add(GameManager.encounteredInstanceID);
            GameManager.encounteredInstanceID = ""; 
        }
    }

    // --- LEVEL UP MENU LOGIC ---

    private void OpenLevelUpMenu()
    {
        // Hide normal combat UI and show the level up screen
        HideAllMenus();

        if (cachedEventSystem != null) cachedEventSystem.enabled = true;

        if (tooltipPanel != null) tooltipPanel.SetActive(false);

        if (warningText != null) warningText.gameObject.SetActive(false); 
        
        levelUpPanel.SetActive(true);
        UpdateLevelUpUI();
    }

    private System.Collections.IEnumerator AutoCloseLevelUp()
    {
        // 1. Turn off the UI clicker so they can't keep spending points while the timer ticks down
        if (cachedEventSystem != null) cachedEventSystem.enabled = false;

        // 2. Repurpose the warning text to show a success message!
        if (warningText != null)
        {
            warningText.text = "Level Up Complete!";
            warningText.color = Color.green;
            warningText.gameObject.SetActive(true);
        }

        // 3. Wait for exactly 1 second
        yield return new WaitForSeconds(1.0f);

        // 4. Close the panel, save the stats, and head back to the Overworld
        levelUpPanel.SetActive(false);
        PersistStatsToPlayerData();
        ReturnToOverworld();
    }

    private void UpdateLevelUpUI()
    {
        if (GameManager.Instance == null || GameManager.Instance.playerData == null) return;
        var pd = GameManager.Instance.playerData;

        levelUpPointsText.text = "Skill Points Available: " + pd.expSystem.availableSkillPoints;
        if (levelReadoutText != null) levelReadoutText.text = "Level: " + pd.expSystem.level;

        hpReadoutText.text = FormatStatText("Max HP", oldMaxHP, pd.maxHP);
        strReadoutText.text = FormatStatText("Strength", oldStr, pd.strength);
        spdReadoutText.text = FormatStatText("Speed", oldSpd, pd.speed);
        defReadoutText.text = FormatStatText("Defense", oldDef, pd.defense);
        luckReadoutText.text = FormatStatText("Luck", oldLuck, pd.luck);

        // --- NEW: Check if they just spent their last point! ---
        if (pd.expSystem.availableSkillPoints <= 0)
        {
            StartCoroutine(AutoCloseLevelUp());
        }
    }

    // --- HELPER TO COLORIZE STAT INCREASES ---
    private string FormatStatText(string statName, int oldValue, int newValue)
    {
        int difference = newValue - oldValue;

        if (difference > 0)
        {
            // If the stat went up, wrap the difference in a GREEN Rich Text tag!
            return $"{statName}: {oldValue} -> {newValue} <color=#00FF00>(+{difference})</color>";
        }
        else
        {
            // If it didn't go up, make the (+0) a dull gray so the green really pops.
            return $"{statName}: {oldValue} -> {newValue} <color=#888888>(+0)</color>";
        }
    }

    // Connect these to the [+] buttons on your UI!
    public void AllocatePoint_HP() { SpendSkillPoint(0); }
    public void AllocatePoint_Str() { SpendSkillPoint(1); }
    public void AllocatePoint_Spd() { SpendSkillPoint(2); }
    public void AllocatePoint_Def() { SpendSkillPoint(3); }
    public void AllocatePoint_Luck() { SpendSkillPoint(4); }

    private void SpendSkillPoint(int statIndex)
    {
        if (GameManager.Instance == null || GameManager.Instance.playerData.expSystem.availableSkillPoints <= 0) return;

        var pd = GameManager.Instance.playerData;
        pd.expSystem.availableSkillPoints--;

        // Add to the chosen stat!
        if (statIndex == 0) pd.maxHP += 5; // HP gets 5 per point to keep it balanced
        else if (statIndex == 1) pd.strength += 1;
        else if (statIndex == 2) pd.speed += 1;
        else if (statIndex == 3) pd.defense += 1;
        else if (statIndex == 4) pd.luck += 1;

        if (warningText != null) warningText.gameObject.SetActive(false);

        // Update the screen so they see the number go up!
        UpdateLevelUpUI();
    }


    private void ReturnToOverworld()
    {
        if (GameManager.Instance != null)
        {
            // --- THE FLAG ---
            // Tell the next scene that we didn't just boot up the game; we are coming back from a fight!
            GameManager.isReturningFromCombat = true;

            // Load the correct floor
            if (!string.IsNullOrEmpty(GameManager.Instance.playerData.floorName))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(GameManager.Instance.playerData.floorName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("firstFloor"); // Fallback
            }
        }
        else
        {
            // Ultimate fallback for testing
            UnityEngine.SceneManagement.SceneManager.LoadScene("firstFloor"); 
        }
    }

    private void CheckWinConditionOrContinue()
    {
        if (enemyHealth <= 0)
        {
            Debug.Log("Enemy defeated!");

            PersistStatsToPlayerData();

            if (GameManager.Instance != null)
            {
                if (!GameManager.Instance.playerData.defeatedEnemies.Contains(GameManager.currentEnemyID))
                {
                    GameManager.Instance.playerData.defeatedEnemies.Add(GameManager.currentEnemyID);
                    Debug.Log(GameManager.currentEnemyID + " added to the graveyard.");
                }

                SceneManager.LoadScene(GameManager.Instance.playerData.floorName);
            }
        }
        else
        {
            EnemyTurn();
        }
    }

    private void PersistStatsToPlayerData()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerData != null)
        {
            GameManager.Instance.playerData.currentHP = playerHealth;
            // Bloom no longer needs to be saved here, because ActiveBloom handles it live!
        }
    }

    public float GetBloomStatMultiplier()
    {
        if (ActiveBloomState >= BloomState.High)
        {
            return 1.10f; // 10% Increase
        }
        return 1.0f; // Normal stats
    }

    private void EnemyTurn()
    {
        if (enemyIsStunned)
        {
            enemyIsStunned = false;
            ShowBattleText(currentEnemy.enemyName + " is stunned and can't move!", 2f);
            StartCoroutine(WaitAndPassTurn(2f));
            return;
        }

        var skill = PickSkill();
        
        ShowBattleText(currentEnemy.enemyName + " lunges at you!", 1.5f);

        StartCoroutine(PerformMeleeAttack(enemyTransform, playerTransform,
            onHit: () =>
            {
                if (skill != null)
                {
                    ShowBattleText(currentEnemy.enemyName + " uses a special attack!", 2f);
                    skill.Execute(this, currentEnemy);
                    StartCoroutine(ShakeSprite(playerTransform, 0.3f, 0.2f));
                }
                else
                {
                    float statMultiplier = GetBloomStatMultiplier();
                    int effectiveDefense = Mathf.RoundToInt(playerDefense * statMultiplier);
                    var actualDamage = Mathf.Max(1, currentEnemy.strength - effectiveDefense);
                    
                    playerHealth -= actualDamage;
                    UpdateHealthUI();
                    
                    ShowBattleText(currentEnemy.enemyName + " hits you for " + actualDamage + " damage!", 2f);
                    StartCoroutine(ShakeSprite(playerTransform, 0.3f, 0.2f));
                }
            },
            onComplete: () =>
            {
                StartCoroutine(WaitAndPassTurn(1.0f));
            }));
    }

    private IEnumerator WaitAndPassTurn(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        if (playerHealth <= 0)
        {
            ShowBattleText("The host has fallen... Game Over.", 3f);
            yield return new WaitForSeconds(3f); // Give them time to read it!
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
        else
        {
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

    public void EndEnemyTurn() { }

    // We add the "snapInstantly" toggle to the parenthesis!
    private void UpdateHealthUI(bool snapInstantly = false)
    {
        // 1. Text Updates
        if (playerHP != null) 
        {
            playerHP.text = "HP: " + playerHealth + " / " + playerMaxHealth;
        }

        // --- THE FIX: Use the dynamic enemy name! ---
        if (enemyHP != null && currentEnemy != null) 
        {
            enemyHP.text = currentEnemy.enemyName + " HP: " + enemyHealth + " / " + currentEnemy.maxHP;
        }

        int currentMaxBloom = (GameManager.Instance != null && GameManager.Instance.playerData != null) 
            ? GameManager.Instance.playerData.maxBloom 
            : 100;

        // 2. Update the Text
        if (bloomText != null) 
        {
            bloomText.text = "Bloom: " + ActiveBloom + " / " + currentMaxBloom;
        }

        // --- FORCE MAXIMUMS ---
        if (playerHpSlider != null) playerHpSlider.maxValue = playerMaxHealth;
        if (enemyHpSlider != null) enemyHpSlider.maxValue = currentEnemy.maxHP;
        
        // Changed this from 100 to currentMaxBloom to keep it fully dynamic!
        if (bloomSlider != null) bloomSlider.maxValue = currentMaxBloom;
        if (bloomPreviewSlider != null) bloomPreviewSlider.maxValue = currentMaxBloom; 

        // 3. APPLY CURRENT VALUES (Snap or Smooth)
        if (snapInstantly)
        {
            if (playerHpSlider != null) playerHpSlider.value = playerHealth;
            if (enemyHpSlider != null) enemyHpSlider.value = enemyHealth;
            if (bloomSlider != null) bloomSlider.value = ActiveBloom;
        }
        else
        {
            // Smoothly animate the sliders using the Coroutines!
            if (playerHpSlider != null) 
            { 
                if (playerHpAnim != null) StopCoroutine(playerHpAnim); 
                playerHpAnim = StartCoroutine(SmoothSliderFill(playerHpSlider, playerHealth)); 
            }
            if (enemyHpSlider != null) 
            { 
                if (enemyHpAnim != null) StopCoroutine(enemyHpAnim); 
                enemyHpAnim = StartCoroutine(SmoothSliderFill(enemyHpSlider, enemyHealth)); 
            }
            if (bloomSlider != null) 
            { 
                if (bloomAnim != null) StopCoroutine(bloomAnim); 
                bloomAnim = StartCoroutine(SmoothSliderFill(bloomSlider, ActiveBloom)); 
            }
        }

        // The preview slider MUST always snap instantly, even during an animation, so it stays accurate!
        if (bloomPreviewSlider != null) bloomPreviewSlider.value = ActiveBloom;

        // 4. COLOR SHIFT
        if (bloomFillImage != null)
        {
            if (ActiveBloomState == BloomState.Stable) bloomFillImage.color = stableBloomColor;
            else if (ActiveBloomState == BloomState.Low) bloomFillImage.color = lowBloomColor;
            else if (ActiveBloomState == BloomState.Medium) bloomFillImage.color = mediumBloomColor;
            else if (ActiveBloomState >= BloomState.High) bloomFillImage.color = highBloomColor; // Catches High and Total!
        }
        
        // DEBUG: Updated to show target variables instead of slider values so the log is accurate mid-animation!
        Debug.Log($"UI UPDATED: Player({playerHealth}/{playerMaxHealth}), Enemy({enemyHealth}/{currentEnemy.maxHP}), Bloom({ActiveBloom}/{currentMaxBloom})");
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

    private IEnumerator PerformItemAnimation(Transform actor, Action onComplete)
    {
        var startPos = actor.position;
        var peakPos = startPos + Vector3.up * 1.5f; 

        while (Vector3.Distance(actor.position, peakPos) > 0.05f)
        {
            actor.position = Vector3.MoveTowards(actor.position, peakPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

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
        var targetPos = Vector3.Lerp(startPos, target.position, 0.6f); 
        
        var midPoint = Vector3.Lerp(startPos, targetPos, 0.5f);
        var peakPos = midPoint + Vector3.up * 2f; 

        while (Vector3.Distance(attacker.position, peakPos) > 0.05f)
        {
            attacker.position = Vector3.MoveTowards(attacker.position, peakPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        while (Vector3.Distance(attacker.position, targetPos) > 0.05f)
        {
            attacker.position = Vector3.MoveTowards(attacker.position, targetPos, moveSpeed * 1.5f * Time.deltaTime); 
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

    private IEnumerator PerformRunAnimation(Transform actor, Action onComplete)
    {
        var startPos = actor.position;
        var offscreenPos = startPos + (Vector3.left * 10f); 

        while (Vector3.Distance(actor.position, offscreenPos) > 0.05f)
        {
            actor.position = Vector3.MoveTowards(actor.position, offscreenPos, moveSpeed * 1.5f * Time.deltaTime);
            yield return null;
        }

        onComplete?.Invoke();
    }

    private IEnumerator ShakeSprite(Transform targetTransform, float duration, float magnitude)
    {
        Vector3 originalPos = targetTransform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = originalPos.x + UnityEngine.Random.Range(-1f, 1f) * magnitude;
            float y = originalPos.y + UnityEngine.Random.Range(-1f, 1f) * magnitude;

            targetTransform.localPosition = new Vector3(x, y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null; 
        }

        targetTransform.localPosition = originalPos;
    }

    private System.Collections.IEnumerator SmoothSliderFill(Slider slider, float targetValue, float duration = 0.3f)
    {
        if (slider == null) yield break;

        float startValue = slider.value;
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            // Smoothly transition from the current fill to the new target
            slider.value = Mathf.Lerp(startValue, targetValue, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Snap exactly to the target at the very end to prevent weird decimals
        slider.value = targetValue;
    }

    

    // --- MENU NAVIGATION ---

    public void OpenSkillMenu()
    {
        if (!isPlayerTurn) return;
        mainMenuPanel.SetActive(false);
        skillMenuPanel.SetActive(true);

        var cores = GameManager.Instance != null ? GetEquippedCores() : testCores;
        var firstButton = cores.Count > 0 ? skillButtonContainer.GetChild(0).gameObject : skillDefaultButton;
        StartCoroutine(HighlightButtonSafe(firstButton));
    }

    public void OpenItemMenu()
    {
        if (!isPlayerTurn) return;
        UpdateItemUI(); 
        
        mainMenuPanel.SetActive(false);
        itemMenuPanel.SetActive(true);

        StartCoroutine(HighlightButtonSafe(itemDefaultButton));
    }

    public void BackToMainMenu()
    {
        if (cachedEventSystem != null)
        {
            cachedEventSystem.enabled = true; 
        }

        skillMenuPanel.SetActive(false);
        itemMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);

        StartCoroutine(HighlightButtonSafe(mainDefaultButton));
    }

    private void HideAllMenus()
    {
        if (cachedEventSystem != null)
        {
            cachedEventSystem.enabled = false;
        }

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (skillMenuPanel != null) skillMenuPanel.SetActive(false);
        if (itemMenuPanel != null) itemMenuPanel.SetActive(false);
    }

    private IEnumerator HighlightButtonSafe(GameObject buttonToHighlight)
    {
        EventSystem.current.SetSelectedGameObject(null); 
        yield return null; 
        EventSystem.current.SetSelectedGameObject(buttonToHighlight); 
    }

    private void UpdateItemUI()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerData != null)
        {
            var potions = GameManager.Instance.playerData.healthPotions;
            if (potions > 0)
            {
                healItemText.text = "Health Potion (x" + potions + ")";
                healItemButton.interactable = true;
            }
            else
            {
                healItemText.text = "Out of Potions!";
                healItemButton.interactable = false;
            }
        }
        else
        {
            healItemText.text = "Health Potion";
            healItemButton.interactable = true;
        }
    }
    
    private void Update()
    {
        if (Time.timeScale <= 0) return;
        if (isPlayerTurn && (Input.GetKeyDown(KeyCode.P)) || (Input.GetKeyDown(KeyCode.X)))
        {
            if (skillMenuPanel.activeSelf || itemMenuPanel.activeSelf)
            {
                BackToMainMenu();
            }
        }
    }

    // Call this when the mouse hovers OVER the button
    public void PreviewBloomCost(int cost)
    {
        if (bloomPreviewSlider != null)
        {
            int currentMaxBloom = (GameManager.Instance != null && GameManager.Instance.playerData != null) 
                ? GameManager.Instance.playerData.maxBloom 
                : 100;

            // Calculate the projected total, clamped to the max
            int projectedBloom = Mathf.Min(ActiveBloom + cost, currentMaxBloom);
            
            // Push the yellow ghost bar forward!
            bloomPreviewSlider.value = projectedBloom; 
        }
    }

    // Call this when the mouse leaves the button
    public void ClearBloomPreview()
    {
        if (bloomPreviewSlider != null)
        {
            // Snap the yellow bar back to hide perfectly behind the current bloom
            bloomPreviewSlider.value = ActiveBloom; 
        }
    }

    // Turns the box on and sets the text
    public void ShowTooltip(string text)
    {
        // If battle text is playing, ignore the mouse!
        if (battleTextCoroutine != null) return; 

        if (tooltipPanel != null) tooltipPanel.SetActive(true);
        if (tooltipText != null) tooltipText.text = text;
    }

    // Hides the box
    public void HideTooltip()
    {
        // If battle text is playing, don't let the mouse accidentally turn it off!
        if (battleTextCoroutine != null) return;

        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    // Calculates the dynamic math for the Attack Button!
    public string GetBasicAttackTooltip()
    {
        // Calculate the exact damage they will do right now
        int expectedDamage = Mathf.Max(1, playerStrength - currentEnemy.defense);
        return $"Basic Attack\nDeals physical damage to the enemy.\nExpected Damage: {expectedDamage}";
    }

    // Call this exactly like a Debug.Log! 
    // Example: ShowBattleText("The Stag attacks for 15 damage!", 2f);
    public void ShowBattleText(string message, float displayTime = 1.5f)
    {
        if (battleTextCoroutine != null) StopCoroutine(battleTextCoroutine);
        battleTextCoroutine = StartCoroutine(BattleTextRoutine(message, displayTime));
    }

    private System.Collections.IEnumerator BattleTextRoutine(string message, float displayTime)
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(true);
        if (tooltipText != null) tooltipText.text = message;

        yield return new WaitForSeconds(displayTime);

        if (tooltipPanel != null) tooltipPanel.SetActive(false);
        
        // --- THE CRITICAL FIX ---
        // Tell the game the battle text is officially done so mouse hovers work again!
        battleTextCoroutine = null; 
    }

    private IEnumerator FlashVignetteRoutine(Color flashColor)
    {
        if (vignetteOverlay == null) yield break;

        // 1. THE IMPACT (Instant Snap)
        // Bam! Instantly set it to maximum opacity on the exact frame the hit connects.
        flashColor.a = vignetteMaxAlpha; 
        vignetteOverlay.color = flashColor;

        // Hold the pure flash for a tiny fraction of a second (The literal "Impact Frame")
        yield return new WaitForSeconds(0.05f);

        float elapsed = 0f;

        // 2. THE SHARP FADE OUT
        // We now use the entire duration purely for the fade-out so it tails off nicely.
        while (elapsed < vignetteFlashDuration)
        {
            elapsed += Time.deltaTime;
            
            // Lerp from max opacity down to 0
            flashColor.a = Mathf.Lerp(vignetteMaxAlpha, 0f, elapsed / vignetteFlashDuration);
            vignetteOverlay.color = flashColor;
            yield return null;
        }

        // 3. Ensure it is completely invisible when done
        flashColor.a = 0f;
        vignetteOverlay.color = flashColor;
    }

    // CORE API
    // Methods called by cores/skills
    private List<CoreTemplate> GetEquippedCores()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerData != null)
            return GameManager.Instance.playerData.equippedCores;
        return new List<CoreTemplate>();
    }

    public void RunCoroutine(IEnumerator routine)
    {
        StartCoroutine(routine);
    }

    public void DealDamageToPlayer(int amount, bool ignoreDefense)
    {
        int actual = ignoreDefense ? amount : Mathf.Max(1, amount - playerDefense);
        if (guardTurnsRemaining > 0)
            actual = Mathf.RoundToInt(actual * (1f - guardDamageReduction));
        playerHealth -= actual;
        UpdateHealthUI();
    }

    public void DealDamageToEnemy(int amount)
    {
        enemyHealth -= amount;
        enemyHealth = Mathf.Max(0, enemyHealth);
        UpdateHealthUI();
    }

    public void AddBloom(int amount)
    {
        int currentMaxBloom = (GameManager.Instance != null && GameManager.Instance.playerData != null)
            ? GameManager.Instance.playerData.maxBloom : 100;
        ActiveBloom = Mathf.Min(ActiveBloom + amount, currentMaxBloom);
        UpdateHealthUI();
    }

    public void TriggerSkillAnimation(Action onHit, Action onComplete)
    {
        StartCoroutine(PerformSkillAnimation(playerTransform, enemyTransform, onHit, onComplete));
    }

    public void ApplyGuard(float reduction, int turns)
    {
        guardDamageReduction = reduction;
        guardTurnsRemaining = turns;
    }

    public void ApplyStun()
    {
        enemyIsStunned = true;
        ShowBattleText("The enemy is stunned and will skip their turn!", 2.5f);
    }

    public void TriggerItemAnimation(Action onComplete)
    {
        StartCoroutine(PerformItemAnimation(playerTransform, onComplete));
    }

    public void TriggerShake(bool shakeEnemy, float duration, float magnitude)
    {
        StartCoroutine(ShakeSprite(shakeEnemy ? enemyTransform : playerTransform, duration, magnitude));
    }

    public void TriggerVignette(VignetteType type)
    {
        Color c = type == VignetteType.Low ? lowBloomColor
                : type == VignetteType.Medium ? mediumBloomColor
                : highBloomColor;
        StartCoroutine(FlashVignetteRoutine(c));
    }

    public void OnCoreComplete()
    {
        if (enemyHealth <= 0) HandleEnemyDefeat();
        else CheckWinConditionOrContinue();
    }

    public void GivePlayerAnotherTurn()
    {
        PlayerStartTurn();
    }

    public void TriggerGameOver()
    {
        Invoke(nameof(LoadMainMenu), 3f);
    }

    public bool IsPlayerDefeated() => playerHealth <= 0;
    public bool IsEnemyDefeated() => enemyHealth <= 0;
    public BloomState GetBloomState() => ActiveBloomState;
    public float GetBloomMultiplier() => GetBloomStatMultiplier();
    public int GetPlayerStrength() => playerStrength;

    [Header("Skill Menu")]
    public GameObject skillButtonPrefab;
    public Transform skillButtonContainer;

    private void PopulateSkillMenu()
    {
        foreach (Transform child in skillButtonContainer)
        {
            if (child.gameObject != skillDefaultButton)
                Destroy(child.gameObject);
        }

        var cores = GameManager.Instance != null ? GetEquippedCores() : testCores;

        for (int i = 0; i < 5; i++)
        {
            var obj = Instantiate(skillButtonPrefab, skillButtonContainer);
            var trigger = obj.GetComponent<BloomPreviewTrigger>();
            var button = obj.GetComponent<UnityEngine.UI.Button>();

            if (i < cores.Count && cores[i] != null)
            {
                trigger.Setup(cores[i], this);
            }
            else
            {
                var emptyTrigger = obj.GetComponent<BloomPreviewTrigger>();
                emptyTrigger.SetupEmpty();
            }
        }

        skillDefaultButton.transform.SetAsLastSibling();
    }
}