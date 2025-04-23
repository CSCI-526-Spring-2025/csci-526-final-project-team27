using UnityEngine;
using System.Collections;

public class MeleeEnemy : BaseEnemy
{
    public enum MoveMode { Aggressive, PlayerOnly }
    public MoveMode moveMode = MoveMode.Aggressive;   // 可在 Inspector 中配置

    [Header("Movement Settings")]
    public float searchInterval = 1.0f;

    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public float attackInterval = 1.0f;
    public float damage = 10.0f;

    [Header("状态标志")]
    public bool isAttacking = false;

    public Transform currentTarget; // 当前锁定目标
    private Rigidbody2D rb;
    public bool canAttack = true;
    public bool attackDisabledBySkill = false;

    // 接口实例（均可在 Inspector 中注入自定义实现，否则在 Awake 中使用默认实现）
    public ITargetFinder targetFinder;
    public IMover mover;
    public IEnemyMelee meleeAttacker;

    [Header("Anim")]
    public SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 若未注入，则使用默认实现
        targetFinder = GetComponent<ITargetFinder>();
        if (targetFinder == null)
        {
            targetFinder = new NearestTeamFinder();
        }
        mover = GetComponent<IMover>();
        if (mover == null)
        {
            mover = new SimpleMover();
        }
        meleeAttacker = GetComponent<IEnemyMelee>();
        if (meleeAttacker == null)
        {
            meleeAttacker = new EnemyMeleeAttacker();
        }

        StartCoroutine(SearchTargetRoutine());
    }

    void Update()
    {
        if(currentTarget != null)
        {
            FaceTarget(currentTarget.position);
        }

        if (currentTarget != null && canAttack && !attackDisabledBySkill)
        {
            float distance = Vector2.Distance(transform.position, currentTarget.position);
            if (distance <= attackRange)
            {
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
        /*
        Vector3 direction = (targetPosition - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);*/
        if(spriteRenderer != null)
        {
            Vector2 direction = (targetPosition - transform.position).normalized;
            if (direction.x > 0)
            {
                spriteRenderer.flipX = false; // 朝右
            }
            else if (direction.x < 0)
            {
                spriteRenderer.flipX = true; // 朝左
            }
        }
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