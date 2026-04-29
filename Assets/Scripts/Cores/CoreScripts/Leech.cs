using UnityEngine;

[CreateAssetMenu(menuName = "Cores/Leech")]
public class Leech : CoreTemplate
{
    [Header("Damage Settings")]
    public int minDamage = 15;
    public int maxDamage = 22;

    [Header("Leech Settings")]
    [Range(0f, 1f)]
    public float leechPercent = 0.5f; // heals 50% of damage dealt

    public override void Execute(CombatSystem system)
    {
        OnFirstCast();
        system.AddBloom(bloomCost);
        system.ShowBattleText("You drain the enemy's life force!", 1.5f);

        system.TriggerSkillAnimation(
            onHit: () =>
            {
                int damage = Random.Range(minDamage, maxDamage + 1);
                damage = Mathf.RoundToInt(damage * system.GetBloomMultiplier());
                system.DealDamageToEnemy(damage);

                int healAmount = Mathf.RoundToInt(damage * leechPercent);
                system.HealPlayer(healAmount);

                system.ShowBattleText("Leech deals " + damage + " damage and restores " + healAmount + " HP!", 2f);
                system.TriggerShake(true, 0.3f, 0.2f);
            },
            onComplete: () => system.OnCoreComplete());
    }
}