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
    public List<SkillBase> skills;
}