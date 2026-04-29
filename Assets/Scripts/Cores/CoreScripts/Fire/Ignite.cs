using UnityEngine;

[CreateAssetMenu(menuName = "Cores/Fire/Ignite")]
public class Ignite : CoreTemplate
{
    [Header("Burn Settings")]
    public int burnDamage = 14;
    public int burnDuration = 4;

    public override void Execute(CombatSystem system)
    {
        OnFirstCast();
        system.AddBloom(bloomCost);
        system.ShowBattleText("You engulf the enemy in flames!", 1.5f);

        system.TriggerSkillAnimation(
            onHit: () =>
            {
                system.ApplyStatusEffect(new StatusEffect(StatusEffectType.Burn, burnDamage, burnDuration));
                system.ShowBattleText("Enemy is heavily ignited! " + burnDamage + " burn damage per turn!", 2f);
                system.TriggerShake(true, 0.3f, 0.2f);
                system.TriggerVignette(VignetteType.Medium);
            },
            onComplete: () => system.OnCoreComplete());
    }
}