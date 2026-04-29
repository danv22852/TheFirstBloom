using UnityEngine;

[CreateAssetMenu(menuName = "Cores/Execution")]
public class Execution : CoreTemplate
{
    [Header("Damage Settings")]
    public int minDamage = 15;
    public int maxDamage = 22;

    [Header("Execution Settings")]
    [Range(0f, 1f)]
    public float executeThreshold = 0.25f; // triggers bonus below 25% HP
    public int bonusDamage = 40;

    public override void Execute(CombatSystem system)
    {
        OnFirstCast();
        system.AddBloom(bloomCost);

        bool isExecuteRange = system.IsEnemyBelowThreshold(executeThreshold);

        if (isExecuteRange)
            system.ShowBattleText("The enemy is vulnerable — finish them!", 1.5f);
        else
            system.ShowBattleText("You strike at the enemy!", 1.5f);

        system.TriggerSkillAnimation(
            onHit: () =>
            {
                int damage = Random.Range(minDamage, maxDamage + 1);
                damage = Mathf.RoundToInt(damage * system.GetBloomMultiplier());

                if (isExecuteRange)
                {
                    damage += bonusDamage;
                    system.ShowBattleText("EXECUTE! Deals " + damage + " damage!", 2f);
                    system.TriggerVignette(VignetteType.High);
                }
                else
                {
                    system.ShowBattleText("Execute deals " + damage + " damage.", 2f);
                }

                system.DealDamageToEnemy(damage);
                system.TriggerShake(true, 0.4f, 0.3f);
            },
            onComplete: () => system.OnCoreComplete());
    }
}