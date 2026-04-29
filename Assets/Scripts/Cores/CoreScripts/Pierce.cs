using UnityEngine;

[CreateAssetMenu(menuName = "Cores/Pierce")]
public class Pierce : CoreTemplate
{
    [Header("Damage Settings")]
    public int minDamage = 18;
    public int maxDamage = 25;

    public override void Execute(CombatSystem system)
    {
        OnFirstCast();
        system.AddBloom(bloomCost);
        system.ShowBattleText("You strike through their defenses!", 1.5f);

        system.TriggerSkillAnimation(
            onHit: () =>
            {
                int damage = Random.Range(minDamage, maxDamage + 1);
                damage = Mathf.RoundToInt(damage * system.GetBloomMultiplier());
                system.DealDamageToEnemyIgnoreDefense(damage);
                system.ShowBattleText("Pierce deals " + damage + " damage! Defense ignored!", 2f);
                system.TriggerShake(true, 0.3f, 0.2f);
            },
            onComplete: () =>
            {
                system.OnCoreComplete();
            });
    }
}