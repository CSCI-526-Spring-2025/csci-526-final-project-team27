// using UnityEngine;
// using System.Collections;

// public class RangedRectangularEnemy : BaseEnemy
// {
//     [Header("Movement Settings")]
//     public float searchInterval = 1.0f;

//     [Header("Attack Settings")]
//     public float attackRange = 3.0f;      // 当距离小于此值时触发攻击
//     public float attackInterval = 2.0f;   // 攻击间隔
//     public float damage = 15.0f;
//     public float chargeDuration = 0.5f;   // 红色闪烁细线显示时间（充能时间）
//     public float attackWidth = 1.0f;      // 矩形攻击宽度
//     public float attackLength = 6.0f;     // 矩形攻击长度

//     [Header("Visual Effect Settings")]
//     public GameObject chargeIndicatorPrefab;  // 红色闪烁细线预制体，用于显示即将攻击的预警效果
//     public float chargeIndicatorDuration = 0.5f; // 预警效果显示时长

//     [Header("状态标志")]
//     public bool isAttacking = false;

//     public Transform currentTarget; // 当前锁定目标
//     private Rigidbody2D rb;
//     private bool canAttack = true;

//     // 接口实例（可在 Inspector 中注入自定义实现，否则在 Start 中使用默认实现）
//     public ITargetFinder targetFinder;
//     public IMover mover;
//     public IRectAttacker rectAttackAttacker;

//     void Start()
//     {
//         rb = GetComponent<Rigidbody2D>();

//         // 使用默认目标查找器（若未注入）
//         targetFinder = GetComponent<ITargetFinder>();
//         if (targetFinder == null)
//         {
//             targetFinder = new NearestTeamFinder();
//         }
//         // 使用默认移动实现（若未注入）
//         mover = GetComponent<IMover>();
//         if (mover == null)
//         {
//             mover = new SimpleMover();
//         }
//         // 使用默认矩形攻击实现（若未注入）
//         rectAttackAttacker = GetComponent<IRectAttacker>();
//         if (rectAttackAttacker == null)
//         {
//             rectAttackAttacker = new EnemyRectAttackAttacker();
//         }

//         StartCoroutine(SearchTargetRoutine());
//     }

//     void Update()
//     {
//         if (currentTarget != null && canAttack)
//         {
//             float distance = Vector2.Distance(transform.position, currentTarget.position);
//             if (distance <= attackRange)
//             {
//                 FaceTarget(currentTarget.position);
//                 StartCoroutine(PerformAttack());
//             }
//         }
//     }

//     void FixedUpdate()
//     {
//         if (isAttacking)
//         {
//             rb.linearVelocity = Vector2.zero;
//             return;
//         }

//         if (currentTarget != null)
//         {
//             mover.Move(transform, rb, currentTarget, moveSpeed);
//         }
//     }

//     // 定时更新目标
//     IEnumerator SearchTargetRoutine()
//     {
//         while (true)
//         {
//             currentTarget = targetFinder.FindTarget(transform);
//             yield return new WaitForSeconds(searchInterval);
//         }
//     }

//     IEnumerator PerformAttack()
//     {
//         canAttack = false;
//         isAttacking = true;

//         // 攻击预警：实例化红色闪烁细线预制体显示攻击范围
//         // if (chargeIndicatorPrefab != null)
//         // {
//         //     GameObject effect = Instantiate(chargeIndicatorPrefab, transform.position, transform.rotation);
//         //     Destroy(effect, chargeIndicatorDuration);
//         // }
//         if (chargeIndicatorPrefab != null)
//         {
//             // 计算偏移量，将预制体中心向攻击方向平移半个攻击长度
//             Vector3 spawnPos = transform.position + transform.right * (attackLength / 2f);
//             GameObject effect = Instantiate(chargeIndicatorPrefab, spawnPos, transform.rotation);
//             Destroy(effect, chargeIndicatorDuration);
//         }


//         // 等待充能时间后再发出实际攻击
//         yield return new WaitForSeconds(chargeDuration);

