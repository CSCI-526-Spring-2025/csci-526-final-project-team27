using System.Collections.Generic;
using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    [Header("��������")]
    public string displayName = "New Enemy";
    [Range(1, 1000)]
    public int baseHealth = 10;
    [Range(1, 1000)]
    public int baseDamage = 2;
    [Range(0.1f, 10f)]
    public float attackCooldown = 1f;
    [Range(0.1f, 100f)]
    public float moveSpeed = 3f;

    [Header("�Ѷ�����")]
    [Tooltip("�õ��˳��ֵ��Ѷ���ֵ")]
    [Range(0, 10)]
    public float difficultyThreshold = 1f;
    [Tooltip("����ͬ�Ѷ��µ�����Ȩ��")]
    public float spawnWeight = 1f; 
    public bool allowInNormal = true;
    public bool allowInElite = true;
    public bool allowInBoss;

    [Header("ս��ϵͳ")]
    public AttackPattern attackPattern;
    public List<EnemySkill> skills = new List<EnemySkill>();

    //// ��ʼ������������
    //public void Initialize()
    //{
    //    // ���ݵ�ǰ�������õ���״̬����Ϊ
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
