using UnityEngine;

[CreateAssetMenu(fileName = "StagBlock", menuName = "Enemies/Skills/Stag Block")]
public class StagBlock : SkillBase
{
    public int speedBonus = 4;

    public override void Execute(CombatSystem system, EnemyData user)
    {
        Debug.Log(user.enemyName + " uses Block!");

        // optional speed boost if we want to try that
        // system.ModifyEnemySpeed(speedBonus);

        system.EndEnemyTurn();
    }
}