using UnityEngine;

public abstract class SkillBase : ScriptableObject
{
    public string skillName;
    [Range(0, 100)] public int weight; // For probability logic

    public abstract void Execute(CombatSystem system, EnemyData user);
}