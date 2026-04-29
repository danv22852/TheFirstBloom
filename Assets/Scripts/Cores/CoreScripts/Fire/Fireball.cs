using UnityEngine;

[CreateAssetMenu(menuName = "Cores/Fire/Fireball")]
public class Fireball : CoreTemplate
{
    [Header("Damage Settings")]
    public int minDamage = 20;
    public int maxDamage = 28;
    public int burnDamage = 18;
    public int burnDuration = 5;

    public override void Execute(CombatSystem system)
    {
        OnFirstCast();
        system.AddBloom(bloomCost);
        system.ShowBattleText("You hurl a massive fireball!", 1.5f);

        system.TriggerSkillAnimation(
            onHit: () =>
            {
                int damage = Random.Range(minDamage, maxDamage + 1);
                damage = Mathf.RoundToInt(damage * system.GetBloomMultiplier());
                system.DealDamageToEnemy(damage);
                system.ApplyStatusEffect(new StatusEffect(StatusEffectType.Burn, burnDamage, burnDuration));
                system.ShowBattleText("Fireball deals " + damage + " damage and scorches the enemy!", 2f);
                system.TriggerShake(true, 0.4f, 0.3f);
                system.TriggerVignette(VignetteType.High);
            },
            onComplete: () => system.OnCoreComplete());
    }
}