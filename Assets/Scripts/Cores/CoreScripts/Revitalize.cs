using UnityEngine;

[CreateAssetMenu(menuName = "Cores/Revitalize")]
public class Revitalize : CoreTemplate
{
    [Header("Heal Settings")]
    public float healPercentPerTurn = 0.15f;
    public int healDuration = 3;

    public override void Execute(CombatSystem system)
    {
        OnFirstCast();
        system.AddBloom(bloomCost);
        system.ApplyRevitalize(healPercentPerTurn, healDuration);
        system.ShowBattleText("The symbiote begins to mend your wounds...", 2f);

        system.TriggerItemAnimation(() =>
        {
            system.OnCoreComplete();
        });
    }
}