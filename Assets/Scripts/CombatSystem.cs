using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
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

    private void Start()
    {
        cachedEventSystem = UnityEngine.EventSystems.EventSystem.current;

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

        if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.currentEnemyID))
        {
            var found = GameManager.Instance.GetEnemyByID(GameManager.currentEnemyID);
            if (found != null) currentEnemy = found;
        }

        if (currentEnemy == null)
        {
            Debug.LogError("CRASH AVOIDED: No Enemy Data! Drag an Enemy ScriptableObject into the Inspector!");
            return;
        }

        var sr = enemyTransform.GetComponent<SpriteRenderer>();
        if (currentEnemy.enemySprite != null && sr != null)
        {
            sr.sprite = currentEnemy.enemySprite;
        }

        enemyHealth = currentEnemy.maxHP;
        enemySpeed = currentEnemy.speed;
        
        UpdateHealthUI();
        DetermineFirstTurn();

        if (isPlayerTurn)
        {
            PlayerStartTurn(); 
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

        isAttackHijacked = false;
        attackButtonText.text = "Attack"; 
        attackButtonText.color = Color.white; 

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

        // Modifying this now writes directly to PlayerData behind the scenes!
        ActiveBloom += bloomCost;
        UpdateHealthUI(); // Update UI to show new Bloom value
        
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
        playerHP.text = "Player HP: " + playerHealth + " / " + playerMaxHealth;
        enemyHP.text = "Enemy HP: " + enemyHealth + " / " + currentEnemy.maxHP;
        bloomText.text = "Bloom: " + ActiveBloom;
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
        if (isPlayerTurn && (Input.GetKeyDown(KeyCode.P)) || (Input.GetKeyDown(KeyCode.X)))
        {
            if (skillMenuPanel.activeSelf || itemMenuPanel.activeSelf)
            {
                BackToMainMenu();
            }
        }
    }
}