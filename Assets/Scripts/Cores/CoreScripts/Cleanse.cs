using UnityEngine;

[CreateAssetMenu(menuName = "Cores/Cleanse")]
public class Cleanse : CoreTemplate
{
    public override void Execute(CombatSystem system)
    {
        OnFirstCast();
        system.AddBloom(bloomCost);

        system.TriggerItemAnimation(() =>
        {
            int removed = system.CleansePlayerEffects();
            if (removed > 0)
                system.ShowBattleText("You purge " + removed + " status effect(s) from yourself!", 2f);
            else
                system.ShowBattleText("Nothing to cleanse.", 1.5f);

            system.OnCoreComplete();
        });
    }
}