using UnityEngine;

[CreateAssetMenu(menuName = "Cores/Last Stand")]
public class LastStand : CoreTemplate
{
    [Header("Damage Settings")]
    public int baseDamage = 10;
    public int maxDamage = 60;

    public override void Execute(CombatSystem system)
    {
        OnFirstCast();
        system.AddBloom(bloomCost);

        int damage = system.GetLastStandDamage(baseDamage, maxDamage);
        system.ShowBattleText("You fight with everything you have left!", 1.5f);

        system.TriggerSkillAnimation(
            onHit: () =>
            {
                system.DealDamageToEnemy(damage);
                system.ShowBattleText("Last Stand deals " + damage + " damage!", 2f);
                system.TriggerShake(true, 0.4f, 0.3f);
                system.TriggerVignette(VignetteType.High);
            },
            onComplete: () => system.OnCoreComplete());
    }
}