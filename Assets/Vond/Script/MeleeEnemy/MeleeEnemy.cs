using UnityEngine;
using System.Collections;

public class MeleeEnemy : MonoBehaviour
{
    public enum MoveMode { Aggressive, PlayerOnly }
    public MoveMode moveMode = MoveMode.Aggressive;   // 可在 Inspector 中配置

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float searchInterval = 1.0f;

    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public float attackInterval = 1.0f;
    public float damage = 10.0f;

    [Header("状态标志")]
    public bool isAttacking = false;

    public Transform currentTarget; // 当前锁定目标
    private Rigidbody2D rb;
    private bool canAttack = true;

    // 接口实例（均可在 Inspector 中注入自定义实现，否则在 Awake 中使用默认实现）
    public ITargetFinder targetFinder;
    public IMover mover;
    public IEnemyMelee meleeAttacker;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 若未注入，则使用默认实现
        if (targetFinder == null)
        {
            targetFinder = new NearestTeamFinder();
        }
        if (mover == null)
        {
            mover = new SimpleMover();
        }
        if (meleeAttacker == null)
        {
            meleeAttacker = new EnemyMeleeAttacker();
        }

        StartCoroutine(SearchTargetRoutine());
    }

    void Update()
    {
        if (currentTarget != null && canAttack)
        {
            float distance = Vector2.Distance(transform.position, currentTarget.position);
            if (distance <= attackRange)
            {
                FaceTarget(currentTarget.position);
                StartCoroutine(PerformAttack());
            }
        }
    }

    void FixedUpdate()
    {
        if (isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (currentTarget != null)
        {
            mover.Move(transform, rb, currentTarget, moveSpeed);
        }
    }

    // 定时更新目标
    IEnumerator SearchTargetRoutine()
    {
        while (true)
        {
            currentTarget = targetFinder.FindTarget(transform);
            yield return new WaitForSeconds(searchInterval);
        }
    }

    IEnumerator PerformAttack()
    {
        canAttack = false;
        isAttacking = true;
        yield return StartCoroutine(meleeAttacker.Attack(this, transform, currentTarget, damage, attackInterval));
        isAttacking = false;
        canAttack = true;
    }

    // 使敌人面向目标
    void FaceTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // 外部调用：当目标消失或死亡时清空当前目标
    public void ClearTarget(Transform target)
    {
        if (currentTarget == target)
        {
            currentTarget = null;
        }
    }
}

// using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;

// public class MeleeEnemy : MonoBehaviour
// {
//     public enum MoveMode { Aggressive, PlayerOnly } // 行动模式
//     public MoveMode moveMode = MoveMode.Aggressive;   // 默认行为模式

//     [Header("Movement Settings")]
//     public float moveSpeed = 3f;          // 移动速度
//     public float searchInterval = 1.0f;   // 目标搜索间隔（可配置）

//     [Header("Attack Settings")]
//     [Tooltip("当敌人与目标距离小于该值时发起攻击")]
//     public float attackRange = 1.5f;
//     [Tooltip("两次攻击之间的间隔")]
//     public float attackInterval = 1.0f;
//     public float damage = 10.0f;

//     private Transform playerTransform;                      // 存储 Player 目标
//     private List<Transform> teammates = new List<Transform>(); // 存储 Teammate 目标
//     public Transform currentTarget;                         // 当前锁定的目标
//     private Rigidbody2D rb;
//     public bool isAttacking = false;                        // 攻击状态标志

//     private bool canAttack = true;                          // 攻击冷却标志

//     void Start()
//     {
//         rb = GetComponent<Rigidbody2D>();
//         CacheTargets(); // 一次性缓存所有目标
//         StartCoroutine(SearchTargetRoutine()); // 定期检查最近目标
//     }

//     void Update()
//     {
//         // 攻击逻辑：如果存在目标且冷却允许，则检测距离发起攻击
//         if (currentTarget != null && canAttack)
//         {
//             float distance = Vector2.Distance(transform.position, currentTarget.position);
//             if (distance <= attackRange)
//             {
//                 FaceTarget(currentTarget.position);
//                 StartCoroutine(Attack());
//             }
//         }
//     }

//     void FixedUpdate()
//     {
//         // 攻击期间停止移动
//         if (isAttacking)
//         {
//             rb.linearVelocity = Vector2.zero;
//             return;
//         }

//         if (currentTarget != null)
//         {
//             MoveTowardsTarget();
//         }
//     }

//     // 缓存所有 Player 和 Teammate（只执行一次）
//     void CacheTargets()
//     {
//         // 存储唯一的 Player
//         GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
//         if (playerObj)
//             playerTransform = playerObj.transform;

//         // 存储所有 Teammate
//         teammates.Clear();
//         foreach (GameObject teammateObj in GameObject.FindGameObjectsWithTag("Teammate"))
//         {
//             teammates.Add(teammateObj.transform);
//         }

//         // 默认锁定最近目标
//         FindNearestTarget();
//     }

//     // 定期重新检查最近目标
//     IEnumerator SearchTargetRoutine()
//     {
//         while (true)
//         {
//             FindNearestTarget();
//             yield return new WaitForSeconds(searchInterval);
//         }
//     }

//     // 搜索场景中最近的目标
//     void FindNearestTarget()
//     {
//         // 如果仅追踪 Player，则直接使用 Player 目标
//         if (moveMode == MoveMode.PlayerOnly)
//         {
//             currentTarget = playerTransform;
//             return;
//         }

//         float minDistance = Mathf.Infinity;
//         Transform nearestTarget = playerTransform; // 默认先设为 Player

//         // 检查所有 Teammate 的距离
//         foreach (Transform teammate in teammates)
//         {
//             if (teammate == null)
//                 continue; // 防止已销毁对象

//             float distance = Vector2.Distance(transform.position, teammate.position);
//             if (distance < minDistance)
//             {
//                 minDistance = distance;
//                 nearestTarget = teammate;
//             }
//         }

//         if (playerTransform != null)
//         {
//             // 检查 Player 距离
//             float playerDistance = Vector2.Distance(transform.position, playerTransform.position);
//             if (playerDistance < minDistance)
//             {
//                 nearestTarget = playerTransform;
//             }
//         }

//         currentTarget = nearestTarget;
//     }

//     // 向当前目标移动
//     void MoveTowardsTarget()
//     {
//         Vector2 direction = (currentTarget.position - transform.position).normalized;
//         rb.linearVelocity = direction * moveSpeed;
//     }

//     // 攻击协程：直接对目标造成伤害（无需生成 Hitbox）
//     IEnumerator Attack()
//     {
//         canAttack = false;
//         isAttacking = true;

//         Health targetHealth = currentTarget.GetComponent<Health>();
//         if (targetHealth != null)
//         {
//             targetHealth.TakeDamage(damage);
//         }
//         else
//         {
//             Debug.LogWarning("目标缺少 Health 组件，无法造成伤害");
//         }

//         // 等待攻击间隔后恢复攻击状态
//         yield return new WaitForSeconds(attackInterval);
//         isAttacking = false;
//         canAttack = true;
//     }

//     // 使敌人面向指定目标（旋转 Z 轴）
//     void FaceTarget(Vector3 targetPosition)
//     {
//         Vector3 direction = (targetPosition - transform.position).normalized;
//         float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
//         transform.rotation = Quaternion.Euler(0, 0, angle);
//     }

//     // 当目标消失或死亡时，清空目标
//     public void ClearTarget(Transform target)
//     {
//         if (currentTarget == target)
//         {
//             currentTarget = null;
//         }
//     }
// }
