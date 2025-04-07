using UnityEngine;
using System.Collections;

public class WanderingEnemy : BaseEnemy
{
    [Header("游荡设置")]

    public float wanderRange = 5f;      // 用于设置 RandomWanderTargetFinder 的范围
    // 攻击相关设置
    [Header("攻击设置")]
    public float damage = 5f;


    // 接口实例，均可在 Inspector 中注入自定义实现
    public IMover mover;
    public ITargetFinder targetFinder;
    public IEnemyMelee meleeAttacker;

    private Rigidbody2D rb;
    private bool canAttack = true;

    // ===== 新增变量：用于检测敌人是否卡住 =====
    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    public float stuckThreshold = 1.0f;    // 1秒内几乎没移动，就认为卡住
    public float minMoveDistance = 0.1f;   // 两帧之间移动距离小于此值认为没移动
    // ============================================

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 使用默认的 SimpleMover 实现移动
        if (mover == null)
            mover = new SimpleMover();

        // 使用随机目标查找器实现游荡行为
        if (targetFinder == null)
        {
            targetFinder = new RandomWanderFinder(transform);
            // 如果需要，可调整游荡范围
            if (targetFinder is RandomWanderFinder wanderFinder)
            {
                wanderFinder.wanderRange = wanderRange;
            }
        }

        // 使用已有的 EnemyMeleeAttacker 实现攻击行为
        if (meleeAttacker == null)
            meleeAttacker = new EnemyMeleeAttacker();
    }

    void FixedUpdate()
    {
        // 获取当前随机目标，并向其移动
        Transform wanderTarget = targetFinder.FindTarget(transform);
        mover.Move(transform, rb, wanderTarget, moveSpeed);

        // ===== 新增：检测敌人是否卡住 =====
        if (Vector3.Distance(transform.position, lastPosition) < minMoveDistance)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer >= stuckThreshold)
            {
                // 如果卡住，则更新游荡目标
                if (targetFinder is RandomWanderFinder wanderFinder)
                {
                    wanderFinder.UpdateWanderTarget(transform.position);
                }
                stuckTimer = 0f;  // 重置计时器
            }
        }
        else
        {
            stuckTimer = 0f;  // 如果移动正常则重置计时器
        }
        lastPosition = transform.position;
        // ==================================
    }

    // 碰撞触发时，如果碰到玩家或队友则造成伤害
    void OnCollisionEnter2D(Collision2D collision)
    {
        if ((collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Teammate")) && canAttack)
        {
            Health targetHealth = collision.gameObject.GetComponent<Health>();
            if (targetHealth != null)
            {
                StartCoroutine(DealDamageRoutine(collision.gameObject.transform));
            }
        }
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (targetFinder is RandomWanderFinder wanderFinder)
            {
                wanderFinder.ForceUpdateTarget(transform.position);
            }
        }
    }

    IEnumerator DealDamageRoutine(Transform target)
    {
        canAttack = false;
        // 使用已实现的攻击逻辑对目标造成伤害
        yield return StartCoroutine(meleeAttacker.Attack(this, transform, target, damage, attackCooldown));
        canAttack = true;
    }
}
