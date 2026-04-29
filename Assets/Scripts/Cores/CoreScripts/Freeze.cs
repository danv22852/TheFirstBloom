using UnityEngine;

[CreateAssetMenu(menuName = "Cores/Freeze")]
public class Freeze : CoreTemplate
{
    public override void Execute(CombatSystem system)
    {
        OnFirstCast();
        system.AddBloom(bloomCost);
        system.ShowBattleText("You freeze the enemy solid!", 1.5f);

        system.TriggerSkillAnimation(
            onHit: () =>
            {
                system.ApplyStun();
                system.TriggerVignette(VignetteType.Low);
            },
            onComplete: () => system.OnCoreComplete());
    }
}