using UnityEngine;

[CreateAssetMenu(menuName = "Cores/Fire/Spark")]
public class Spark : CoreTemplate
{
    [Header("Damage Settings")]
    public int minDamage = 8;
    public int maxDamage = 12;
    public int burnDamage = 4;
    public int burnDuration = 2;

    public override void Execute(CombatSystem system)
    {
        OnFirstCast();
        system.AddBloom(bloomCost);
        system.ShowBattleText("You flick a spark at the enemy!", 1.5f);

        system.TriggerSkillAnimation(
            onHit: () =>
            {
                int damage = Random.Range(minDamage, maxDamage + 1);
                system.DealDamageToEnemy(damage);
                system.ApplyStatusEffect(new StatusEffect(StatusEffectType.Burn, burnDamage, burnDuration));
                system.ShowBattleText("Spark deals " + damage + " damage and ignites the enemy!", 2f);
                system.TriggerShake(true, 0.2f, 0.15f);
            },
            onComplete: () => system.OnCoreComplete());
    }
}