using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class RangedEnemy : BaseEnemy
{
    [Header("Movement Settings")]
    public float detectionInterval = 0.5f; // 目标检测间隔

    [Header("Attack Settings")]
    public float attackRange = 5f;        // 远程攻击射程
    public float attackInterval = 1.5f;   // 两次攻击之间的间隔
    public GameObject bulletPrefab;       // 子弹预制体
    public Transform firePoint;           // 子弹发射位置
    public float bulletSpeed = 10f;       // 子弹移动速度
    public float bulletLifetime = 2f;     // 子弹存在时间

    private Transform target;             // 玩家目标
    private Rigidbody2D rb;
    private bool canAttack = true;        // 攻击冷却标志

    // 通过接口实现功能解耦
    public ITargetFinder targetFinder;
    public IMover mover;
    public IRangedAttacker rangedAttacker;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // 若外部未注入，则使用默认实现
        if (targetFinder == null)
        {
            targetFinder = new PlayerFinder();
        }
        if (mover == null)
        {
            mover = new SimpleMover();
        }
        if (rangedAttacker == null)
        {
            rangedAttacker = new RangedAttacker();
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
            // 利用 IMover 接口实现移动（RotatingMover 会同时处理旋转）
            mover.Move(transform, rb, target, moveSpeed);
        }
    }

    // 定时更新目标（利用 PlayerFinder 查找玩家）
    IEnumerator UpdateTargetRoutine()
    {
        while (true)
        {
            target = targetFinder.FindTarget(transform);
            yield return new WaitForSeconds(detectionInterval);
        }
    }

    // 调用 IRangedAttacker 接口进行攻击
    IEnumerator PerformAttack()
    {
        canAttack = false;
        yield return StartCoroutine(rangedAttacker.Attack(this, transform, target, attackInterval, bulletPrefab, firePoint, bulletSpeed, bulletLifetime));
        canAttack = true;
    }

    // 保证敌人始终面向目标
    void FaceTarget(Vector3 targetPos)
    {
        Vector2 direction = (targetPos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}