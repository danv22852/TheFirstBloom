using UnityEngine;

[CreateAssetMenu(fileName = "StagStomp", menuName = "Enemies/Skills/Stag Stomp")]
public class StagStomp : SkillBase
{
    public int damagePerHit = 5; // Low damage per hit

    public override void Execute(CombatSystem system, EnemyData user)
    {
        int hitCount = Random.Range(2, 5); // 2 to 4 hits
        Debug.Log(user.enemyName + " uses Stomp! Hits: " + hitCount);

        for (int i = 0; i < hitCount; i++)
        {
            Debug.Log("Stomp hit " + (i + 1) + ", deals " + damagePerHit + " damage.");
            system.DealDamageToPlayer(damagePerHit, ignoreDefense: false);
        }
    }
}