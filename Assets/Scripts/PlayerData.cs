using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Player/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Stats")]
    public int currentHP = 5;
    public int maxHP = 5;
    public int strength = 15;
    public int speed = 10;
    public int defense = 5;
    public int luck = 0;

    [Header("Inventory")]
    public int healthPotions = 1;

    [Header("Equipment")]
    public bool hasAlien = false;
    // public bool hasBow = false; UNUSED

    [Header("Cores")]
    public CoreTemplate[] coreSlots = new CoreTemplate[5];
    public List<string> knownCoreIDs = new List<string>();

   // [Header("Progression")]
    public bool finishedTutorial = false;
    public List<string> defeatedEnemies = new List<string>();

    // convenience method to apply damage to the player
    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP <= 0)
        {
            currentHP = 0;
            Debug.Log("Player died.");
        }
    }

    // used if player wants to start a new run
    public void ResetForNewRun()
    {
        currentHP = maxHP;
        healthPotions = 3;
        coreSlots = new CoreTemplate[5];
        knownCoreIDs = new List<string>();
        defeatedEnemies = new List<string>();
        finishedTutorial = false;
    }
}