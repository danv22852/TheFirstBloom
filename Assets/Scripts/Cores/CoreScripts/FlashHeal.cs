using UnityEngine;

[CreateAssetMenu(menuName = "Cores/Flash Heal")]
public class FlashHeal : CoreTemplate
{
    [Header("Heal Settings")]
    public int healAmount = 30;

    public override void Execute(CombatSystem system)
    {
        OnFirstCast();
        system.AddBloom(bloomCost);
        system.ShowBattleText("A burst of energy surges through you!", 1.5f);

        system.TriggerItemAnimation(() =>
        {
            system.HealPlayer(healAmount);
            system.ShowBattleText("Flash Heal restores " + healAmount + " HP!", 2f);
            system.OnCoreComplete();
        });
    }
}