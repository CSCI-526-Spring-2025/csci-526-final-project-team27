using System;
using UnityEngine;

/// <summary>
/// 近战角色控制主组件，控制攻击、索敌和移动逻辑
/// </summary>
public class MeleeCharacter2D_VonD : MonoBehaviour
{
    // 角色类型枚举
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

    // 组件引用
    private Rigidbody2D rb;
    private Transform currentTarget;
    private float lastAttackTime;

    // 模块化移动行为接口引用
    private IMovementBehavior movementBehavior;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        // 尝试获取已挂载的移动行为组件
        movementBehavior = GetComponent<IMovementBehavior>();
        if (movementBehavior == null)
        {
            // 如果没有找到，则默认添加直接移动行为组件
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
    /// 在一定范围内查找目标，根据优先级选择合适的目标
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
    /// 当目标在攻击范围内且攻击冷却结束时，执行攻击操作
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
    /// 调用移动行为组件
    /// </summary>
    void MovementLogic()
    {
        if (currentTarget == null) return;
        movementBehavior.Move(currentTarget, rb, moveSpeed);
    }

    /// <summary>
    /// 检测周围目标，并对符合条件的目标施加伤害
    /// </summary>
    void Attack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, targetLayer);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<Health>(out var health))
            {
                // 根据角色类型过滤有效攻击目标
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

    // 在编辑器中可视化攻击范围，方便调试
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
