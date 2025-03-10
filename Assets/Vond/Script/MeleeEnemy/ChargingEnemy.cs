using UnityEngine;
using System.Collections;

public class ChargingEnemy : BaseEnemy
{
    [Header("冲撞设置")]
    [Tooltip("冲撞时的移动速度")]
    public float chargeSpeed = 8f;
    [Tooltip("每次冲撞后的休息时间")]
    public float restTime = 1f;
    [Tooltip("冲撞对玩家/队友造成的伤害")]
    public float damage = 15f;
    [Tooltip("对碰撞目标施加的推力大小")]
    public float pushForce = 5f;

    // 接口实例，可在 Inspector 中注入自定义实现
    public IMover mover;
    public ITargetFinder targetFinder;
    public IEnemyMelee meleeAttacker;

    private Rigidbody2D rb;
    private bool isCharging = false;
    private Transform currentTarget;
    // 冲撞开始时锁定的目标位置
    private Vector3 lockedChargeTarget;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 默认使用 SimpleMover 实现直线移动（但这里我们直接自行计算方向）
        if (mover == null)
            mover = new SimpleMover();

        // 若没有自定义目标查找器，则简单查找玩家
        if (targetFinder == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                currentTarget = player.transform;
        }
        else
        {
            currentTarget = targetFinder.FindTarget(transform);
        }

        // 默认使用已有的 EnemyMeleeAttacker 实现攻击逻辑
        if (meleeAttacker == null)
            meleeAttacker = new EnemyMeleeAttacker();

        // 开启冲撞循环协程
        StartCoroutine(ChargeCycle());
    }

    void FixedUpdate()
    {
        if (isCharging)
        {
            // 如果距离锁定目标非常近，则结束冲撞，避免反复计算方向产生抖动
            if (Vector2.Distance(transform.position, lockedChargeTarget) < 0.1f)
            {
                isCharging = false;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                return;
            }
            // 计算固定目标方向
            Vector2 direction = ((Vector2)lockedChargeTarget - (Vector2)transform.position).normalized;
            rb.linearVelocity = direction * chargeSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    IEnumerator ChargeCycle()
    {
        while (true)
        {
            // 更新目标（仅在非冲撞时）
            if (targetFinder != null)
            {
                currentTarget = targetFinder.FindTarget(transform);
            }
            else
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    currentTarget = player.transform;
            }

            // 锁定当前目标位置，作为本次冲撞方向
            if (currentTarget != null)
            {
                lockedChargeTarget = currentTarget.position;
            }
            
            // 开始冲撞
            isCharging = true;

            // 固定冲撞时长为 3 秒
            float chargeTime = 5f;
            float timer = 0f;
            while (isCharging && timer < chargeTime)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            // 冲撞结束后，停止运动并进入休息状态
            isCharging = false;
            rb.linearVelocity = Vector2.zero;
            yield return new WaitForSeconds(restTime);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 如果碰到障碍物（假设障碍物标签为 "Obstacle"），则停止本次冲撞
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            isCharging = false;
            rb.linearVelocity = Vector2.zero;
        }
        
        // 推开所有碰撞对象，使用碰撞接触点的法线来计算更准确的推力方向
        if (collision.rigidbody != null)
        {
            Vector2 pushDirection = Vector2.zero;
            if (collision.contacts != null && collision.contacts.Length > 0)
            {
                pushDirection = collision.contacts[0].normal;
            }
            else
            {
                pushDirection = (collision.transform.position - transform.position).normalized;
            }
            collision.rigidbody.AddForce(pushDirection * pushForce, ForceMode2D.Impulse);
        }
        
        // 若碰撞对象是玩家或队友，则造成伤害
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Ally"))
        {
            StartCoroutine(DealDamageRoutine(collision.gameObject.transform));
        }
    }

    IEnumerator DealDamageRoutine(Transform target)
    {
        // 调用已有攻击逻辑，对目标造成伤害（攻击间隔设为 0）
        yield return StartCoroutine(meleeAttacker.Attack(this, transform, target, damage, 0));
    }
}
