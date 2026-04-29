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
    public int currentHP = 100;
    public int maxHP = 100;
    public int strength = 10;
    public int speed = 10;
    public int defense = 10;
    public int luck = 0;

    public string floorName = "firstFloor";

    [Header("Inventory")]
    public int healthPotions = 3;
    public int coins = 0; // <-- NEW: Track the player's money
    public int wiltPotions = 0;

    public int keys = 0; // <-- NEW: Track the player's keys
    public int thirdItem = 0; //placeholder

    [Header("World State")]
    public List<string> defeatedEnemies = new List<string>();
    // --- NEW: Track which items have been picked up so they stay gone! ---
    public List<string> collectedItems = new List<string>();

    [Header("Equipment")]
    public bool hasAlien = false;

    [Header("Symbiote / Bloom")]
    public int currentBloom = 0;
    public int maxBloom = 100;
    public BloomState currentBloomState = BloomState.Stable;

    [Header("Cores")]
    public List<CoreTemplate> equippedCores = new List<CoreTemplate>();
    public int maxCoreSlots = 5;
    public List<string> knownCoreIDs = new List<string>();

    public bool finishedTutorial = false;
    

    [Header("Progression")]
    // This creates an instance of your new script directly inside PlayerData!
    public ExperienceSystem expSystem = new ExperienceSystem();

    [Header("Bloom Mechanics")]
    public int decayFloor = 0;

    [Header("Item Discovery")]
    // Tracks if the player has ever picked up a Wilt Potion
    public bool hasDiscoveredWiltPotions = false;

    // Call this whenever the player finishes a fight or drinks a Wilt Potion!
    public void SetDecayFloor()
    {
        // High (75+) stops decaying at 50 (Bottom of Medium)
        if (currentBloom >= 75) decayFloor = 50;
        
        // Medium (50-74) stops decaying at 25 (Bottom of Low)
        else if (currentBloom >= 50) decayFloor = 25;
        
        // Low (25-49) or Stable naturally decays all the way to 0
        else decayFloor = 0; 
    }

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
        equippedCores = new List<CoreTemplate>();
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