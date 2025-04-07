using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using static UnityEngine.GraphicsBuffer;

public class Temp_Boss : BaseEnemy
{
    [Header("Movement Settings")]
    public float searchInterval = 1.0f;


    [Header("Rect Attack Settings")]
    public float attackRange = 3.0f;      // 当距离小于此值时触发攻击
    public float attackInterval = 2.0f;   // 攻击间隔
    public float damage = 15.0f;
    public float chargeDuration = 0.5f;   // 闪烁预警显示时间（充能时间）
    public float attackWidth = 1.0f;      // 矩形攻击宽度
    public float attackLength = 6.0f;     // 矩形攻击长度

    [Header("Rect Visual Effect Settings")]
    public GameObject chargeIndicatorPrefab;  // 红色闪烁细线预制体（预制体需挂 BlinkingLine 脚本）
    public float chargeIndicatorDuration = 0.5f; // 预警效果显示时长

    public GameObject rectAttackPrefab;       // 矩形攻击范围预制体（显示最终攻击区域）
    public float rectAttackEffectDuration = 0.3f; // 最终攻击效果显示时长

    [Header("Eight Direct Attack Settings")]
    public float rangedAttackRange = 5f;        // 攻击射程（可按需求决定是否依然需要检测距离）
    public float rangedAttackInterval = 1.5f;   // 攻击间隔
    public GameObject bulletPrefab;       // 子弹预制体
    public Transform firePoint;           // 子弹发射位置
    public float bulletSpeed = 10f;       // 子弹移动速度
    public float bulletLifetime = 2f;     // 子弹存在时间

    [Header("Attackers Settings")]
    public int AttackerCount = 2;
    public EnemyRectAttackAttacker enemyRectAttackAttacker;
    public EightDirectionRangedAttacker eightDirectionRangedAttacker;

    public ITargetFinder targetFinder;
    public IMover mover;

    public Transform currentTarget; // 当前锁定目标
    private Rigidbody2D rb;

    private int attackIndex = 0; // 攻击方式索引

    private bool canAttack = true;        // 攻击冷却标志
    private bool isAttacking = false;     // 攻击状态标志

    private void Awake()
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

        if (enemyRectAttackAttacker == null)
        {
            enemyRectAttackAttacker = new EnemyRectAttackAttacker();
        }
        // 使用默认八方向远程攻击实现（若未注入）
        if (eightDirectionRangedAttacker == null)
        {
            eightDirectionRangedAttacker = new EightDirectionRangedAttacker();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (currentTarget != null && canAttack)
        { 
            if(CheckAttackAble())
            {
                FaceTarget(currentTarget.position);
                StartCoroutine(PerformRandomAttack());
            }
        }
        StartCoroutine(SearchTargetRoutine());
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


    IEnumerator PerformRandomAttack()
    {
        switch (attackIndex)
        {
            case 0:
                // 执行矩形攻击
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
                yield return StartCoroutine(enemyRectAttackAttacker.Attack(this, transform, currentTarget, damage, attackInterval, attackWidth, attackLength));

                isAttacking = false;
                canAttack = true;
                attackIndex = Random.Range(0, 2); // 随机选择下一个攻击方式
                break;


            case 1:
                // 执行八方向远程攻击
                canAttack = false;
                yield return StartCoroutine(eightDirectionRangedAttacker.Attack(this, transform, currentTarget, rangedAttackInterval, bulletPrefab, firePoint, bulletSpeed, bulletLifetime));
                canAttack = true;
                break;

            default:
                break;
        }
    }

    bool CheckAttackAble()
    {
        switch (attackIndex)
        {
            case 0:
                return RectAttackCheck();

            case 1:
                return EightDirectionRangedCheck();

            default:
                break;
        }
        return false;
    }

    bool RectAttackCheck()
    {
        float distance = Vector2.Distance(transform.position, currentTarget.position);
        if (distance <= attackRange)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool EightDirectionRangedCheck()
    {
        float distance = Vector2.Distance(transform.position, currentTarget.position);
        if (distance <= attackRange)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // 使敌人面向目标
    void FaceTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
