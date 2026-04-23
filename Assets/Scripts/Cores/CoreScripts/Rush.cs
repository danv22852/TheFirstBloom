using UnityEngine;

[CreateAssetMenu(menuName = "Cores/Rush")]
public class Rush : CoreTemplate
{
    [Header("Damage Settings")]
    public int minDamage = 8;
    public int maxDamage = 14;

    public override void Execute(CombatSystem system)
    {
        OnFirstCast();
        system.AddBloom(bloomCost);
        system.ShowBattleText("You rush the enemy! You may take another turn.", 2f);

        system.TriggerSkillAnimation(
            onHit: () =>
            {
                int damage = Random.Range(minDamage, maxDamage + 1);
                damage = Mathf.RoundToInt(damage * system.GetBloomMultiplier());
                system.DealDamageToEnemy(damage);
                system.ShowBattleText("Quick Strike deals " + damage + " damage!", 2f);
                system.TriggerShake(true, 0.2f, 0.15f);
            },
            onComplete: () =>
            {
                if (system.IsEnemyDefeated())
                    system.OnCoreComplete();
                else
                    system.GivePlayerAnotherTurn();
            });
    }
}