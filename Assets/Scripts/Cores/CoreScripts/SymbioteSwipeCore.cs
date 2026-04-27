using UnityEngine;

[CreateAssetMenu(menuName = "Cores/Symbiote/Symbiote Swipe")]
public class SymbioteSwipeCore : CoreTemplate
{
    [Header("Damage Scaling Settings")]
    [Tooltip("Multiplies the player's Strength stat. 2.5x makes it a heavy armor-breaker.")]
    public float strengthMultiplier = 2.5f;
    
    [Tooltip("Adds a random +/- percentage to the final damage (e.g., 0.1 for 10% variance).")]
    public float damageVariance = 0.1f;

    public override void Execute(CombatSystem system)
    {
        OnFirstCast();

        // 1. Calculate base scaling damage
        float scalingDamage = system.playerStrength * strengthMultiplier;
        float variance = Random.Range(1f - damageVariance, 1f + damageVariance);
        int baseDamage = Mathf.RoundToInt(scalingDamage * variance);
        
        // 2. Apply Bloom buffs
        float statMultiplier = system.GetBloomMultiplier();
        int buffedDamage = Mathf.RoundToInt(baseDamage * statMultiplier);

        // --- NEW: ARMOR CALCULATION ---
        int effectiveDefense = system.currentEnemy.defense;

        // If the Troll is guarding, we completely ignore its 5x multiplier!
        if (system.isBossFight && system.isBossGuarding)
        {
            Debug.Log("Symbiote Swipe ignores the Troll's Stone Skin!");
            // We just leave effectiveDefense as the base defense
        }

        // Subtract defense from the final damage
        int finalDamage = Mathf.Max(1, buffedDamage - effectiveDefense);

        // 3. HIGH BLOOM PENALTY
        if (system.GetBloomState() >= BloomState.High)
        {
            // Now calculates exactly 5% of the player's Max HP instead of a random number
            int selfDamage = Mathf.Max(1, Mathf.RoundToInt(system.GetPlayerMaxHealth() * 0.05f));
            
            system.DealDamageToPlayer(selfDamage, true);
            system.TriggerShake(false, 0.4f, 0.25f);
            system.ShowBattleText("High Bloom Penalty! You take " + selfDamage + " damage to fuel the attack!", 2.5f);

            if (system.IsPlayerDefeated())
            {
                system.ShowBattleText("The host was consumed. Game Over.", 3f);
                system.TriggerGameOver();
                return;
            }
        }
        else
        {
            system.ShowBattleText("You unleash a Symbiote Swipe!", 1.5f);
        }

        system.AddBloom(bloomCost);

        // 4. EXECUTE ANIMATION AND DAMAGE
        system.TriggerSkillAnimation(
            onHit: () =>
            {
                system.DealDamageToEnemy(finalDamage);
                system.ShowBattleText("Symbiote Swipe deals " + finalDamage + " damage!", 2f);
                system.TriggerShake(true, 0.3f, 0.3f);

                VignetteType vignette = VignetteType.Low;
                if (system.GetBloomState() == BloomState.Medium) vignette = VignetteType.Medium;
                else if (system.GetBloomState() >= BloomState.High) vignette = VignetteType.High;
                system.TriggerVignette(vignette);
            },
            onComplete: () =>
            {
                system.OnCoreComplete();
            });
    }

    public override string GetDynamicDescription(CombatSystem system)
    {
        if (system == null) return coreDescription;

        float scalingDamage = system.playerStrength * strengthMultiplier;
        int minDamage = Mathf.Max(1, Mathf.RoundToInt(scalingDamage * (1f - damageVariance)));
        int maxDamage = Mathf.Max(1, Mathf.RoundToInt(scalingDamage * (1f + damageVariance)));

        // Accurately reflect the defense subtraction in the UI!
        if (system.currentEnemy != null) 
        {
            minDamage = Mathf.Max(1, minDamage - system.currentEnemy.defense);
            maxDamage = Mathf.Max(1, maxDamage - system.currentEnemy.defense);
        }

        string damageText = (minDamage == maxDamage) ? minDamage.ToString() : $"{minDamage} - {maxDamage}";

        string text = coreDescription;
        text += "\nExpected Damage: " + damageText;
        if (bloomCost > 0) text += "\nCost: " + bloomCost + " Bloom";

        return text;
    }
}