using UnityEngine;

[System.Serializable] 
public class ExperienceSystem
{
    [Header("Level Stats")]
    public int level = 1;
    public int currentEXP = 0;
    public int expToNextLevel = 100;
    public int availableSkillPoints = 0;

    public bool AddEXP(int amount, PlayerData playerStats)
    {
        currentEXP += amount;
        bool leveledUp = false;

        while (currentEXP >= expToNextLevel)
        {
            currentEXP -= expToNextLevel; 
            level++;
            
            // 1. Give the player exactly ONE manual point to spend
            availableSkillPoints += 1; 
            
            // 2. 50% chance for each stat to naturally increase
            if (playerStats != null)
            {
                // We use Random.Range(0, 100) < 50 for a flat 50% chance.
                // (I set Max HP to grow by 5 so it stays balanced with other stats, 
                // but you can change these numbers to whatever you like!)
                if (Random.Range(0, 100) < 50) playerStats.maxHP += 5; 
                if (Random.Range(0, 100) < 50) playerStats.strength += 1;
                if (Random.Range(0, 100) < 50) playerStats.speed += 1;
                if (Random.Range(0, 100) < 50) playerStats.defense += 1;
                if (Random.Range(0, 100) < 50) playerStats.luck += 1; 

                // Fully heal the player (and apply any new Max HP bonuses!)
                playerStats.currentHP = playerStats.maxHP;
            }
            
            // Increase the cost of the next level
            expToNextLevel = Mathf.RoundToInt(expToNextLevel * 1.5f); 
            
            leveledUp = true;
        }

        return leveledUp;
    }
}   