using UnityEngine;

[CreateAssetMenu(fileName = "Stab", menuName = "Cores/Sample Cores/Stab")]
public class Stab : CoreTemplate
{
    public float scaling = 1.5f;

    // public override void Execute(CombatSystem system)
    // {
    //     OnFirstCast(); // reveals bloom cost in UI if not already known

    //     system.currentBloom += bloomCost;
    //     system.UpdateBloomState();

    //     Debug.Log("Player uses " + coreName + "!\nBloom cost: " + bloomCost);

    //     system.StartCoroutine(system.PerformMeleeAttack(
    //         system.playerTransform,
    //         system.enemyTransform,
    //         onHit: () =>
    //         {
    //             var actualDamage = Mathf.Max(1, (Mathf.RoundToInt(system.playerStrength * scaling)));
    //             system.enemyHealth -= actualDamage;
    //             system.UpdateHealthUI();
    //             Debug.Log(coreName + " deals " + actualDamage + " damage!");
    //         },
    //         onComplete: () =>
    //         {
    //             system.CheckWinConditionOrContinue();
    //         }));
    // }
}