using System;
using UnityEngine;

/// <summary>
/// ��ս��ɫ��������������ƹ��������к��ƶ��߼�
/// </summary>
public class MeleeCharacter2D_VonD : MonoBehaviour
{
    // ��ɫ����ö��
    public enum CharacterType { Enemy, Ally }
    public enum TargetPriority { PlayerFirst, AllyFirst, EnemyFirst }

    [Header("Base Settings")]
    [SerializeField] private CharacterType characterType = CharacterType.Enemy;
    [SerializeField] private TargetPriority priority = TargetPriority.PlayerFirst;

    [Header("Combat Settings")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private int damage = 15;
    [SerializeField] private LayerMask targetLayer;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;

    // �������
    private Rigidbody2D rb;
    private Transform currentTarget;
    private float lastAttackTime;

    // ģ�黯�ƶ���Ϊ�ӿ�����
    private IMovementBehavior movementBehavior;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        // ���Ի�ȡ�ѹ��ص��ƶ���Ϊ���
        movementBehavior = GetComponent<IMovementBehavior>();
        if (movementBehavior == null)
        {
            // ���û���ҵ�����Ĭ�����ֱ���ƶ���Ϊ���
            movementBehavior = gameObject.AddComponent<DirectMovementBehavior>();
        }
    }

    void Update()
    {
        FindTarget();
        CombatLogic();
        MovementLogic();
    }

    /// <summary>
    /// ��һ����Χ�ڲ���Ŀ�꣬�������ȼ�ѡ����ʵ�Ŀ��
    /// </summary>
    void FindTarget()
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, 10f, targetLayer);
        if (targets.Length == 0)
        {
            currentTarget = null;
            return;
        }

        currentTarget = priority switch
        {
            TargetPriority.PlayerFirst => Array.Find(targets, t => t.CompareTag("Player"))?.transform,
            TargetPriority.AllyFirst => Array.Find(targets, t => t.CompareTag("Ally"))?.transform,
            TargetPriority.EnemyFirst => Array.Find(targets, t => t.CompareTag("Enemy"))?.transform,
            _ => targets[0].transform
        };
    }

    /// <summary>
    /// ��Ŀ���ڹ�����Χ���ҹ�����ȴ����ʱ��ִ�й�������
    /// </summary>
    void CombatLogic()
    {
        if (currentTarget == null) return;

        if (Vector2.Distance(transform.position, currentTarget.position) <= attackRange)
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
    }

    /// <summary>
    /// �����ƶ���Ϊ���
    /// </summary>
    void MovementLogic()
    {
        if (currentTarget == null) return;
        movementBehavior.Move(currentTarget, rb, moveSpeed);
    }

    /// <summary>
    /// �����ΧĿ�꣬���Է���������Ŀ��ʩ���˺�
    /// </summary>
    void Attack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, targetLayer);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<Health>(out var health))
            {
                // ���ݽ�ɫ���͹�����Ч����Ŀ��
                bool isValidTarget = characterType switch
                {
                    CharacterType.Enemy => hit.CompareTag("Player") || hit.CompareTag("Ally"),
                    CharacterType.Ally => hit.CompareTag("Enemy"),
                    _ => false
                };

                if (isValidTarget)
                {
                    health.TakeDamage(damage);
                }
            }
        }
    }

    // �ڱ༭���п��ӻ�������Χ���������
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
