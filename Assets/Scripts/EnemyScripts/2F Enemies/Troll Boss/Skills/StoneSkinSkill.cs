using UnityEngine;

[CreateAssetMenu(fileName = "StoneSkin", menuName = "Enemies/Skills/Troll/Stone Skin")]
public class StoneSkinSkill : SkillBase
{
    // No damage variable needed for a shield!
    
    public override void Execute(CombatSystem system, EnemyData user)
    {
        // This physically turns on the 5x defense multiplier in your CombatSystem!
        system.isBossGuarding = true; 
    }
}