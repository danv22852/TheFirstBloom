using UnityEngine;

[CreateAssetMenu(menuName = "Cores/Guard")]
public class GuardCore : CoreTemplate
{
    [Header("Guard Settings")]
    public float damageReduction = 0.6f;
    public int guardDuration = 2;

    public override void Execute(CombatSystem system)
    {
        OnFirstCast();
        system.AddBloom(bloomCost);
        system.ApplyGuard(damageReduction, guardDuration);
        system.ShowBattleText("You brace for impact! Damage reduced for " + guardDuration + " turns!", 2f);
        system.TriggerItemAnimation(() => system.OnCoreComplete());
    }
}