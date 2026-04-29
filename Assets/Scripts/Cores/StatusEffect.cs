public enum StatusEffectType { Burn, Poison, Weaken, Chilled }

[System.Serializable]
public class StatusEffect
{
    public StatusEffectType type;
    public int damage;
    public int turnsRemaining;
    public float weakenMultiplier = 1f;

    public StatusEffect(StatusEffectType type, int damage, int turnsRemaining, float weakenMultiplier = 1f)
    {
        this.type = type;
        this.damage = damage;
        this.turnsRemaining = turnsRemaining;
        this.weakenMultiplier = weakenMultiplier;
    }
}