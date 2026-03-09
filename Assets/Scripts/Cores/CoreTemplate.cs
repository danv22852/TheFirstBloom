using UnityEngine;

public enum CoreRarity
{
    Common,     // 0
    Rare,       // 1
    Legendary   // 2
}

public abstract class CoreTemplate : ScriptableObject
{
    [Header("Core Info")]
    public string coreID;
    public string coreName;
    public string coreDescription;
    public CoreRarity rarity;
    public Sprite coreSprite;

    [Header("Combat")]
    public int bloomCost;

    [Header("Discovery")]
    // True once the player has cast this core in combat at least once
    // Bloom cost is hidden in the UI until this is set
    // Note: move to PlayerData / save system when migration happens
    public bool isKnown = false;

    public abstract void Execute(CombatSystem system);

    // Called by the CombatSystem the first time this core is cast in combat
    public void OnFirstCast()
    {
        if (!isKnown)
        {
            isKnown = true;
            Debug.Log(coreName + " bloom cost revealed: " + bloomCost);
        }
    }
}