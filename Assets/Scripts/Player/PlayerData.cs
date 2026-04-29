using UnityEngine;
using System.Collections.Generic;
using System;

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
    public int coins = 0;
    public int wiltPotions = 0;
    public int keys = 0;
    public int thirdItem = 0;

    [Header("World State")]
    public List<string> defeatedEnemies = new List<string>();
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

    [Header("Progression")]
    public bool finishedTutorial = false;

    public ExperienceSystem expSystem = new ExperienceSystem();

    [Header("Bloom Mechanics")]
    public int decayFloor = 0;

    [Header("Item Discovery")]
    public bool hasDiscoveredWiltPotions = false;

    // -------------------------
    // BLOOM LOGIC
    // -------------------------
    public void SetDecayFloor()
    {
        if (currentBloom >= 75) decayFloor = 50;
        else if (currentBloom >= 50) decayFloor = 25;
        else decayFloor = 0;
    }

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

    // -------------------------
    // DAMAGE / DEATH
    // -------------------------
    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        if (currentHP <= 0)
        {
            currentHP = 0;
            Debug.Log("Player died.");

            ResetOnDeath(); // 🔥 IMPORTANT
        }

        OnStatsChanged?.Invoke();
    }

    public void AcquireAlienPower()
    {
        hasAlien = true;
        OnStatsChanged?.Invoke();
    }

    // -------------------------
    // RESET: NEW RUN
    // -------------------------
    public void ResetForNewRun()
    {
        Debug.Log("Resetting Player Data for New Run");

        currentHP = maxHP;
        healthPotions = 3;
        coins = 0;
        wiltPotions = 0;

        equippedCores = new List<CoreTemplate>();
        knownCoreIDs = new List<string>();

        defeatedEnemies.Clear();
        collectedItems.Clear();

        hasAlien = false;
        finishedTutorial = false;

        floorName = "firstFloor";

        currentBloom = 0;
        UpdateBloomState();

        OnStatsChanged?.Invoke();
    }

    // -------------------------
    // RESET: SCENE CHANGE
    // -------------------------
    public void ResetSceneState()
    {
        Debug.Log("Resetting Scene State");

        defeatedEnemies.Clear();
        collectedItems.Clear();

        OnStatsChanged?.Invoke();
    }

    // -------------------------
    // RESET: PLAYER DEATH
    // -------------------------
    public void ResetOnDeath()
{
    Debug.Log("Resetting On Death → Full Reset + Save Sync");

    currentHP = maxHP;
    healthPotions = 3;
    coins = 0;
    wiltPotions = 0;

    equippedCores.Clear();
    knownCoreIDs.Clear();

    defeatedEnemies.Clear();
    collectedItems.Clear();

    hasAlien = false;
    finishedTutorial = false;

    currentBloom = 0;
    UpdateBloomState();

    floorName = "firstFloor";

    expSystem = new ExperienceSystem();

    OnStatsChanged?.Invoke();

    // 🔥 CRITICAL: sync to save file immediately
    if (GameManager.Instance != null)
    {
        // GameManager.Instance.SaveGame();
    }
}
}