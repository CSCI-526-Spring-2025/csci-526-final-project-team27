using UnityEngine;
using System.Collections;

public class RangedFanSprayEnemy : BaseEnemy
{
    [Header("Movement Settings")]
    public float searchInterval = 1.0f;

    [Header("Attack Settings")]
    public float attackRange = 3.0f;      // 当距离小于此值时触发攻击
    public float attackInterval = 2.0f;   // 攻击间隔
    public float damage = 15.0f;
    public float sprayAngle = 90.0f;      // 喷雾攻击扇形角度（例如 90 度）
    public float sprayRange = 5.0f;       // 喷雾攻击半径

    [Header("Visual Effect Settings")]
    public GameObject fanAttackPrefab;    // 预制体，用于显示扇形攻击的范围
    public float fanEffectDuration = 0.5f;  // 预制体显示时长

    [Header("状态标志")]
    public bool isAttacking = false;

    public Transform currentTarget; // 当前锁定目标
    public bool canAttack = true;
    public bool attackDisabledBySkill = false;

    private Rigidbody2D rb;

    // 接口实例（可以在 Inspector 中注入自定义实现，否则在 Start 中使用默认实现）
    public ITargetFinder targetFinder;
    public IMover mover;
    public ISprayAttacker fanSprayAttacker;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 若未注入，则使用默认目标查找器
        targetFinder = GetComponent<ITargetFinder>();
        if (targetFinder == null)
        {
            targetFinder = new NearestTeamFinder();
        }
        // 若未注入，则使用默认移动实现
        mover = GetComponent<IMover>();
        if (mover == null)
        {
            mover = new SimpleMover();
        }
        // 若未注入，则使用默认扇形攻击实现
        fanSprayAttacker = GetComponent<ISprayAttacker>();
        if (fanSprayAttacker == null)
        {
            fanSprayAttacker = new EnemyFanSprayAttacker();
        }

        StartCoroutine(SearchTargetRoutine());
    }

    void Update()
    {
        if (currentTarget != null && canAttack && !attackDisabledBySkill)
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

        // 在攻击前实例化预制体显示扇形范围（注意：预制体需要设置好合适的视觉表现）
        if (fanAttackPrefab != null)
        {
            GameObject effect = Instantiate(fanAttackPrefab, transform.position, transform.rotation);
            Destroy(effect, fanEffectDuration);
        }

        // 执行扇形喷雾攻击
        yield return StartCoroutine(fanSprayAttacker.Attack(this, transform, currentTarget, damage, attackInterval, sprayAngle, sprayRange));
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
}
