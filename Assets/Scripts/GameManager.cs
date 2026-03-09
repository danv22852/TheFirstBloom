using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Stats")]
    public int currentHP = 5;
    public static int healthPotions = 3;
    public int maxHP = 5;

    [Header("Equipment")]
    public bool hasAlien = false;
    public bool hasBow;

    [Header("Persistence")]
    public static Vector3 lastPlayerPosition;
    public static bool isReturningFromCombat = false;
    // Store the NAME of the boundary object to find it across scenes
    public static string currentMapBoundaryName; 

    public static string currentEnemyID = "";
    public static List<string> defeatedEnemies = new List<string>();
    
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
        currentHP -= damage;
        if (currentHP <= 0)
        {
            currentHP = 0;
            Debug.Log("Player died");
        }
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