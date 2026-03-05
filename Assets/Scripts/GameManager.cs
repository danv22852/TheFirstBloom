using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Stats")]
    public int currentHP = 5;
    public int maxHP = 5;

    [Header("Equipment")]
    public bool hasAlien = true;
    public bool hasBow;

    public static Vector3 lastPlayerPosition;
    public static bool isReturningFromCombat = false;

    // Remembers the ID of the enemy we are currently fighting
    public static string currentEnemyID = "";

    // The "Graveyard" list of enemies that have been killed
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
        if (currentHP <= 0){currentHP = 0;
        Debug.Log("Player died" + currentHP);
        }
    }
}