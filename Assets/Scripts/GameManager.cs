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
}