//         // 执行矩形攻击
//         yield return StartCoroutine(rectAttackAttacker.Attack(this, transform, currentTarget, damage, attackInterval, attackWidth, attackLength));

//         isAttacking = false;
//         canAttack = true;
//     }

//     // 使敌人面向目标
//     void FaceTarget(Vector3 targetPosition)
//     {
//         Vector3 direction = (targetPosition - transform.position).normalized;
//         float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
//         transform.rotation = Quaternion.Euler(0, 0, angle);
//     }

// }

using UnityEngine;
using System.Collections;

public class RangedRectangularEnemy : BaseEnemy
{
    [Header("Movement Settings")]
    public float searchInterval = 1.0f;

    [Header("Attack Settings")]
    public float attackRange = 3.0f;      // 当距离小于此值时触发攻击
    public float attackInterval = 2.0f;   // 攻击间隔
    public float damage = 15.0f;
    public float chargeDuration = 0.5f;   // 闪烁预警显示时间（充能时间）
    public float attackWidth = 1.0f;      // 矩形攻击宽度
    public float attackLength = 6.0f;     // 矩形攻击长度

    [Header("Visual Effect Settings")]
    public GameObject chargeIndicatorPrefab;  // 红色闪烁细线预制体（预制体需挂 BlinkingLine 脚本）
    public float chargeIndicatorDuration = 0.5f; // 预警效果显示时长

    public GameObject rectAttackPrefab;       // 矩形攻击范围预制体（显示最终攻击区域）
    public float rectAttackEffectDuration = 0.3f; // 最终攻击效果显示时长

    [Header("状态标志")]
    public bool isAttacking = false;

    public Transform currentTarget; // 当前锁定目标
    private Rigidbody2D rb;
    public bool canAttack = true;
    public bool attackDisabledBySkill = false;

    // 接口实例（可在 Inspector 中注入自定义实现，否则在 Start 中使用默认实现）
    public ITargetFinder targetFinder;
    public IMover mover;
    public IRectAttacker rectAttackAttacker;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 使用默认目标查找器（若未注入）
        targetFinder = GetComponent<ITargetFinder>();
        if (targetFinder == null)
        {
            targetFinder = new NearestTeamFinder();
        }
        // 使用默认移动实现（若未注入）
        mover = GetComponent<IMover>();
        if (mover == null)
        {
            mover = new SimpleMover();
        }
        // 使用默认矩形攻击实现（若未注入）
        rectAttackAttacker = GetComponent<IRectAttacker>();
        if (rectAttackAttacker == null)
        {
            rectAttackAttacker = new EnemyRectAttackAttacker();
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

        // 预警阶段：实例化闪烁预警效果
        if (chargeIndicatorPrefab != null)
        {
            // 计算偏移量，将预制体中心向攻击方向平移半个攻击长度
            Vector3 spawnPos = transform.position + transform.right * (attackLength / 2f);
            GameObject effect = Instantiate(chargeIndicatorPrefab, spawnPos, transform.rotation);
            Destroy(effect, chargeIndicatorDuration);
        }


        // 等待充能时间
        yield return new WaitForSeconds(chargeDuration);

        // 攻击效果预警：实例化矩形攻击区域显示预制体
        if (rectAttackPrefab != null)
        {
            // 计算矩形区域中心位置：敌人位置向前偏移 attackLength/2
            Vector3 rectCenter = transform.position + transform.right * (attackLength / 2f);
            GameObject rectEffect = Instantiate(rectAttackPrefab, rectCenter, transform.rotation);
            // 调整预制体的缩放以匹配攻击范围（假设预制体原始尺寸为 1 单位）
            rectEffect.transform.localScale = new Vector3(attackLength, attackWidth, 1f);
            Destroy(rectEffect, rectAttackEffectDuration);
        }

        // 执行矩形攻击：对区域内目标造成伤害
        yield return StartCoroutine(rectAttackAttacker.Attack(this, transform, currentTarget, damage, attackInterval, attackWidth, attackLength));

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
