using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class RangedTeammate : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;           // 追踪敌人的移动速度
    public float detectionInterval = 0.5f; // 定时检测敌人目标的间隔
    public float attackRange = 7f;         // 当目标进入此距离时开始攻击

    [Header("Attack Settings")]
    public float attackInterval = 1.5f;    // 两次攻击之间的间隔
    public GameObject bulletPrefab;        // 子弹预制体
    public Transform firePoint;            // 子弹发射点
    public float bulletSpeed = 10f;        // 子弹飞行速度
    public float bulletLifetime = 2f;      // 子弹最大存在时间

    // -------添加跟随设置 - 新增部分---------
    [Header("Follow Settings")]
    private float followDistance = 2.0f;   // 跟随玩家的距离
    public bool shouldFollowPlayer = true; // 是否应该跟随玩家
    //---------------------

    private Transform currentTarget;       // 当前锁定的目标
    private Rigidbody2D rb;
    private bool canAttack = true;         // 攻击冷却标志

    // 复用已有接口：目标查找和移动逻辑
    public ITargetFinder targetFinder;
    public IMover mover;
    // 新增远程攻击接口
    public IRangedAttacker rangedAttacker;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // 如果外部未注入，则使用默认实现
        targetFinder = GetComponent<ITargetFinder>();
        if (targetFinder == null)
        {
            targetFinder = new NearestEnemyFinder();
        }
        
        mover = GetComponent<IMover>();
        if (mover == null)
        {
            mover = new SimpleMover();
        }
        
        rangedAttacker = GetComponent<IRangedAttacker>();
        if (rangedAttacker == null)
        {
            rangedAttacker = new RangedAttacker();
        }
    }

    void Start()
    {
        StartCoroutine(SearchTargetRoutine());
    }

    void Update()
    {
        // 添加避免其他队友的逻辑 - 新增部分
        AvoidOtherTeammates();
        
        if (currentTarget == null)
        {
            if (shouldFollowPlayer)
            {
                Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
                float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
                if (distToPlayer > followDistance)
                {
                    mover.Move(transform, rb, playerTransform, moveSpeed);
                }
                else
                {
                    rb.linearVelocity = Vector2.zero;
                }
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }
            return;
        }

        float distance = Vector2.Distance(transform.position, currentTarget.position);
        if (distance <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            if (canAttack)
            {
                StartCoroutine(PerformAttack());
            }
        }
        else
        {
            MoveTowardsTarget();
        }
    }

    IEnumerator PerformAttack()
    {
        canAttack = false;
        yield return StartCoroutine(rangedAttacker.Attack(this, transform, currentTarget, attackInterval, bulletPrefab, firePoint, bulletSpeed, bulletLifetime));
        canAttack = true;
    }

    void MoveTowardsTarget()
    {
        mover.Move(transform, rb, currentTarget, moveSpeed);
    }


    // 定时搜索最近的敌人目标
    IEnumerator SearchTargetRoutine()
    {
        while (true)
        {
            // 直接调用 NearestEnemyFinder 的 FindTarget 方法
            currentTarget = targetFinder.FindTarget(transform);
            yield return new WaitForSeconds(detectionInterval);
        }
    }

    void AvoidOtherTeammates()
    {
        Vector2 pushDir = Vector2.zero;
        int nearbyCount = 0;

        foreach (var other in FindObjectsOfType<RangedTeammate>())
        {
            if (other == this) continue;

            float dist = Vector2.Distance(transform.position, other.transform.position);
            if (dist < followDistance) // 根据体型调整这个距离
            {
                Vector2 away = (transform.position - other.transform.position).normalized / dist;
                pushDir += away;
                nearbyCount++;
            }
        }

        if (nearbyCount > 0)
        {
            Vector2 avgPush = pushDir / nearbyCount;
            transform.position += (Vector3)(avgPush * 0.05f);  
        }
    }

    // 队友销毁时从管理器中移除
    void OnDestroy()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            TeammateManager teammateManager = player.GetComponent<TeammateManager>();
            if (teammateManager != null)
            {
                teammateManager.RemoveTeammate(gameObject);
            }
        }
    }
}

// using UnityEngine;
// using System.Collections;

// [RequireComponent(typeof(Rigidbody2D))]
// public class RangedTeammate : MonoBehaviour
// {
//     [Header("Movement Settings")]
//     public float moveSpeed = 3f;           // 追踪敌人的移动速度
//     public float detectionInterval = 0.5f; // 定时检测敌人目标的间隔
//     public float attackRange = 7f;         // 当目标进入此距离时开始攻击

