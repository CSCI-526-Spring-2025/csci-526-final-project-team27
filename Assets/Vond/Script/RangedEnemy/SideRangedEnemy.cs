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
    public Transform firePointLeft;       // 左侧发射点
    public Transform firePointRight;      // 右侧发射点
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
            // 停止移动时设置动画状态
            if(animator != null)
            {
                animator.SetBool("isWalking", false);
            }
            return;
        }

        // 更新精灵朝向
        if (spriteRenderer != null)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            spriteRenderer.flipX = direction.x < 0;
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

    // 定时更新目标
    IEnumerator UpdateTargetRoutine()
    {
        while (true)
        {
            target = targetFinder.FindTarget(transform);
            yield return new WaitForSeconds(detectionInterval);
        }
    }

    // 修改攻击方法，使用对应方向的发射点
    IEnumerator PerformAttack()
    {
        canAttack = false;
        
        // 播放攻击动画
        if(animator != null)
        {
            animator.Play("Attack");
        }
        
        // 根据朝向选择发射点
        Transform currentFirePoint = spriteRenderer.flipX ? firePointLeft : firePointRight;
        
        // 等待动画延迟
        yield return new WaitForSeconds(delayAttackTime);
        
        yield return StartCoroutine(rangedAttacker.Attack(this, transform, target, attackInterval, 
            bulletPrefab, currentFirePoint, bulletSpeed, bulletLifetime));
        
        canAttack = true;
    }

    // 修改面向目标方法，同时处理旋转和精灵翻转
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
