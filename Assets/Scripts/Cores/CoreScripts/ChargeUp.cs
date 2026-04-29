using UnityEngine;

[CreateAssetMenu(menuName = "Cores/Charge Up")]
public class ChargeUp : CoreTemplate
{
    public override void Execute(CombatSystem system)
    {
        OnFirstCast();
        system.AddBloom(bloomCost);
        system.ShowBattleText("You charge up your next attack!", 1.5f);

        system.TriggerItemAnimation(() =>
        {
            system.ApplyChargeUp();
            system.ShowBattleText("Next move will deal double damage!", 2f);
            system.OnCoreComplete();
        });
    }
}