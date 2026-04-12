using UnityEngine;

[CreateAssetMenu(fileName = "Stab", menuName = "Cores/Sample Cores/Stab")]
public class Stab : CoreTemplate
{
    public float scaling = 1.5f;

    public override void Execute(CombatSystem system)
    {
        OnFirstCast();
        system.AddBloom(bloomCost);

        int damage = Mathf.Max(1, Mathf.RoundToInt(system.GetPlayerStrength() * scaling));

        system.TriggerSkillAnimation(
            onHit: () =>
            {
                system.DealDamageToEnemy(damage);
                system.ShowBattleText(coreName + " deals " + damage + " damage!", 2f);
                system.TriggerShake(true, 0.2f, 0.15f);
            },
            onComplete: () =>
            {
                system.OnCoreComplete();
            });
    }

    private System.Collections.IEnumerator ExecuteRoutine(CombatSystem system, int damage)
    {
        system.TriggerSkillAnimation(
            onHit: () =>
            {
                system.DealDamageToEnemy(damage);
                system.ShowBattleText(coreName + " deals " + damage + " damage!", 2f);
                system.TriggerShake(true, 0.2f, 0.15f);
            },
            onComplete: () =>
            {
                system.OnCoreComplete();
            });

        yield break;
    }
}