using UnityEngine;
using System.Collections.Generic;
using System;

// Moved the enum here so it is globally accessible across the project
public enum BloomState
{
    Stable, // 0-24
    Low,    // 25-49
    Medium, // 50-74
    High,   // 75-99
    Total   // 100
}

[CreateAssetMenu(fileName = "PlayerData", menuName = "Player/Player Data")]
public class PlayerData : ScriptableObject
{
    public event Action OnStatsChanged;

    [Header("Stats")]
    public int currentHP = 5;
    public int maxHP = 5;
    public int strength = 15;
    public int speed = 10;
    public int defense = 5;
    public int luck = 0;

    public string floorName = "firstFloor";

    [Header("Inventory")]
    public int healthPotions = 1;

    [Header("Equipment")]
    public bool hasAlien = false;

    [Header("Symbiote / Bloom")]
    public int currentBloom = 0;
    public BloomState currentBloomState = BloomState.Stable;

    [Header("Cores")]
    public CoreTemplate[] coreSlots = new CoreTemplate[5];
    public List<string> knownCoreIDs = new List<string>();

    public bool finishedTutorial = false;
    public List<string> defeatedEnemies = new List<string>();

    // Call this whenever Bloom is modified outside of combat
    public void UpdateBloomState()
    {
        if (currentBloom > 100) currentBloom = 100;

        if (currentBloom >= 100) currentBloomState = BloomState.Total;
        else if (currentBloom >= 75) currentBloomState = BloomState.High;
        else if (currentBloom >= 50) currentBloomState = BloomState.Medium;
        else if (currentBloom >= 25) currentBloomState = BloomState.Low;
        else currentBloomState = BloomState.Stable;

        OnStatsChanged?.Invoke();
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP <= 0)
        {
            currentHP = 0;
            Debug.Log("Player died.");
        }
        
        OnStatsChanged?.Invoke(); 
    }

    public void AcquireAlienPower()
    {
        hasAlien = true;
        OnStatsChanged?.Invoke();
    }

    public void ResetForNewRun()
    {
        Debug.Log("Resetting Player Data for New Run");
        currentHP = maxHP;
        healthPotions = 3;
        coreSlots = new CoreTemplate[5];
        knownCoreIDs = new List<string>();
        defeatedEnemies = new List<string>();
        hasAlien = false;
        finishedTutorial = false;
        floorName = "firstFloor";
        
        // Reset Bloom for the new run
        currentBloom = 0;
        UpdateBloomState();

        OnStatsChanged?.Invoke(); 
    }
}