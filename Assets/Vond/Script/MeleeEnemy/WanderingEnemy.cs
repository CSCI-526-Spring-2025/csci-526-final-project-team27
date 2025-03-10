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
    }

    IEnumerator DealDamageRoutine(Transform target)
    {
        canAttack = false;
        // 使用已实现的攻击逻辑对目标造成伤害
        yield return StartCoroutine(meleeAttacker.Attack(this, transform, target, damage, attackCooldown));
        canAttack = true;
    }
}