//     [Header("Attack Settings")]
//     public float attackInterval = 1.5f;    // 两次攻击之间的间隔
//     public GameObject bulletPrefab;        // 子弹预制体（可复用已有的子弹逻辑）
//     public Transform firePoint;            // 子弹发射点（通常设置在角色手部或枪口位置）
//     public float bulletSpeed = 10f;        // 子弹飞行速度
//     public float bulletLifetime = 2f;      // 子弹最大存在时间

//     private Transform currentTarget;       // 当前锁定的目标（最近的敌人）
//     private Rigidbody2D rb;
//     private bool canAttack = true;         // 攻击冷却标志

//     void Start()
//     {
//         rb = GetComponent<Rigidbody2D>();
//         // 定时搜索最近的敌人目标
//         StartCoroutine(SearchTargetRoutine());
//     }

//     void Update()
//     {
//         if (currentTarget == null)
//         {
//             // 如果暂时没有目标，则停止运动
//             rb.linearVelocity = Vector2.zero;
//             return;
//         }

//         float distance = Vector2.Distance(transform.position, currentTarget.position);
//         // 当敌人进入攻击范围时
//         if (distance <= attackRange)
//         {
//             rb.linearVelocity = Vector2.zero;  // 停止移动
//             FaceTarget(currentTarget.position);
//             if (canAttack)
//             {
//                 StartCoroutine(Attack());
//             }
//         }
//         else // 敌人不在攻击范围内时，向目标移动
//         {
//             MoveTowardsTarget();
//         }
//     }

//     // 攻击协程：生成子弹并等待攻击冷却
//     IEnumerator Attack()
//     {
//         canAttack = false;
//         // 计算从 firePoint 到目标的方向
//         Vector2 attackDirection = (currentTarget.position - firePoint.position).normalized;
//         float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
//         Quaternion rotation = Quaternion.Euler(0, 0, angle);
        
//         // 生成子弹
//         GameObject bullet = Instantiate(bulletPrefab, firePoint.position, rotation);
//         Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
//         if (bulletRb != null)
//         {
//             bulletRb.linearVelocity = attackDirection * bulletSpeed;
//         }
//         Destroy(bullet, bulletLifetime);

//         // 等待下一次攻击
//         yield return new WaitForSeconds(attackInterval);
//         canAttack = true;
//     }

//     // 向当前目标移动
//     void MoveTowardsTarget()
//     {
//         Vector2 direction = (currentTarget.position - transform.position).normalized;
//         rb.linearVelocity = direction * moveSpeed;
//         FaceTarget(currentTarget.position);
//     }

//     // 使角色面向目标
//     void FaceTarget(Vector3 targetPos)
//     {
//         /*
//         Vector2 direction = (targetPos - transform.position).normalized;
//         float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
//         transform.rotation = Quaternion.Euler(0, 0, angle);*/

//     }

//     // 定时搜索最近的敌人目标
//     IEnumerator SearchTargetRoutine()
//     {
//         while (true)
//         {
//             FindNearestEnemy();
//             yield return new WaitForSeconds(detectionInterval);
//         }
//     }

//     // 搜索场景中所有 Tag 为 "Enemy" 的对象，找到距离自己最近的敌人
//     void FindNearestEnemy()
//     {
//         GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
//         float minDistance = Mathf.Infinity;
//         Transform nearest = null;

//         foreach (GameObject enemy in enemies)
//         {
//             // 如果敌人被销毁了，则跳过
//             if (enemy == null) continue;

//             float distance = Vector2.Distance(transform.position, enemy.transform.position);
//             if (distance < minDistance)
//             {
//                 minDistance = distance;
//                 nearest = enemy.transform;
//             }
//         }

//         currentTarget = nearest;
//     }

//     //修改OnDestroy函数, 使得队友死亡时从队友列表中移除
//     void OnDestroy()
//     {
//         //找到当前Scene中的Player对象
//         GameObject player = GameObject.FindGameObjectWithTag("Player");
//         if (player != null)
//         {
//             TeammateManager teammateManager = player.GetComponent<TeammateManager>();
//             if (teammateManager != null)
//             {
//                 teammateManager.RemoveTeammate(gameObject);
//             }
//         }
//     }
// }
