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
    public Transform firePointLeft;   // 左侧发射点
    public Transform firePointRight;  // 右侧发射点
    public float bulletSpeed = 10f;       // 子弹移动速度
    public float bulletLifetime = 2f;     // 子弹存在时间

    [Header("Anim Settings")]
    public Animator animator;                // 动画控制器
    public SpriteRenderer spriteRenderer;    // 精灵渲染器
    public float delayAttackTime = 0.1f;    // 攻击动画延迟时间

    private Transform target;             // 玩家目标
    private Rigidbody2D rb;
    public bool canAttack = true;        // 攻击冷却标志
    public bool attackDisabledBySkill = false;

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
            // 停止移动时设置动画状态
            if(animator != null)
            {
                animator.SetBool("isWalking", false);
            }
            return;
        }

        if (spriteRenderer != null)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            if (direction.x < 0)
            {
                spriteRenderer.flipX = true;
            }
            else
            {
                spriteRenderer.flipX = false;
            }
        }

        float distance = Vector2.Distance(transform.position, target.position);
        if (distance <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            // 停止移动时设置动画状态
            if(animator != null)
            {
                animator.SetBool("isWalking", false);
            }
            FaceTarget(target.position);
            if (canAttack && !attackDisabledBySkill)
            {
                StartCoroutine(PerformAttack());
            }
        }
        else
        {
            // 移动时设置动画状态
            if(animator != null)
            {
                animator.SetBool("isWalking", true);
            }
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
        
        // 根据朝向选择发射点
        Transform currentFirePoint = spriteRenderer.flipX ? firePointLeft : firePointRight;
        
        if(animator != null)
        {
            animator.Play("Attack");
        }
        
        yield return new WaitForSeconds(delayAttackTime);
        
        // 使用选定的发射点
        yield return StartCoroutine(rangedAttacker.Attack(this, transform, target, attackInterval, 
            bulletPrefab, currentFirePoint, bulletSpeed, bulletLifetime));
        
        canAttack = true;
    }

    // 保证敌人始终面向目标
    void FaceTarget(Vector3 targetPos)
    {
        if(spriteRenderer != null)
        {
            Vector2 direction = (targetPos - transform.position).normalized;
            if (direction.x < 0)
            {
                spriteRenderer.flipX = true;  // 向左时翻转精灵
            }
            else
            {
                spriteRenderer.flipX = false; // 向右时不翻转
            }
        }
    }
}