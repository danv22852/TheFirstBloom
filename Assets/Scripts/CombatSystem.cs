using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

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
    public Color lowBloomColor = new Color(0.4f, 0f, 0.6f); // Dark Purple
    public Color mediumBloomColor = new Color(1f, 0f, 0.8f); // Hot Pink
    public Color highBloomColor = new Color(1f, 0f, 0f); // Blood Red

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
    private BloomState testBloomState = BloomState.Stable;

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

        // --- 5. START THE ROUND ---
        UpdateHealthUI();
        DetermineFirstTurn();

        // DetermineFirstTurn handles calling PlayerStartTurn() or EnemyTurn()
        // so we don't need to call PlayerStartTurn() again here.
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

        isAttackHijacked = false;
        attackButtonText.text = "Attack"; 
        attackButtonText.color = Color.black; 

        if (ActiveBloomState >= BloomState.High)
        {
            Debug.Log("High Bloom! The Symbiote completely takes over your basic attacks.");
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

        BackToMainMenu();
        
    }

    public void OnAttackButton()
    {
        if (!isPlayerTurn) return;

        if (isAttackHijacked)
        {
            Debug.Log("The Symbiote hijacked your attack!");
            UseSymbioteSwipe(); 
            return; 
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

                StartCoroutine(ShakeSprite(enemyTransform, 0.2f, 0.15f));
            },
            onComplete: () =>
            {
                // --- NEW: Check for victory AFTER the animation finishes! ---
                if (enemyHealth <= 0)
                {
                    HandleEnemyDefeat(); // Send to the graveyard and load the Overworld!
                }
                else
                {
                    // If the enemy survived, continue the normal turn flow
                    CheckWinConditionOrContinue();
                }
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

        var bloomCost = 3; 
        float statMultiplier = GetBloomStatMultiplier();
        var baseSkillDamage = UnityEngine.Random.Range(28, 33);
        int finalDamage = Mathf.RoundToInt(baseSkillDamage * statMultiplier); 

        if (ActiveBloomState >= BloomState.High)
        {
            var selfDamage = UnityEngine.Random.Range(5, 11);
            playerHealth -= selfDamage;
            UpdateHealthUI();

            StartCoroutine(ShakeSprite(playerTransform, 0.4f, 0.25f));
            Debug.Log("High Bloom Penalty! Player takes " + selfDamage + " damage to fuel the attack!");

            if (playerHealth <= 0)
            {
                Debug.Log("The host was consumed. Game Over.");
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
                return;
            }
        }

        // Grab the max bloom from your data file (fallback to 100 just in case)
        int currentMaxBloom = (GameManager.Instance != null && GameManager.Instance.playerData != null) 
            ? GameManager.Instance.playerData.maxBloom 
            : 100;

        // Mathf.Min picks the smaller of the two numbers. 
        // If ActiveBloom + cost equals 115, it will force it back down to 100!
        ActiveBloom = Mathf.Min(ActiveBloom + bloomCost, currentMaxBloom);
        UpdateHealthUI(); 
        
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
                // --- NEW: Check for victory AFTER the animation finishes! ---
                if (enemyHealth <= 0)
                {
                    HandleEnemyDefeat(); // Send to the graveyard and load the Overworld!
                }
                else
                {
                    // If the enemy survived, continue the normal turn flow
                    CheckWinConditionOrContinue();
                }
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

        // Prevent errors in testing mode if GameManager is null
        if (GameManager.Instance == null)
        {
            Debug.Log("Testing Mode: Pretending to use a potion.");
        }
        else if (GameManager.Instance.playerData.healthPotions <= 0)
        {
            Debug.Log("No potions left!");
            return;
        }
        else 
        {
            GameManager.Instance.playerData.healthPotions--;
        }

        isPlayerTurn = false; 
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
                BackToMainMenu(); 
            }));
    }

    public void OnRunButton()
    {
        if (!isPlayerTurn) return;

        if (ActiveBloomState >= BloomState.Low)
        {
            Debug.Log("You are in " + ActiveBloomState + " Bloom! The symbiote won't let you run!");
            return;
        }

        HideAllMenus();

        var escapeChance = UnityEngine.Random.Range(0, 100);
        if (escapeChance > 50) 
        {
            Debug.Log("Escaped successfully!");
            isPlayerTurn = false; 
            
            StartCoroutine(PerformRunAnimation(playerTransform, 
                onComplete: () => 
                {
                    PersistStatsToPlayerData();
                    if (GameManager.Instance != null)
                    {
                        SceneManager.LoadScene(GameManager.Instance.playerData.floorName);
                    }
                }));
        }
        else
        {
            Debug.Log("Failed to escape!");
            isPlayerTurn = false;
            EnemyTurn(); 
        }
    }

    private void HandleEnemyDefeat()
    {
        Debug.Log("<color=green>VICTORY! The enemy is defeated.</color>");

        // 1. Save all player stats using your custom helper function!
        PersistStatsToPlayerData(); 

        // 2. THE GRAVEYARD LOGIC 
        if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.encounteredInstanceID))
        {
            if (!GameManager.Instance.playerData.defeatedEnemies.Contains(GameManager.encounteredInstanceID))
            {
                GameManager.Instance.playerData.defeatedEnemies.Add(GameManager.encounteredInstanceID);
            }
            GameManager.encounteredInstanceID = ""; 
        }

        // 3. RETURN TO OVERWORLD (Using your dynamic floorName!)
        if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.playerData.floorName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(GameManager.Instance.playerData.floorName);
        }
        else
        {
            // Fallback just in case
            UnityEngine.SceneManagement.SceneManager.LoadScene("SecondFloor"); 
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
        Debug.Log(currentEnemy.enemyName + "'s turn!");
        
        var skill = PickSkill();

        StartCoroutine(PerformMeleeAttack(enemyTransform, playerTransform,
            onHit: () =>
            {
                if (skill != null)
                {
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
                    
                    Debug.Log("Player takes " + actualDamage + " damage. (Effective Defense: " + effectiveDefense + ")");
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
            Debug.Log("Player died. Game Over.");
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

    public void DealDamageToPlayer(int amount, bool ignoreDefense)
    {
        var actualDamage = ignoreDefense ? amount : Mathf.Max(1, amount - playerDefense);
        playerHealth -= actualDamage;
        UpdateHealthUI();
    }

    public void EndEnemyTurn() { }

    private void UpdateHealthUI()
    {
        // 1. Text Updates
        if (playerHP != null) 
        {
            playerHP.text = "HP: " + playerHealth + " / " + playerMaxHealth;
        }

        // --- THE FIX: Use the dynamic enemy name! ---
        if (enemyHP != null && currentEnemy != null) 
        {
            // This will now print "Hornet HP: 130 / 130"
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

        if (playerHpSlider != null) playerHpSlider.maxValue = playerMaxHealth;
        if (enemyHpSlider != null) enemyHpSlider.maxValue = currentEnemy.maxHP;
        if (bloomSlider != null) bloomSlider.maxValue = 100;

        // 3. APPLY CURRENT VALUES
        if (playerHpSlider != null) playerHpSlider.value = playerHealth;
        if (enemyHpSlider != null) enemyHpSlider.value = enemyHealth;
        if (bloomSlider != null) bloomSlider.value = ActiveBloom;

        // 4. COLOR SHIFT
        if (bloomFillImage != null)
        {
            if (ActiveBloomState == BloomState.Low) bloomFillImage.color = lowBloomColor;
            else if (ActiveBloomState == BloomState.Medium) bloomFillImage.color = mediumBloomColor;
            else if (ActiveBloomState == BloomState.High) bloomFillImage.color = highBloomColor;
        }
        
        // DEBUG: If it's still breaking, this will tell us why!
        Debug.Log($"UI UPDATED: PlayerSlider({playerHpSlider.value}/{playerHpSlider.maxValue}), EnemySlider({enemyHpSlider.value}/{enemyHpSlider.maxValue}), BloomSlider({bloomSlider.value}/{bloomSlider.maxValue})");
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

    // --- MENU NAVIGATION ---

    public void OpenSkillMenu()
    {
        if (!isPlayerTurn) return;
        mainMenuPanel.SetActive(false);
        skillMenuPanel.SetActive(true);

        StartCoroutine(HighlightButtonSafe(skillDefaultButton));
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
                healItemText.text = "Heal +20 (x" + potions + ")";
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
            healItemText.text = "Heal +20 (Testing)";
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
}