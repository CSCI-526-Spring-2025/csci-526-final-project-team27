using UnityEngine;
using System.Collections;

/// <summary>
/// 利用依赖注入调用各个行为接口，实现解耦的近战队友逻辑
/// </summary>
public class MeleeTeammate : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;            // 移动速度
    public float attackRange = 1.5f;          // 攻击范围
    public float searchInterval = 0.5f;       // 搜索目标的间隔

    [Header("Attack Settings")]
    public GameObject hitboxPrefab;         // 攻击时生成的 Hitbox 预制体
    public float hitboxOffset = 1.0f;         // Hitbox 生成时的偏移量
    public float attackCooldown = 1.0f;       // 攻击冷却时间

    public float damage = 10.0f;            // 攻击伤害

    private Transform currentTarget;
    private Rigidbody2D rb;
    private bool isAttacking = false;
    private bool canAttack = true;

    // 行为接口（可通过外部注入自定义实现）
    public ITargetFinder targetFinder;
    public IMover mover;
    public ITeammateMelee attacker;

    void Awake() 
    {
        rb = GetComponent<Rigidbody2D>();

        // 如果没有外部注入，则使用默认实现
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
        attacker = GetComponent<ITeammateMelee>();
        if (attacker == null) 
        {
            attacker = new TeammateMeleeAttacker();
        }

    }

    void Start() 
    {
        StartCoroutine(SearchTargetRoutine());
    }

    void Update() 
    {
        if (currentTarget == null) 
        {
            //rb.linearVelocity = Vector2.zero;
            // 无目标时向玩家移动
            mover.Move(transform, rb, GameObject.FindGameObjectWithTag("Player").transform, moveSpeed);
            return;
        }

        float distance = Vector2.Distance(transform.position, currentTarget.position);

        // 当目标在攻击范围内且处于可攻击状态时执行攻击
        if (distance <= attackRange && canAttack) 
        {
            StartCoroutine(PerformAttack());
        }
        else if (!isAttacking) 
        {
            // 不在攻击范围时进行移动
            mover.Move(transform, rb, currentTarget, moveSpeed);
            FaceTarget(currentTarget.position);
        }
    }

    void FixedUpdate() 
    {
        // 攻击期间确保刚体速度归零
        if (isAttacking) 
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    IEnumerator PerformAttack() 
    {
        canAttack = false;
        isAttacking = true;
        FaceTarget(currentTarget.position);

        // 调用攻击接口
        yield return StartCoroutine(attacker.Attack(this, transform, currentTarget, damage, hitboxPrefab, hitboxOffset, attackCooldown));

        isAttacking = false;
        canAttack = true;
    }

    // 使角色面向目标，未来可扩展成翻转Sprite或旋转角色
    void FaceTarget(Vector3 targetPos) 
    {
        // TODO: 添加翻转或旋转逻辑，目前仅为占位
    }

    // 定时搜索目标
    IEnumerator SearchTargetRoutine() 
    {
        while (true) 
        {
            currentTarget = targetFinder.FindTarget(transform);
            yield return new WaitForSeconds(searchInterval);
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
// using Unity.VisualScripting;

// public class MeleeTeammate : MonoBehaviour
// {
//     [Header("Movement Settings")]
//     public float moveSpeed = 3f;            // 移动速度
//     public float attackRange = 1.5f;        // 攻击范围（当与目标距离小于该值时开始攻击）
//     public float searchInterval = 0.5f;     // 定时搜索目标的间隔

//     [Header("Attack Settings")]
//     public GameObject hitboxPrefab;         // 攻击时生成的 Hitbox 预制体（内置伤害和击退逻辑）
//     public float hitboxOffset = 1.0f;       // Hitbox 生成时的偏移量（沿攻击方向）
//     public float attackCooldown = 1.0f;     // 两次攻击之间的间隔

//     private Transform currentTarget;        // 当前锁定的敌人目标（Tag 为 "Enemy"）
//     private Rigidbody2D rb;
//     private bool isAttacking = false;       // 攻击状态
//     private bool canAttack = true;          // 攻击冷却标志

//     public float damage = 10.0f;

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
//             // 没有目标时停止移动
//             rb.linearVelocity = Vector2.zero;
//             return;
//         }

//         float distance = Vector2.Distance(transform.position, currentTarget.position);

//         // 如果目标在攻击范围内且处于可攻击状态，则执行攻击
//         if (distance <= attackRange && canAttack)
//         {
//             StartCoroutine(Attack());
//         }
//         else if (!isAttacking)
//         {
//             // 如果目标不在攻击范围内，并且当前没有攻击，则向目标移动
//             MoveTowardsTarget();
//         }
//     }

//     void FixedUpdate()
//     {
//         // 攻击期间确保刚体速度归零，防止残留移动
//         if (isAttacking)
//         {
//             rb.linearVelocity = Vector2.zero;
//         }
//     }

//     // 攻击协程：生成 Hitbox 发动攻击，并等待冷却时间后恢复
//     IEnumerator Attack()
//     {
//         canAttack = false;
//         isAttacking = true;

//         // 面向目标
//         FaceTarget(currentTarget.position);

//         // 计算攻击方向和生成 Hitbox 的位置（沿攻击方向偏移）
//         Vector2 attackDirection = (currentTarget.position - transform.position).normalized;
//         Vector2 spawnPos = (Vector2)transform.position + attackDirection * hitboxOffset;
//         float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
//         Quaternion rotation = Quaternion.Euler(0, 0, angle);

//         // 实例化 Hitbox（Hitbox 脚本内包含伤害和击退的逻辑）
//         if (hitboxPrefab != null)
//         {
//             Instantiate(hitboxPrefab, spawnPos, rotation);
//         }

//         Health targetHealth = currentTarget.GetComponent<Health>();
//         if (targetHealth != null)
//         {
//             Debug.Log($"{this.gameObject.name} 攻击 {currentTarget.name} 造成 {damage} 点伤害"); 
//             targetHealth.TakeDamage(damage);
//         }
//         else
//         {
//             Debug.LogWarning("目标缺少 Health 组件，无法造成伤害");
//         }

//         // 等待攻击间隔后恢复
//         yield return new WaitForSeconds(attackCooldown);

//         isAttacking = false;
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
//         transform.rotation = Quaternion.Euler(0, 0, angle);
//         */

//         // flip x, 之后有素材了再加

//     }

//     // 定时搜索最近的敌人目标
//     IEnumerator SearchTargetRoutine()
//     {
//         while (true)
//         {
//             FindNearestEnemy();
//             yield return new WaitForSeconds(searchInterval);
//         }
//     }

//     // 搜索场景中所有 Tag 为 "Enemy" 的对象，选择距离自己最近的一个
//     void FindNearestEnemy()
//     {
//         GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
//         float minDistance = Mathf.Infinity;
//         Transform nearest = null;

//         foreach (GameObject enemy in enemies)
//         {
//             if (enemy == null)
//                 continue;

//             float d = Vector2.Distance(transform.position, enemy.transform.position);
//             if (d < minDistance)
//             {
//                 minDistance = d;
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
