using UnityEngine;

[CreateAssetMenu(fileName = "Stab", menuName = "Cores/Stab")]
public class Stab : CoreTemplate
{
    [Header("Damage Settings")]
    public int minDamage = 35;
    public int maxDamage = 45;

    public override void Execute(CombatSystem system)
    {
        OnFirstCast();
        system.AddBloom(bloomCost);
        system.ShowBattleText("You drive your symbiote into the enemy!", 1.5f);

        system.TriggerSkillAnimation(
            onHit: () =>
            {
                int damage = Random.Range(minDamage, maxDamage + 1);
                damage = Mathf.RoundToInt(damage * system.GetBloomMultiplier());
                system.DealDamageToEnemy(damage);
                system.ShowBattleText("Stab deals " + damage + " damage!", 2f);
                system.TriggerShake(true, 0.4f, 0.3f);
            },
            onComplete: () => system.OnCoreComplete());
    }
}