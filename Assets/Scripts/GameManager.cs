using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;  

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Data")]
    public PlayerData playerData;
    public static bool playerFirstStrike = false;

    [Header("Persistence")]
    public static Vector3 lastPlayerPosition;
    public static bool isReturningFromCombat = false;
    // Store the NAME of the boundary object to find it across scenes
    public static string currentMapBoundaryName;

    public static string currentEnemyID = "";
    public static string encounteredInstanceID = "";
    // This will hold the EXACT data file passed from the Overworld!
    public static EnemyData pendingEnemyData;

    [Header("Enemy Roster")]
    public EnemyData[] enemyRoster;
    
    private void OnEnable()
{
    SceneManager.sceneLoaded += OnSceneLoaded;
}

private void OnDisable()
{
    SceneManager.sceneLoaded -= OnSceneLoaded;
}

private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    PlayerController pc = FindFirstObjectByType<PlayerController>();
    if (pc != null)
    {
        pc.canMove = true;
        pc.EnableMovement();
    }
}
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        playerData.TakeDamage(damage);
    }

    public EnemyData GetEnemyByID(string id)
    {
        foreach (var enemy in enemyRoster)
        {
            if (enemy.enemyID == id)
                return enemy;
        }
        Debug.LogWarning("No enemy found with ID: " + id);
        return null;
    }

    // This magic tag creates a custom button in your Unity Editor!
    [ContextMenu("Reset Save Data")]
    public void ResetSaveData()
    {
        if (playerData != null)
        {
            // 1. Empty the Enemy Graveyard
            if (playerData.defeatedEnemies != null)
            {
                playerData.defeatedEnemies.Clear();
            }

            // 2. --- NEW: Empty the Collected Items list ---
            if (playerData.collectedItems != null)
            {
                playerData.collectedItems.Clear();
            }

            // (Optional) 3. Reset the player's actual inventory and stats back to default!
            playerData.expSystem.level = 1;
            playerData.expSystem.currentEXP = 0;
            playerData.currentHP = 100;
            playerData.maxHP = 100;
            playerData.strength = 10;
            playerData.speed = 10;
            playerData.defense = 10;
            playerData.luck = 0;
            playerData.coins = 0;
            playerData.healthPotions = 3;
            

            Debug.Log("Save Data completely reset! All enemies and items will respawn.");
        }
        else
        {
            Debug.LogWarning("Could not reset data: PlayerData is missing.");
            return;
        }

        // 4. Reload the scene to physically spawn everything back in!
        UnityEngine.SceneManagement.Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene.buildIndex);
    }

    private void Update()
    {
        // If you press the F12 key on your keyboard while playing...
        if (Input.GetKeyDown(KeyCode.F12))
        {
            ResetSaveData();
        }
    }
}