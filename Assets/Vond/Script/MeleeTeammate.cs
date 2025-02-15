using UnityEngine;
using System.Collections;

public class MeleeTeammate : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;            // 移动速度
    public float attackRange = 1.5f;        // 攻击范围（当与目标距离小于该值时开始攻击）
    public float searchInterval = 0.5f;     // 定时搜索目标的间隔

    [Header("Attack Settings")]
    public GameObject hitboxPrefab;         // 攻击时生成的 Hitbox 预制体（内置伤害和击退逻辑）
    public float hitboxOffset = 1.0f;       // Hitbox 生成时的偏移量（沿攻击方向）
    public float attackCooldown = 1.0f;     // 两次攻击之间的间隔

    private Transform currentTarget;        // 当前锁定的敌人目标（Tag 为 "Enemy"）
    private Rigidbody2D rb;
    private bool isAttacking = false;       // 攻击状态
    private bool canAttack = true;          // 攻击冷却标志

    public float damage = 10.0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // 定时搜索最近的敌人目标
        StartCoroutine(SearchTargetRoutine());
    }

    void Update()
    {
        if (currentTarget == null)
        {
            // 没有目标时停止移动
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distance = Vector2.Distance(transform.position, currentTarget.position);

        // 如果目标在攻击范围内且处于可攻击状态，则执行攻击
        if (distance <= attackRange && canAttack)
        {
            StartCoroutine(Attack());
        }
        else if (!isAttacking)
        {
            // 如果目标不在攻击范围内，并且当前没有攻击，则向目标移动
            MoveTowardsTarget();
        }
    }

    void FixedUpdate()
    {
        // 攻击期间确保刚体速度归零，防止残留移动
        if (isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // 攻击协程：生成 Hitbox 发动攻击，并等待冷却时间后恢复
    IEnumerator Attack()
    {
        canAttack = false;
        isAttacking = true;

        // 面向目标
        FaceTarget(currentTarget.position);

        // 计算攻击方向和生成 Hitbox 的位置（沿攻击方向偏移）
        Vector2 attackDirection = (currentTarget.position - transform.position).normalized;
        Vector2 spawnPos = (Vector2)transform.position + attackDirection * hitboxOffset;
        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        // 实例化 Hitbox（Hitbox 脚本内包含伤害和击退的逻辑）
        if (hitboxPrefab != null)
        {
            Instantiate(hitboxPrefab, spawnPos, rotation);
        }

        Health_BC targetHealth = currentTarget.GetComponent<Health_BC>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage);
        }

        // 等待攻击间隔后恢复
        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
        canAttack = true;
    }

    // 向当前目标移动
    void MoveTowardsTarget()
    {
        Vector2 direction = (currentTarget.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
        FaceTarget(currentTarget.position);
    }

    // 使角色面向目标
    void FaceTarget(Vector3 targetPos)
    {
        Vector2 direction = (targetPos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // 定时搜索最近的敌人目标
    IEnumerator SearchTargetRoutine()
    {
        while (true)
        {
            FindNearestEnemy();
            yield return new WaitForSeconds(searchInterval);
        }
    }

    // 搜索场景中所有 Tag 为 "Enemy" 的对象，选择距离自己最近的一个
    void FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float minDistance = Mathf.Infinity;
        Transform nearest = null;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null)
                continue;

            float d = Vector2.Distance(transform.position, enemy.transform.position);
            if (d < minDistance)
            {
                minDistance = d;
                nearest = enemy.transform;
            }
        }
        currentTarget = nearest;
    }
}
