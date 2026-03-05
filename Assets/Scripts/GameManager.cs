using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Stats")]
    public int currentHP = 5;
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
}