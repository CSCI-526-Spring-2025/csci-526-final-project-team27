using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class SideRangedEnemy : BaseEnemy
{
    [Header("Movement Settings")]
    public float detectionInterval = 0.5f; // 目标检测间隔

    [Header("Attack Settings")]
    public float attackRange = 5f;        // 攻击射程
    public float attackInterval = 1.5f;   // 攻击间隔
    public GameObject bulletPrefab;       // 子弹预制体
    public Transform firePoint;           // 子弹发射位置
    public float bulletSpeed = 10f;       // 子弹移动速度
    public float bulletLifetime = 2f;     // 子弹存在时间

    private Transform target;             // 玩家目标
    private Rigidbody2D rb;
    private bool canAttack = true;        // 攻击冷却标志

    // 复用接口：目标查找、移动和攻击
    public ITargetFinder targetFinder;
    public IMover mover;
    public IRangedAttacker rangedAttacker;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // 若未外部注入，使用默认实现
        if (targetFinder == null)
        {
            targetFinder = new NearestTeamFinder();
        }
        if (mover == null)
        {
            mover = new SimpleMover();
        }
        // 使用新实现的侧向攻击方式
        if (rangedAttacker == null)
        {
            rangedAttacker = new SideRangedAttacker();
        }
    }

    void Start()
    {
        StartCoroutine(UpdateTargetRoutine());
    }

    void Update()
    {
        if (target == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distance = Vector2.Distance(transform.position, target.position);
        if (distance <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            FaceTarget(target.position);
            if (canAttack)
            {
                StartCoroutine(PerformAttack());
            }
        }
        else
        {
            mover.Move(transform, rb, target, moveSpeed);
        }
    }

    // 定时更新目标
    IEnumerator UpdateTargetRoutine()
    {
        while (true)
        {
            target = targetFinder.FindTarget(transform);
            yield return new WaitForSeconds(detectionInterval);
        }
    }

    // 利用 IRangedAttacker 接口进行攻击
    IEnumerator PerformAttack()
    {
        canAttack = false;
        yield return StartCoroutine(rangedAttacker.Attack(this, transform, target, attackInterval, bulletPrefab, firePoint, bulletSpeed, bulletLifetime));
        canAttack = true;
    }

    // 使敌人始终面向目标
    void FaceTarget(Vector3 targetPos)
    {
        Vector2 direction = (targetPos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
