using UnityEngine;
using System.Collections.Generic; // We need this to use Lists!

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
            
            // 2. Pick exactly 3 random unique stats to increase!
            if (playerStats != null)
            {
                // Create a list representing our 5 stats (0=HP, 1=Str, 2=Spd, 3=Def, 4=Luck)
                List<int> statPool = new List<int> { 0, 1, 2, 3, 4 };

                // Shuffle the list randomly
                for (int i = 0; i < statPool.Count; i++)
                {
                    int temp = statPool[i];
                    int randomIndex = Random.Range(i, statPool.Count);
                    statPool[i] = statPool[randomIndex];
                    statPool[randomIndex] = temp;
                }

                // Now just take the first 3 numbers from our shuffled list!
                for (int i = 0; i < 3; i++)
                {
                    int chosenStat = statPool[i];

                    if (chosenStat == 0) playerStats.maxHP += 5; 
                    else if (chosenStat == 1) playerStats.strength += 1;
                    else if (chosenStat == 2) playerStats.speed += 1;
                    else if (chosenStat == 3) playerStats.defense += 1;
                    else if (chosenStat == 4) playerStats.luck += 1; 
                }

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