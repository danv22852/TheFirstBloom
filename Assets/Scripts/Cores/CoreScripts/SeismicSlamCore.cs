using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "SeismicSlamCore", menuName = "Cores/Symbiote/Seismic Slam")]
public class SeismicSlamCore : CoreTemplate 
{
    [Header("Damage Scaling Settings")]
    [Tooltip("Multiplies the player's Strength stat. 3.0x makes it a massive heavy hit.")]
    public float strengthMultiplier = 3.0f;
    
    [Tooltip("Adds a random +/- percentage to the final damage.")]
    public float damageVariance = 0.1f;

    public override void Execute(CombatSystem system)
    {
        OnFirstCast();

        // 1. Calculate base scaling damage
        float scalingDamage = system.playerStrength * strengthMultiplier;
        float variance = Random.Range(1f - damageVariance, 1f + damageVariance);
        int baseDamage = Mathf.RoundToInt(scalingDamage * variance);
        
        // 2. Apply Bloom buffs
        float statMultiplier = system.GetBloomMultiplier();
        int buffedDamage = Mathf.RoundToInt(baseDamage * statMultiplier);

        // 3. Armor Calculation
        int effectiveDefense = system.currentEnemy.defense;
        int finalDamage = Mathf.Max(1, buffedDamage - effectiveDefense);

        // 4. Pay the Bloom Cost
        system.AddBloom(bloomCost);

        // 5. Start the custom cinematic Coroutine!
        system.StartCoroutine(SeismicSlamRoutine(system, finalDamage));
    }

    // --- THE CUSTOM LEAP ANIMATION ---
    private IEnumerator SeismicSlamRoutine(CombatSystem system, int finalDamage)
    {
        // Grab the transforms from the Combat System
        Transform pTransform = system.playerTransform;
        Transform eTransform = system.enemyTransform;

        Vector3 startPos = pTransform.position;
        
        // Calculate the apex (high above the middle point) and the impact zone
        Vector3 apexPos = Vector3.Lerp(startPos, eTransform.position, 0.5f) + new Vector3(0, 3f, 0);
        Vector3 smashPos = eTransform.position + new Vector3((startPos.x < eTransform.position.x ? -1.5f : 1.5f), 0, 0);

        system.ShowBattleText("You leap into the air for a Seismic Slam!", 1.5f);

        // 1. Launch into the air
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 3.5f; 
            pTransform.position = Vector3.Lerp(startPos, apexPos, t);
            yield return null;
        }

        // 2. Hang time for dramatic effect
        yield return new WaitForSeconds(0.2f);

        // 3. Violent strike down!
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 12f; // Fall incredibly fast
            pTransform.position = Vector3.Lerp(apexPos, smashPos, t);
            yield return null;
        }

        // 4. IMPACT!
        system.DealDamageToEnemy(finalDamage);
        system.ShowBattleText("Seismic Slam crushes the enemy for " + finalDamage + " damage!", 2.5f);
        
        // Shake the camera (false) AND the enemy (true) to make it feel incredibly heavy
        system.TriggerShake(false, 0.6f, 0.4f);
        system.TriggerShake(true, 0.4f, 0.4f);

        // 5. Brief pause on the ground while the dust settles
        yield return new WaitForSeconds(0.6f);

        // 6. Return back to the starting line
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 4f;
            pTransform.position = Vector3.Lerp(smashPos, startPos, t);
            yield return null;
        }
        pTransform.position = startPos;

        // 7. END TURN (This is the crucial line that fixes the freeze!)
        system.OnCoreComplete();
    }

    // --- DYNAMIC UI HOVER TEXT ---
    public override string GetDynamicDescription(CombatSystem system)
    {
        if (system == null) return coreDescription;

        float scalingDamage = system.playerStrength * strengthMultiplier;
        int minDamage = Mathf.Max(1, Mathf.RoundToInt(scalingDamage * (1f - damageVariance)));
        int maxDamage = Mathf.Max(1, Mathf.RoundToInt(scalingDamage * (1f + damageVariance)));

        // Accurately reflect the enemy's armor in the preview text
        if (system.currentEnemy != null) 
        {
            minDamage = Mathf.Max(1, minDamage - system.currentEnemy.defense);
            maxDamage = Mathf.Max(1, maxDamage - system.currentEnemy.defense);
        }

        string damageText = (minDamage == maxDamage) ? minDamage.ToString() : $"{minDamage} - {maxDamage}";

        string text = coreDescription;
        text += "\nExpected Damage: " + damageText;
        if (bloomCost > 0) text += "\nCost: " + bloomCost + " Bloom";

        return text;
    }
}