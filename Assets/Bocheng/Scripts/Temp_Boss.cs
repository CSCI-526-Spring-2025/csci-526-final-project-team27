using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using static UnityEngine.GraphicsBuffer;

public class Temp_Boss : BaseEnemy
{
    [Header("Sprite")]
    public SpriteRenderer spriteRenderer; // 用于翻转精灵的 SpriteRenderer 组件


    [Header("Movement Settings")]
    public float searchInterval = 1.0f;
    public float teleportPercent = 0.5f; // 50% 概率传送
    public float teleportDistance = 3.0f; // 传送距离


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

    [Header("Spray Attack Settings")]
    public float sprayAttackRange = 3.0f;      // 当距离小于此值时触发攻击
    public float sprayAttackInterval = 2.0f;   // 攻击间隔
    public float sprayDamage = 15.0f;
    public float sprayAngle = 90.0f;      // 喷雾攻击扇形角度（例如 90 度）
    public float sprayRange = 5.0f;       // 喷雾攻击半径

    [Header("Spray Visual Effect Settings")]
    public GameObject fanAttackPrefab;    // 预制体，用于显示扇形攻击的范围
    public float fanEffectDuration = 0.5f;  // 预制体显示时长


    [Header("Attackers Settings")]
    public int AttackerCount = 3;
    public EnemyRectAttackAttacker enemyRectAttackAttacker;
    public EightDirectionRangedAttacker eightDirectionRangedAttacker;
    public EnemyFanSprayAttacker enemyFanSprayAttacker;


    public ITargetFinder targetFinder;
    public IMover mover;

    public Transform currentTarget; // 当前锁定目标
    private bool lockOnTarget = false; // 是否锁定目标
    private Rigidbody2D rb;

    private int attackIndex = 2; // 攻击方式索引

    private bool canAttack = true;        // 攻击冷却标志
    private bool isAttacking = false;     // 攻击状态标志

    private Vector2 roomPosition; // 房间位置
    private Vector2 roomSize;     // 房间大小
    private float bias = 4.0f;


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

        // 使用默认扇形喷雾攻击实现（若未注入）
        if (enemyFanSprayAttacker == null)
        {
            enemyFanSprayAttacker = new EnemyFanSprayAttacker();
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

        if(!lockOnTarget)
        {
            StartCoroutine(SearchTargetRoutine());
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

    public void SetRoomInfo(Vector2 position, Vector2 size)
    {
        roomPosition = position;
        roomSize = new Vector2(size.x - bias, size.y - 0.5f * bias);
        Debug.Log("Room Position: " + roomPosition);
        Debug.Log("Room Size: " + roomSize);
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
                lockOnTarget = true; // 锁定目标，停止搜索

                Vector3 TPosition = currentTarget.position;
                Vector3 center = transform.position + (TPosition - transform.position).normalized * (attackLength / 2f);
                float angle = Mathf.Atan2(TPosition.y - transform.position.y, TPosition.x - transform.position.x) * Mathf.Rad2Deg;

                // 预警阶段：实例化闪烁预警效果
                if (chargeIndicatorPrefab != null)
                {
                    Vector3 spawnPos = center;
                    Quaternion rotation = Quaternion.Euler(0, 0, angle);
                    
                    GameObject effect = Instantiate(chargeIndicatorPrefab, spawnPos, rotation);
                    Destroy(effect, chargeIndicatorDuration);
                }


                // 等待充能时间
                yield return new WaitForSeconds(chargeDuration);

                // 攻击效果预警：实例化矩形攻击区域显示预制体
                if (rectAttackPrefab != null)
                {
                    // 计算矩形区域中心位置：敌人位置向前偏移 attackLength/2
                    // Vector3 rectCenter = transform.position + transform.right * (attackLength / 2f);
                    Vector3 spawnPos = center;
                    Quaternion rotation = Quaternion.Euler(0, 0, angle);
                    // GameObject rectEffect = Instantiate(rectAttackPrefab, rectCenter, transform.rotation);

                    GameObject rectEffect = Instantiate(rectAttackPrefab, spawnPos, rotation);
                    // 调整预制体的缩放以匹配攻击范围（假设预制体原始尺寸为 1 单位）
                    rectEffect.transform.localScale = new Vector3(attackLength, attackWidth, 1f);
                    Destroy(rectEffect, rectAttackEffectDuration);
                }

                // 执行矩形攻击：对区域内目标造成伤害
                yield return StartCoroutine(enemyRectAttackAttacker.Attack(this, transform, currentTarget, damage, attackInterval, attackWidth, attackLength,center,angle));

                isAttacking = false;
                canAttack = true;
                AttackEnd(); // 攻击结束，随机选择下一个攻击方式
                break;


            case 1:
                // 执行八方向远程攻击
                canAttack = false;
                isAttacking = true;
                lockOnTarget = true; // 锁定目标，停止搜索
                yield return StartCoroutine(eightDirectionRangedAttacker.Attack(this, transform, currentTarget, rangedAttackInterval, bulletPrefab, firePoint, bulletSpeed, bulletLifetime));
                isAttacking = false;
                canAttack = true;
                AttackEnd();
                break;

            case 2:
                canAttack = false;
                isAttacking = true;
                lockOnTarget = true; // 锁定目标，停止搜索
                // 在攻击前实例化预制体显示扇形范围（注意：预制体需要设置好合适的视觉表现）
                if (fanAttackPrefab != null)
                {
                    GameObject effect = Instantiate(fanAttackPrefab, transform.position, transform.rotation);
                    Destroy(effect, fanEffectDuration);
                }
                // 执行扇形喷雾攻击
                yield return StartCoroutine(enemyFanSprayAttacker.Attack(this, transform, currentTarget, sprayDamage, sprayAttackInterval, sprayAngle, sprayRange, false));

                isAttacking = false;
                canAttack = true;
                AttackEnd();
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

            case 2:
                return SprayAttackCheck();

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
        if (distance <= rangedAttackRange)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool SprayAttackCheck()
    {
        float distance = Vector2.Distance(transform.position, currentTarget.position);
        if (distance <= sprayAttackRange)
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
        /*
        Vector3 direction = (targetPosition - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        */

        if (spriteRenderer != null)
        {
            Vector2 direction = (targetPosition - transform.position).normalized;
            if (direction.x < 0)
            {
                spriteRenderer.flipX = true;
            }
            else
            {
                spriteRenderer.flipX = false;
            }
        }
    }

    void AttackEnd()
    {
        lockOnTarget = false; // 解除锁定目标，继续搜索
        attackIndex = Random.Range(0, AttackerCount); // 随机选择下一个攻击方式
        // 50% 概率传送
        if (Random.value < teleportPercent)
        {
            Teleport();
        }
    }

    void Teleport()
    {
        //choose a random position within the room
        Vector2 randomPosition = new Vector2(
                       Random.Range(roomPosition.x - roomSize.x / 2, roomPosition.x + roomSize.x / 2),
                                  Random.Range(roomPosition.y - roomSize.y / 2, roomPosition.y + roomSize.y / 2)
                                         );
        transform.position = randomPosition;
    }
}
