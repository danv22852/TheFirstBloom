using UnityEngine;

[CreateAssetMenu(menuName = "Cores/Symbiote/Symbiote Swipe")]
public class SymbioteSwipeCore : CoreTemplate
{
    [Header("Damage Settings")]
    public int minDamage = 28;
    public int maxDamage = 33;

    public override void Execute(CombatSystem system)
    {
        OnFirstCast();

        float statMultiplier = system.GetBloomMultiplier();
        int baseDamage = Random.Range(minDamage, maxDamage + 1);
        int finalDamage = Mathf.RoundToInt(baseDamage * statMultiplier);

        // High bloom penalty
        if (system.GetBloomState() >= BloomState.High)
        {
            int selfDamage = Random.Range(5, 11);
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
}