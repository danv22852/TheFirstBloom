using UnityEngine;

[CreateAssetMenu(menuName = "Cores/Lightning Bolt")]
public class LightningBolt : CoreTemplate
{
    [Header("Damage Settings")]
    public int minDamage = 15;
    public int maxDamage = 22;

    [Header("Stun Settings")]
    [Range(0, 100)]
    public int stunChance = 40;

    public override void Execute(CombatSystem system)
    {
        OnFirstCast();
        system.AddBloom(bloomCost);
        system.ShowBattleText("You call down a lightning bolt!", 1.5f);

        system.TriggerSkillAnimation(
            onHit: () =>
            {
                int damage = Random.Range(minDamage, maxDamage + 1);
                damage = Mathf.RoundToInt(damage * system.GetBloomMultiplier());
                system.DealDamageToEnemy(damage);
                system.ShowBattleText("Lightning Bolt deals " + damage + " damage!", 2f);
                system.TriggerShake(true, 0.3f, 0.2f);
                system.TriggerVignette(VignetteType.Medium);

                int roll = Random.Range(0, 100);
                if (roll < stunChance)
                    system.ApplyStun();
            },
            onComplete: () =>
            {
                system.OnCoreComplete();
            });
    }
}