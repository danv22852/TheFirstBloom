using UnityEngine;

[CreateAssetMenu(menuName = "Cores/Shock")]
public class Shock : CoreTemplate
{
    [Header("Damage Settings")]
    public int minDamage = 8;
    public int maxDamage = 14;

    [Header("Stun Settings")]
    [Range(0, 100)]
    public int stunChance = 20;

    public override void Execute(CombatSystem system)
    {
        OnFirstCast();
        system.AddBloom(bloomCost);
        system.ShowBattleText("You zap the enemy!", 1.5f);

        system.TriggerSkillAnimation(
            onHit: () =>
            {
                int damage = Random.Range(minDamage, maxDamage + 1);
                damage = Mathf.RoundToInt(damage * system.GetBloomMultiplier());
                system.DealDamageToEnemy(damage);
                system.ShowBattleText("Shock deals " + damage + " damage!", 2f);
                system.TriggerShake(true, 0.2f, 0.15f);

                int roll = Random.Range(0, 100);
                if (roll < stunChance)
                    system.ApplyStun();
            },
            onComplete: () => system.OnCoreComplete());
    }
}