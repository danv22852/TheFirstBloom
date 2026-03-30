using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Data")]
    public PlayerData playerData;

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
            // 1. Empty the graveyard completely!
            playerData.defeatedEnemies.Clear();

            // 2. Heal the player back to their maximum health
            playerData.currentHP = playerData.maxHP;

            Debug.Log("<color=cyan>SAVE DATA RESET! The graveyard is empty and player is healed.</color>");
        }
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