using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{

    public string enemyID;
    public string enemyName;
    public Sprite enemySprite;
    public int maxHP;
    public int strength;
    public int speed;
    public int defense;
    public float combatScale = 1.0f;
    public bool flipSprite;
    public float hoverHeight = 0f;
    public int expDrop; 
    public List<SkillBase> skills;
}