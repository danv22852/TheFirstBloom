using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;

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
    public int playerLuck;

    [Header("Critical Hits")]
    [Tooltip("How much damage a crit multiplies. 1.5x is standard.")]
    public float critMultiplier = 1.5f;

    [Header("Bloom Hijack UI")]
    public TextMeshProUGUI attackButtonText; 
    private bool isAttackHijacked = false;   

    // --- ENEMY STATS ---
    [Header("Enemy Stats")]
    public EnemyData currentEnemy;
    private int enemyHealth;
    private int enemySpeed; 
    private Vector3 baseEnemyPosition;

    // --- COMBAT STATE ---
    private bool isPlayerTurn = false;
    private bool hasUsedItemThisTurn = false;
    private bool isAutoBattlerExecutingCore = false;
    private int guardTurnsRemaining = 0; // Tracks how many turns of guard are left

    private float guardDamageReduction = 0f; // 50% damage reduction when guarding

    private bool enemyIsStunned = false; // New flag to track if the enemy is stunned



    // --- CORE FIELDS ---
    private float revitalizeHealPercent = 0f;
    private int revitalizeTurnsRemaining = 0;
    private bool isChargedUp = false;
    private float weakenMultiplier = 1f;

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
    public UnityEngine.UI.Button wiltItemButton; 
    public TMPro.TextMeshProUGUI wiltItemText;

    [Header("Animation Settings")]
    public Transform playerTransform;
    public Transform enemyTransform;
    public float moveSpeed = 15f;

    [Header("Player Stats (Testing Fallbacks)")]
    public int testPlayerMaxHealth = 100;
    public int testPlayerStrength = 10;
    public int testPlayerSpeed = 10;
    public int testPlayerDefense = 10;
    public int testPlayerLuck = 10;
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

    [Header("QTE Manager")]
    public QTEManager qteManager;
    
    // Text to show "Strength: 10 -> 11 (+1)"
    public TextMeshProUGUI hpReadoutText;
    public TextMeshProUGUI strReadoutText;
    public TextMeshProUGUI spdReadoutText;
    public TextMeshProUGUI defReadoutText;
    public TextMeshProUGUI luckReadoutText;

    // Snapshot variables to remember what the stats used to be!
    private int oldMaxHP, oldStr, oldSpd, oldDef, oldLuck;

    [Header("Troll Boss Mechanics")]
    public bool isBossFight = false;
    private int bossTurnCounter = 0; // Tracks the 3-turn cycle
    private int bossPhase = 1; // 1 = Guarding, 2 = Frenzy
    public bool isBossGuarding = false;
    public SkillBase trollCaveInSkill;
    public SkillBase trollSeismicSlamSkill;

    [Header("Boss Rewards")]
    public CoreTemplate bossCoreReward2F; // Drag the Seismic Slam core here in the Inspector!

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
        if (enemyTransform != null)
        {
            baseEnemyPosition = enemyTransform.position;
        }
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
            playerLuck = pd.luck;
        }
        else
        {
            Debug.Log("<color=cyan>TESTING MODE:</color> No GameManager found. Using Inspector Fallback Stats!");
            playerMaxHealth = testPlayerMaxHealth;
            playerHealth = testPlayerMaxHealth;
            playerStrength = testPlayerStrength;
            playerSpeed = testPlayerSpeed;
            playerDefense = testPlayerDefense;
            playerLuck = testPlayerLuck;
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

        // --- 4. SET ENEMY VISUALS AND STATS ---
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
                enemyTransform.position = baseEnemyPosition + new Vector3(0, currentEnemy.hoverHeight, 0);
                Debug.Log("<color=cyan>3. Sprite assigned successfully!</color>");
            }
        }

        enemyHealth = currentEnemy.maxHP;
        enemySpeed = currentEnemy.speed;

        // --- NEW: AUTO-DETECT THE BOSS! ---
        if (currentEnemy.enemyID == "TrollBoss" || currentEnemy.enemyName == "Underground Troll")
        {
            isBossFight = true;
            bossPhase = 1;      // Reset phase
            bossTurnCounter = 0; // Reset turn counter
            Debug.Log("<color=red>BOSS FIGHT DETECTED! Activating Troll AI.</color>");
        }
        else
        {
            isBossFight = false;
        }

        // Set the maximums for our sliders!
        if (playerHpSlider != null) playerHpSlider.maxValue = playerMaxHealth;
        if (enemyHpSlider != null) enemyHpSlider.maxValue = currentEnemy.maxHP;
        if (bloomSlider != null) bloomSlider.maxValue = 100;

        lastKnownBloomState = ActiveBloomState;

        // --- 5. START THE ROUND ---
        UpdateHealthUI(true);
        // DetermineFirstTurn();
        PopulateSkillMenu();

        yield return StartCoroutine(CombatIntroSequence());
    }


    private void DetermineFirstTurn()
    {
        if (playerSpeed >= enemySpeed)
        {
            Debug.Log("Player goes first.");
            // ShowBattleText("Speed Advantage! You strike first!", 2f);
            PlayerStartTurn();
        }
        else
        {
            Debug.Log("Enemy goes first.");
            // ShowBattleText(currentEnemy.enemyName + " is faster! They attack first!", 2f);
            EnemyTurn();
        }
    }

    public void PlayerStartTurn()
    {
        TickPlayerStatusEffects();
        if (playerHealth <= 0) return;

        isPlayerTurn = true;
        hasUsedItemThisTurn = false;
        
        isAttackHijacked = false;
        attackButtonText.text = "Attack"; 
        attackButtonText.color = Color.black;

        if (ActiveBloomState == BloomState.Total)
        {
            StartCoroutine(SymbioteAutoBattleRoutine());
            return; // Stops the rest of PlayerTurn from running!
        } 

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

        if (revitalizeTurnsRemaining > 0)
        {
            int healAmount = Mathf.RoundToInt(playerMaxHealth * revitalizeHealPercent);
            playerHealth = Mathf.Min(playerHealth + healAmount, playerMaxHealth);
            UpdateHealthUI();
            revitalizeTurnsRemaining--;
            
            if (revitalizeTurnsRemaining > 0)
                ShowBattleText("Revitalize heals " + healAmount + " HP! (" + revitalizeTurnsRemaining + " turns left)", 2f);
            else
                ShowBattleText("Revitalize has worn off.", 2f);
        }

        BackToMainMenu();
    }

    private System.Collections.IEnumerator CombatIntroSequence()
    {
        // 1. Lock the UI
        if (cachedEventSystem != null) cachedEventSystem.enabled = false;

        // --- NEW: 2. Check how the battle started! ---
        if (GameManager.playerFirstStrike)
        {
            // Player hit the enemy in the overworld!
            isPlayerTurn = true;
            ShowBattleText("Ambush! You strike first!", 2f);
            
            // CRITICAL: Reset the flag immediately so the NEXT battle 
            // doesn't accidentally think it's a first strike too!
            GameManager.playerFirstStrike = false; 
        }
        else
        {
            // The enemy touched the player normally. Rely on the Speed Stat!
            if (playerSpeed >= enemySpeed)
            {
                isPlayerTurn = true; 
                ShowBattleText("Speed Advantage! You strike first!", 2f);
            }
            else
            {
                isPlayerTurn = false;
                ShowBattleText(currentEnemy.enemyName + " is faster! They attack first!", 2f);
            }
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
            Debug.Log("Player goes first.");
            PlayerStartTurn();
        }
        else
        {
            // Call your Enemy's attack
            Debug.Log("Enemy goes first.");
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
                int effectiveDefense = currentEnemy.defense;

                if (isBossFight && isBossGuarding)
                {
                    effectiveDefense *= 5; // The Troll's defense becomes a brick wall!
                    ShowBattleText("The Troll's rocky skin deflects the blow!", 1.5f);
                }

                int rawDamage = Mathf.Max(1, playerStrength - effectiveDefense);
                float varianceMultiplier = UnityEngine.Random.Range(0.85f, 1.15f);
                int finalDamage = Mathf.RoundToInt(rawDamage * varianceMultiplier);

                // --- NEW: CRITICAL HIT CHECK ---
                bool isCrit = CheckForCriticalHit();
                if (isCrit)
                {
                    finalDamage = Mathf.RoundToInt(finalDamage * critMultiplier);
                    
                    // Flash the screen white for extra impact!
                    StartCoroutine(FlashVignetteRoutine(Color.white)); 
                }

                finalDamage = Mathf.Max(1, finalDamage);
                enemyHealth -= finalDamage;
                enemyHealth = Mathf.Max(0, enemyHealth);

                UpdateHealthUI();

                // --- NEW: DYNAMIC BATTLE TEXT ---
                if (isCrit)
                {
                    ShowBattleText("CRITICAL HIT! Dealt " + finalDamage + " damage!", 2.5f);
                }
                else
                {
                    ShowBattleText("Dealt " + finalDamage + " damage to the enemy.", 2f);
                }

                StartCoroutine(ShakeSprite(enemyTransform, 0.2f, 0.15f));
            },
            onComplete: () =>
            {
                if (enemyHealth <= 0) HandleEnemyDefeat();
                else CheckWinConditionOrContinue();
            }));
    }

    public bool CheckForCriticalHit()
    {
        // Random.Range(0, 100) rolls a number from 0 to 99.
        // If Luck is 5, it returns true if it rolls 0, 1, 2, 3, or 4 (exactly 5% chance!)
        int roll = UnityEngine.Random.Range(0, 100);
        return roll < playerLuck;
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

    public void UseWiltPotion()
    {
        if (hasUsedItemThisTurn)
        {
            ShowBattleText("You already used an item this turn.", 1.5f);
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.playerData.wiltPotions <= 0)
        {
            ShowBattleText("No Wilt Potions left.", 1.5f);
            return;
        }
        else if (GameManager.Instance != null)
        {
            GameManager.Instance.playerData.wiltPotions--;
        }

        isPlayerTurn = false; 
        HideAllMenus(); 
        
        ShowBattleText("You drink a Wilt Potion...", 1.5f);

        StartCoroutine(PerformItemAnimation(playerTransform, 
            onComplete: () => 
            {
                // Subtract 25 Bloom! (Mathf.Max ensures it doesn't drop below 0)
                ActiveBloom = Mathf.Max(0, ActiveBloom - 25);

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.playerData.SetDecayFloor();
                }
                
                hasUsedItemThisTurn = true;
                
                // Force the UI bars to update instantly
                UpdateHealthUI(true);
                
                ShowBattleText("The Symbiote grip weakens. Bloom reduced by 25!", 2.5f);
                
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
        // --- 1. INITIAL DEFEAT & REWARDS ---
        if (isBossFight && currentEnemy.enemyID == "TrollBoss")
        {
            if (GameManager.Instance != null && !GameManager.Instance.playerData.defeatedEnemies.Contains("TrollBoss"))
            {
                GameManager.Instance.playerData.defeatedEnemies.Add("TrollBoss");
                Debug.Log("<color=green>Troll Boss permanently added to Graveyard!</color>");
                
                if (bossCoreReward2F != null)
                {
                    var pData = GameManager.Instance.playerData;
                    if (pData.equippedCores.Count < pData.maxCoreSlots) pData.equippedCores.Add(bossCoreReward2F);
                    else pData.knownCoreIDs.Add(bossCoreReward2F.name);
                }
            }

            ShowBattleText("The Symbiote devours the Troll's remains...\nAbsorbed Core: SEISMIC SLAM!", 3.5f);
            
            // THE FIX: Use Invoke to securely jump to the EXP function!
            Invoke(nameof(ProcessEXPAndVictory), 3.5f);
        }
        else
        {
            ShowBattleText("You defeated the " + currentEnemy.enemyName + "!", 2f);
            
            // THE FIX: Use Invoke to securely jump to the EXP function!
            Invoke(nameof(ProcessEXPAndVictory), 2.0f);
        }
    }

    // --- 2. EXP & SCENE LOAD (Safe from the Text Box wiping it out!) ---
    private void ProcessEXPAndVictory()
    {
        int expGained = currentEnemy != null ? currentEnemy.expDrop : 0;
        bool leveledUp = false;

        if (GameManager.Instance != null && GameManager.Instance.playerData != null)
        {
            var pd = GameManager.Instance.playerData;
            
            oldMaxHP = pd.maxHP;
            oldStr = pd.strength;
            oldSpd = pd.speed;
            oldDef = pd.defense;
            oldLuck = pd.luck;

            leveledUp = pd.expSystem.AddEXP(expGained, pd);
        }

        // Log the enemy in the graveyard (checks Instance ID first, then falls back to regular ID)
        if (GameManager.Instance != null)
        {
            var targetID = !string.IsNullOrEmpty(GameManager.encounteredInstanceID) ? GameManager.encounteredInstanceID : GameManager.currentEnemyID;
            
            if (!string.IsNullOrEmpty(targetID) && !GameManager.Instance.playerData.defeatedEnemies.Contains(targetID))
            {
                GameManager.Instance.playerData.defeatedEnemies.Add(targetID);
            }
            GameManager.encounteredInstanceID = ""; 
        }

        PersistStatsToPlayerData(); 

        // Chain the final scene transition!
        if (leveledUp)
        {
            ShowBattleText($"Victory!\nGained {expGained} EXP. LEVEL UP!", 2.5f);
            Invoke(nameof(OpenLevelUpMenu), 2.5f);
        }
        else
        {
            ShowBattleText($"Victory! {currentEnemy.enemyName} defeated.\nGained {expGained} EXP.", 2.5f);
            Invoke(nameof(ReturnToOverworld), 2.5f);
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

        if (GameManager.Instance != null && GameManager.Instance.playerData != null)
        {
            // Grab the absolute latest Max HP (in case they just spent a point on the HP stat)
            playerMaxHealth = GameManager.Instance.playerData.maxHP; 
            
            // Fully heal the player to that new maximum!
            playerHealth = playerMaxHealth; 
        }

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

            GameManager.Instance.playerData.SetDecayFloor();

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
            // Call it normally. It is no longer a Coroutine!
            HandleEnemyDefeat(); 
        }
        else
        {
            // Boss Phase 2 Transition Logic
            if (isBossFight && bossPhase == 1 && enemyHealth <= (currentEnemy.maxHP / 2))
            {
                bossPhase = 2;
                isBossGuarding = false; 

                ShowBattleText("The Troll roars! The deafening sound enrages your Symbiote! (+25 Bloom)", 3.0f);

                int panicHeal = Mathf.RoundToInt(playerMaxHealth * 0.30f);
                playerHealth = Mathf.Min(playerMaxHealth, playerHealth + panicHeal);
                UpdateHealthUI();
                AddBloom(25);
                StartCoroutine(ShakeSprite(playerTransform, 0.5f, 0.4f));
            }

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
        if (isBossFight)
        {
            ExecuteTrollBossAI();
            return;
        }

        if (enemyHealth <= 0) return;

        // Apply status effects first
        TickStatusEffects();

        if (chilledStacks > 0)
        {
            chilledStacks--;
            if (chilledStacks > 0)
                ShowBattleText(currentEnemy.enemyName + " is thawing... (" + chilledStacks + " chill stacks remaining)", 1.5f);
            else
                ShowBattleText(currentEnemy.enemyName + " has thawed out.", 1.5f);
        }

        // If stunned, skip rest of turn
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

    private void ExecuteTrollBossAI()
    {
        TickStatusEffects();
        if (enemyHealth <= 0) return;

        if (chilledStacks > 0)
        {
            chilledStacks--;
            if (chilledStacks > 0)
                ShowBattleText(currentEnemy.enemyName + " is thawing... (" + chilledStacks + " stacks remaining)", 1.5f);
            else
                ShowBattleText(currentEnemy.enemyName + " has thawed out.", 1.5f);
        }

        if (enemyIsStunned)
        {
            enemyIsStunned = false;
            ShowBattleText(currentEnemy.enemyName + " is stunned and can't move!", 2f);
            StartCoroutine(WaitAndPassTurn(2f));
            return;
        }


        // PHASE 2: SEISMIC SLAM FRENZY
        if (bossPhase == 2)
        {
            isBossGuarding = false;
            
            ShowBattleText("The Troll uses SEISMIC SLAM!", 1.5f);
            StartCoroutine(PerformMeleeAttack(enemyTransform, playerTransform,
                onHit: () => 
                { 
                    if (trollSeismicSlamSkill != null) trollSeismicSlamSkill.Execute(this, currentEnemy);
                },
                onComplete: () => { StartCoroutine(WaitAndPassTurn(1.0f)); }));
            return;
        }

        // PHASE 1: STONE SKIN CYCLE
        bossTurnCounter++;

        if (bossTurnCounter == 1)
        {
            // Turn 1: Guard!
            isBossGuarding = true;
            ShowBattleText("The Troll uses STONE SKIN! Its defense skyrockets.", 2f);
            StartCoroutine(WaitAndPassTurn(2.0f));
        }
        else if (bossTurnCounter == 2)
        {
            // Turn 2: Still Guarding, charging up!
            isBossGuarding = true;
            ShowBattleText("The Troll's Stone Skin holds strong as it raises its club...", 2f);
            StartCoroutine(WaitAndPassTurn(2.0f));
        }
        else if (bossTurnCounter >= 3)
        {
            // Turn 3: SMASH!
            isBossGuarding = false;
            bossTurnCounter = 0; // Reset the cycle
            
            ShowBattleText("CAVE-IN! The Troll brings the club down!", 2f);
            StartCoroutine(PerformMeleeAttack(enemyTransform, playerTransform,
                onHit: () => 
                { 
                    if (trollCaveInSkill != null) trollCaveInSkill.Execute(this, currentEnemy);
                },
                onComplete: () => { StartCoroutine(WaitAndPassTurn(1.0f)); }));
        }
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
            var pd = GameManager.Instance.playerData;
            bool isSymbioteInControl = (ActiveBloomState == BloomState.Total);

            // 1. Health Potions
            if (healItemText != null && healItemButton != null)
            {
                if (isSymbioteInControl)
                {
                    healItemText.text = "BLOCKED BY SYMBIOTE";
                    healItemButton.interactable = false;
                }
                else if (pd.healthPotions > 0)
                {
                    healItemText.text = "Health Potion (x" + pd.healthPotions + ")";
                    healItemButton.interactable = true;
                }
                else
                {
                    healItemText.text = "Out of Potions!";
                    healItemButton.interactable = false;
                }
            }

            // 2. Wilt Potions
            if (wiltItemText != null && wiltItemButton != null)
            {
                // --- THE DISCOVERY CHECK ---
                if (pd.hasDiscoveredWiltPotions)
                {
                    // 1. Ensure the slot is turned ON because they found the item!
                    wiltItemButton.gameObject.SetActive(true);

                    // 2. Your normal logic continues here...
                    if (isSymbioteInControl)
                    {
                        wiltItemText.text = "BLOCKED BY SYMBIOTE";
                        wiltItemButton.interactable = false;
                    }
                    else if (pd.wiltPotions > 0)
                    {
                        wiltItemText.text = "Wilt Potion (x" + pd.wiltPotions + ")";
                        wiltItemButton.interactable = true;
                    }
                    else
                    {
                        wiltItemText.text = "Out of Wilt Potions!";
                        wiltItemButton.interactable = false;
                    }
                }
                else
                {
                    // 3. If they have NEVER picked one up, completely hide the slot!
                    wiltItemButton.gameObject.SetActive(false);
                }
            }
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
        // 1. Calculate the raw base damage
        int rawDamage = Mathf.Max(1, playerStrength - currentEnemy.defense);

        // 2. Calculate the lowest and highest possible hits based on your 15% variance
        int minDamage = Mathf.Max(1, Mathf.RoundToInt(rawDamage * 0.85f));
        int maxDamage = Mathf.Max(1, Mathf.RoundToInt(rawDamage * 1.15f));

        // 3. Format the string cleanly
        string damageText;
        if (minDamage == maxDamage)
        {
            // If they are identical (e.g., both are 1 due to heavy armor), just show the single number
            damageText = minDamage.ToString();
        }
        else
        {
            // Otherwise, show the range!
            damageText = $"{minDamage} - {maxDamage}";
        }

        return $"Basic Attack\nDeals physical damage to the enemy.\nExpected Damage: {damageText}";
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
        if (isChargedUp)
        {
            amount *= 2;
            isChargedUp = false;
        }
        amount = Mathf.RoundToInt(amount * GetChilledDamageMultiplier());
        enemyHealth -= amount;
        enemyHealth = Mathf.Max(0, enemyHealth);
        UpdateHealthUI();
    }

    public void DealDamageToEnemyIgnoreDefense(int amount)
    {
        if (isChargedUp)
        {
            amount *= 2;
            isChargedUp = false;
        }
        enemyHealth -= amount;
        enemyHealth = Mathf.Max(0, enemyHealth);
        UpdateHealthUI();
    }

    public int GetPlayerMaxHealth() => playerMaxHealth;

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

    public void ApplyRevitalize(float percentPerTurn, int duration)
    {
        revitalizeHealPercent = percentPerTurn;
        revitalizeTurnsRemaining = duration;
    }

    public string GetEnemyName() => currentEnemy.enemyName;

    // STATUS EFFECTS
    private List<StatusEffect> enemyStatusEffects = new List<StatusEffect>();
    private List<StatusEffect> playerStatusEffects = new List<StatusEffect>();
    private int chilledStacks = 0;

    public void ApplyStatusEffect(StatusEffect effect)
    {
        if (effect.type == StatusEffectType.Poison)
        {
            var existing = enemyStatusEffects.Find(e => e.type == StatusEffectType.Poison);
            if (existing != null)
            {
                existing.damage += effect.damage;
                ShowBattleText("Poison intensifies! Now dealing " + existing.damage + " per turn.", 2f);
                return;
            }
        }
        if (effect.type == StatusEffectType.Weaken)
        {
            weakenMultiplier = effect.weakenMultiplier;
            ShowBattleText(currentEnemy.enemyName + " is weakened!", 2f);
        }
        enemyStatusEffects.Add(effect);
    }

    public void TickStatusEffects()
    {
        for (int i = enemyStatusEffects.Count - 1; i >= 0; i--)
        {
            var effect = enemyStatusEffects[i];
            enemyHealth -= effect.damage;
            UpdateHealthUI();

            if (effect.type == StatusEffectType.Burn)
            {
                effect.damage = Mathf.Max(0, effect.damage - 2); // decrement burn
                ShowBattleText(currentEnemy.enemyName + " burns for " + effect.damage + " damage!", 1.5f);
                effect.turnsRemaining--;
                if (effect.turnsRemaining <= 0 || effect.damage <= 0)
                {
                    enemyStatusEffects.RemoveAt(i);
                    ShowBattleText("The burn fades.", 1.5f);
                }
            }
            else if (effect.type == StatusEffectType.Poison)
            {
                ShowBattleText(currentEnemy.enemyName + " is poisoned for " + effect.damage + " damage!", 1.5f);
                // Poison doesn't decrement or expire naturally
            }
            else if (effect.type == StatusEffectType.Weaken)
            {
                effect.turnsRemaining--;
                if (effect.turnsRemaining <= 0)
                {
                    weakenMultiplier = 1f;
                    enemyStatusEffects.RemoveAt(i);
                    ShowBattleText(currentEnemy.enemyName + " is no longer weakened.", 2f);
                }
            }

            if (enemyHealth <= 0)
            {
                HandleEnemyDefeat();
                return;
            }
        }
    }

    public void CleanseEnemyEffects()
    {
        enemyStatusEffects.Clear();
        chilledStacks = 0;
        ShowBattleText(currentEnemy.enemyName + " shrugs off all status effects!", 2f);
    }

    public void ApplyStatusEffectToPlayer(StatusEffect effect)
    {
        playerStatusEffects.Add(effect);
    }

    public void TickPlayerStatusEffects()
    {
        for (int i = playerStatusEffects.Count - 1; i >= 0; i--)
        {
            var effect = playerStatusEffects[i];
            playerHealth -= effect.damage;
            UpdateHealthUI();

            ShowBattleText("You are affected by " + effect.type + " for " + effect.damage + " damage!", 1.5f);

            if (effect.type == StatusEffectType.Burn)
            {
                effect.damage = Mathf.Max(0, effect.damage - 2);
                effect.turnsRemaining--;
                if (effect.turnsRemaining <= 0 || effect.damage <= 0)
                {
                    playerStatusEffects.RemoveAt(i);
                    ShowBattleText("The " + effect.type + " fades.", 1.5f);
                }
            }

            if (playerHealth <= 0)
            {
                ShowBattleText("The host has fallen... Game Over.", 3f);
                Invoke(nameof(LoadMainMenu), 3f);
                return;
            }
        }
    }

    public int CleansePlayerEffects()
    {
        int count = playerStatusEffects.Count;
        playerStatusEffects.Clear();
        return count;
    }


    public void ApplyChilled(int stacks)
    {
        chilledStacks += stacks;
        ShowBattleText(currentEnemy.enemyName + " is chilled! (" + chilledStacks + " stacks)", 2f);
    }

    public int ConsumeChilled()
    {
        int stacks = chilledStacks;
        chilledStacks = 0;
        return stacks;
    }

    public int GetChilledStacks() => chilledStacks;

    public float GetChilledDamageMultiplier()
    {
        return 1f + (chilledStacks * 0.25f);
    }

    public int GetLastStandDamage(int baseDamage, int maxDamage)
    {
        float missingHPPercent = 1f - ((float)playerHealth / playerMaxHealth);
        return Mathf.RoundToInt(Mathf.Lerp(baseDamage, maxDamage, missingHPPercent));
    }

    public bool IsEnemyBelowThreshold(float threshold)
    {
        return (float)enemyHealth / currentEnemy.maxHP <= threshold;
    }

    public void ApplyChargeUp()
    {
        isChargedUp = true;
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
        // --- NEW: Intercept the end of the attack if the Symbiote is in control! ---
        if (isAutoBattlerExecutingCore)
        {
            isAutoBattlerExecutingCore = false;
            return; // Skip normal turn-passing so the Coroutine can resume!
        }

        if (enemyHealth <= 0) HandleEnemyDefeat();
        else CheckWinConditionOrContinue();
    }

    public void GivePlayerAnotherTurn()
    {
        PlayerStartTurn();
    }

    public int GetHealAmount(float percentOfMissing)
    {
        int missing = playerMaxHealth - playerHealth;
        return Mathf.RoundToInt(missing * percentOfMissing);
    }

    public void HealPlayer(int amount)
    {
        playerHealth = Mathf.Min(playerHealth + amount, playerMaxHealth);
        UpdateHealthUI();
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

    private int symbioteTurnsTaken = 0; // Tracks how long it's been in control

    private System.Collections.IEnumerator SymbioteAutoBattleRoutine()
    {
        symbioteTurnsTaken++;
        Debug.Log("Symbiote is taking Turn " + symbioteTurnsTaken);

        // 1. Hide the Combat Menu and show a warning
        HideAllMenus();
        ShowBattleText("<color=red>THE SYMBIOTE IS IN CONTROL.</color>", 1.5f);
        yield return new WaitForSeconds(1.5f);

        ShowBattleText("The Symbiote lashes out violently!", 1.0f);

        // --- THE POWERSPIKE ---
        // Save the real strength, then buff it by 35% for the attack!
        int originalStrength = playerStrength;
        playerStrength = Mathf.RoundToInt(playerStrength * 1.35f);

        // --- EXECUTE THE CORE ---
        isAutoBattlerExecutingCore = true;
        
        if (innateSymbioteSwipe != null)
        {
            innateSymbioteSwipe.Execute(this);
            StartCoroutine(FlashVignetteRoutine(Color.red)); // Keep that cool red flash!
        }
        else
        {
            Debug.LogError("No Symbiote Swipe assigned to the Inspector!");
            isAutoBattlerExecutingCore = false;
        }

        // Pause this routine until OnCoreComplete() flips the flag back to false
        while (isAutoBattlerExecutingCore) yield return null;

        // Restore the original strength so stats are accurate
        playerStrength = originalStrength;

        // 4. Check if the enemy died from the attack
        if (enemyHealth <= 0)
        {
            HandleEnemyDefeat();
            yield break; // Stop the routine early, the battle is over!
        }

        // 5. The Parasitic Drain! (Take 10% max HP from the player)
        int drainAmount = Mathf.RoundToInt(playerMaxHealth * 0.05f);
        ShowBattleText("The Symbiote drains " + drainAmount + " HP to fuel its attack!", 1.5f);
        
        DealDamageToPlayer(drainAmount, true);

        // Wait for the drain text to be read!
        yield return new WaitForSeconds(1.5f);

        // 6. Check if the player died from the drain
        if (playerHealth <= 0)
        {
            TriggerConsumedGameOver();
            yield break;
        }

        // 7. Check if we need to trigger the QTE Struggle
        if (symbioteTurnsTaken >= 2)
        {
            if (qteManager != null)
            {
                ShowBattleText("FIGHT BACK! Press the sequence to break free!", 1.5f);
                yield return new WaitForSeconds(1.5f); 
                qteManager.StartQTE(3.0f); 
            }
            else
            {
                Debug.LogError("QTE Manager is missing from the Inspector!");
                EnemyTurn();
            }
        }
        else
        {
            // End the player's turn and let the enemy go
            EnemyTurn(); 
        }
    }

    // Your QTE script calls this when the timer ends!
    public void OnQTEFinished(bool success)
    {
        if (success)
        {
            Debug.Log("Player wrestles control back!");
            
            // Drop them down to Medium Bloom (e.g., 50)
            GameManager.Instance.playerData.currentBloom = 50; 
            GameManager.Instance.playerData.UpdateBloomState();
            
            // Reset the counter for next time
            symbioteTurnsTaken = 0; 
            
            // Pass the turn to the enemy, but NEXT turn the player has control!
            EnemyTurn(); 
        }
        else
        {
            Debug.Log("Player failed to break free. Symbiote keeps control.");
            // Do not lower bloom. Do not reset the counter.
            // The player just sits there and the enemy gets to attack!
            EnemyTurn(); 
        }
    }

    private void TriggerConsumedGameOver()
    {
        // Hide UI and show the terrifying death message
        HideAllMenus();
        ShowBattleText("<color=red>THE HOST HAS BEEN CONSUMED.</color>", 3f);
        
        // Wait 3 seconds, then go to the Main Menu
        Invoke(nameof(LoadMainMenu), 3f);
    }
}

