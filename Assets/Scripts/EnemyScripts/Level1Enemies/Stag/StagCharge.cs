using UnityEngine;

[CreateAssetMenu(fileName = "StagCharge", menuName = "Enemies/Skills/Stag Charge")]
public class StagCharge : SkillBase
{
    public int damageAmount = 18; // Medium damage, bypasses defense

    public override void Execute(CombatSystem system, EnemyData user)
    {
        Debug.Log(user.enemyName + " uses Charge!");
        system.DealDamageToPlayer(damageAmount, ignoreDefense: true);
        Debug.Log("Charge deals " + damageAmount + " damage, piercing through defense!");
    }
}
