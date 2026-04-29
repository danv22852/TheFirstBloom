using UnityEngine;

[CreateAssetMenu(menuName = "Cores/Weaken")]
public class Weaken : CoreTemplate
{
    [Header("Weaken Settings")]
    public float damageMultiplier = 0.5f; // enemy deals 50% damage
    public int duration = 3;

    public override void Execute(CombatSystem system)
    {
        OnFirstCast();
        system.AddBloom(bloomCost);
        system.ShowBattleText("You sap the enemy's strength!", 1.5f);

        system.TriggerSkillAnimation(
            onHit: () =>
            {
                system.ApplyStatusEffect(new StatusEffect(StatusEffectType.Weaken, 0, duration, damageMultiplier));
                system.ShowBattleText(system.GetEnemyName() + " is weakened for " + duration + " turns!", 2f);
                system.TriggerShake(true, 0.2f, 0.15f);
            },
            onComplete: () => system.OnCoreComplete());
    }
}