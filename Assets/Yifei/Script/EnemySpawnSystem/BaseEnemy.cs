using System.Collections.Generic;
using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    [Header("基础属性")]
    public string displayName = "New Enemy";
    [Range(1, 1000)]
    public int baseHealth = 10;
    [Range(1, 1000)]
    public int baseDamage = 2;
    [Range(0.1f, 10f)]
    public float attackCooldown = 1f;
    [Range(0.1f, 100f)]
    public float moveSpeed = 3f;

    [Header("难度设置")]
    [Tooltip("该敌人出现的难度阈值")]
    [Range(0, 10)]
    public float difficultyThreshold = 1f;
    [Tooltip("在相同难度下的生成权重")]
    public float spawnWeight = 1f; 
    public bool allowInNormal = true;
    public bool allowInElite = true;
    public bool allowInBoss;

    [Header("战斗系统")]
    public AttackPattern attackPattern;
    public List<EnemySkill> skills = new List<EnemySkill>();

    //// 初始化或其他方法
    //public void Initialize()
    //{
    //    // 根据当前属性设置敌人状态或行为
    //}
}

public enum AttackPattern
{
    Melee,
    Ranged,
}

[System.Serializable]
public class EnemySkill
{
    public string skillName;
    public float cooldown;
    public GameObject[] effectPrefabs;
}
