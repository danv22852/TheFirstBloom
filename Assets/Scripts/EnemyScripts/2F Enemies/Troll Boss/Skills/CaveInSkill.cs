using UnityEngine;

[CreateAssetMenu(fileName = "CaveIn", menuName = "Enemies/Skills/Troll/Cave-In")]
public class CaveInSkill : SkillBase
{
    public int damageAmount = 30; // Matches Troll's base strength

    public override void Execute(CombatSystem system, EnemyData user)
    {
        // We handle the text and damage here when the attack connects!
        system.DealDamageToPlayer(damageAmount, ignoreDefense: false);
        system.TriggerShake(false, 0.4f, 0.3f);
    }
}