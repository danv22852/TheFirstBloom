using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System;

public class TutorialBattle : MonoBehaviour
{
    private UnityEngine.EventSystems.EventSystem cachedEventSystem;

    // --- PLAYER STATS ---
    private int playerHealth;
    private int playerMaxHealth;
    public int playerStrength;
    public int playerSpeed;
    public int playerDefense;

    // --- ENEMY STATS ---
    [Header("Enemy Stats")]
    public EnemyData currentEnemy;
    private int enemyHealth;
    private int enemySpeed; 

    // --- COMBAT STATE ---
    private bool isPlayerTurn = false;
    private bool hasUsedItemThisTurn = false;
    private int enemyTurnCount = 0;

    // --- UI ELEMENTS ---
    [Header("UI Elements")]
    public TextMeshProUGUI playerHP;
    public TextMeshProUGUI enemyHP;

    [Header("Keyboard Navigation Defaults")]
    public GameObject mainDefaultButton;  
    public GameObject itemDefaultButton;  

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
        cachedEventSystem = UnityEngine.EventSystems.EventSystem.current;

        // Read player stats from PlayerData
        var pd = GameManager.Instance.playerData;
        playerHealth = pd.currentHP;
        playerMaxHealth = pd.maxHP;
        playerStrength = pd.strength;
        playerSpeed = pd.speed;
        playerDefense = pd.defense;

        enemyHealth = currentEnemy.maxHP;
        enemySpeed = currentEnemy.speed;
        
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

        BackToMainMenu();
    }

    // --- PLAYER ACTIONS ---

    public void OnAttackButton()
    {
        if (!isPlayerTurn) return;

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

        GameManager.Instance.playerData.healthPotions--;
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

        HideAllMenus();
        isPlayerTurn = false;

        Debug.Log("There's no escaping this tutorial fight! You stumbled and wasted your turn.");

        // Immediately pass the turn to the enemy
        EnemyTurn(); 
    }

    private void CheckWinConditionOrContinue()
    {
        if (enemyHealth <= 0)
        {
            Debug.Log("Enemy defeated!");

            GameManager.Instance.playerData.currentHP = playerHealth;

            if (!GameManager.Instance.playerData.defeatedEnemies.Contains(GameManager.currentEnemyID))
            {
                GameManager.Instance.playerData.defeatedEnemies.Add(GameManager.currentEnemyID);
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
            Debug.Log("Enemy prepares a devastating scripted attack!");
            
            StartCoroutine(PerformLeapAnimation(enemyTransform, playerTransform,
                onHit: () =>
                {
                    Debug.Log("DEVASTATING BLOW! Player drops to 1 HP!");
                    playerHealth = 1;
                    UpdateHealthUI();
                    StartCoroutine(ShakeSprite(playerTransform, 0.4f, 0.3f));
                },
                onComplete: () =>
                {
                    GameManager.Instance.playerData.currentHP = playerHealth;
                    GameManager.Instance.playerData.finishedTutorial = true;
                    SceneManager.LoadScene("firstFloor");
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
                    
                    Debug.Log("Player takes " + actualDamage + " damage. (Effective Defense: " + playerDefense + ")");
                    StartCoroutine(ShakeSprite(playerTransform, 0.3f, 0.2f));
                },
                onComplete: () =>
                {
                    StartCoroutine(WaitAndPassTurn(1.0f));
                }));
        }
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

    private void UpdateHealthUI()
    {
        playerHP.text = "Player HP: " + playerHealth + " / " + playerMaxHealth;
        enemyHP.text = "Enemy HP: " + enemyHealth + " / " + currentEnemy.maxHP;
    }

    // --- NEW ANIMATIONS & COROUTINES ---

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

    private IEnumerator PerformLeapAnimation(Transform attacker, Transform target, Action onHit, Action onComplete)
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
        if (isPlayerTurn && (Input.GetKeyDown(KeyCode.P)) || (Input.GetKeyDown(KeyCode.X)))
        {
            if (itemMenuPanel.activeSelf)
            {
                BackToMainMenu();
            }
        }
    }
}