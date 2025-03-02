using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Enemy")]
public class EnemyConfig : ScriptableObject
{
    // 需要怪物脚本中有相应的初始化方法

    [Header("预制体")]
    public GameObject prefab;

    [Header("基础属性")]
    public string displayName = "New Enemy";
    [Range(1, 1000)] public int baseHealth = 10;
    [Range(1, 1000)] public int baseDamage = 2;
    [Range(0.1f, 10f)] public float attackCooldown = 1f;
    [Range(0.1f, 10f)] public float moveSpeed = 3f;

    [Header("难度设置")]
    [Tooltip("该敌人出现的难度阈值")]
    [Range(0, 10)] public float difficultyThreshold = 1f;
    [Tooltip("在相同难度下的生成权重")]
    public float spawnWeight = 1f;
    // 可能不需要
    public bool allowInNormal = true;
    public bool allowInElite = true;
    public bool allowInBoss;

    [Header("战斗系统")] // 可能不需要
    public AttackPattern attackPattern;
    public List<EnemySkill> skills = new List<EnemySkill>();

}

public enum AttackPattern
{
    // 不一定用得上
    Melee,
    Ranged,
}

[System.Serializable]
public class EnemySkill
{
    // 不一定用得上
    public string skillName;
    public float cooldown;
    public GameObject[] effectPrefabs;
}