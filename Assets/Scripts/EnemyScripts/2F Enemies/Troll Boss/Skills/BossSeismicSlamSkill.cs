using UnityEngine;

[CreateAssetMenu(fileName = "BossSeismicSlam", menuName = "Enemies/Skills/Troll/Boss Seismic Slam")]
public class BossSeismicSlamSkill : SkillBase
{
    public int damageAmount = 40; // Strength + 10 for Phase 2!

    public override void Execute(CombatSystem system, EnemyData user)
    {
        system.DealDamageToPlayer(damageAmount, ignoreDefense: false);
        // Heavier screen shake for the Phase 2 slam
        system.TriggerShake(false, 0.6f, 0.5f);
    }
}