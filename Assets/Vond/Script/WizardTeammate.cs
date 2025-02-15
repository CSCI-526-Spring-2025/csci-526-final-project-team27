using UnityEngine;
using System.Collections;

public class WizardTeammate : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;         // 追踪敌人的移动速度
    public float attackRange = 7f;       // 当目标进入该距离时，停止移动并开始施法

    [Header("Casting & Explosion Settings")]
    public float chantTime = 2f;         // 吟唱时间（施法准备时间）
    public float attackCooldown = 3f;    // 攻击冷却时间
    public float explosionRadius = 3f;   // 爆炸的范围半径
    public float explosionDamage = 15f;  // 爆炸造成的伤害
    [Tooltip("爆炸特效存在的时间，之后会自动销毁")]
    public float explosionEffectLifetime = 1f; 

    [Header("Effects")]
    public GameObject chantEffectPrefab;      // 吟唱时生成的阴影特效（圆形阴影）
    public GameObject explosionEffectPrefab;  // 爆炸特效

    [Header("Target Search Settings")]
    public float searchInterval = 0.5f;         // 定时搜索目标的间隔

    private Transform currentTarget;          // 最近的敌人目标（Tag 为 "Enemy"）
    private Rigidbody2D rb;
    private bool isChanting = false;            // 施法状态

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // 定时搜索最近的敌人目标
        StartCoroutine(SearchTargetRoutine());
    }

    void Update()
    {
        // 如果没有目标，则停止移动
        if (currentTarget == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distance = Vector2.Distance(transform.position, currentTarget.position);

        // 当目标不在攻击范围内且未处于施法状态时，追踪目标
        if (distance > attackRange && !isChanting)
        {
            MoveTowardsTarget();
        }
        // 当目标进入攻击范围且未处于施法状态时，开始施法
        else if (!isChanting)
        {
            StartCoroutine(ChantAndExplode());
        }
    }

    // 向当前目标移动
    void MoveTowardsTarget()
    {
        Vector2 direction = (currentTarget.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
        FaceTarget(currentTarget.position);
    }

    // 吟唱并爆炸的协程
    IEnumerator ChantAndExplode()
    {
        isChanting = true;
        rb.linearVelocity = Vector2.zero; // 停止移动

        // 使队友面向目标
        FaceTarget(currentTarget.position);

        // 在目标当前位置生成吟唱阴影
        GameObject chantEffect = null;
        if (chantEffectPrefab != null)
        {
            chantEffect = Instantiate(chantEffectPrefab, currentTarget.position, Quaternion.identity);
        }

        // 等待吟唱时间
        yield return new WaitForSeconds(chantTime);

        // 以吟唱阴影位置为爆炸中心，先销毁吟唱阴影
        Vector3 explosionPos = Vector3.zero;
        if (chantEffect != null)
        {
            explosionPos = chantEffect.transform.position;
            Destroy(chantEffect);
        }
        else
        {
            explosionPos = currentTarget.position;
        }

        // 在爆炸位置生成爆炸特效
        GameObject explosionEffect = null;
        if (explosionEffectPrefab != null)
        {
            explosionEffect = Instantiate(explosionEffectPrefab, explosionPos, Quaternion.identity);
        }

        // 执行爆炸检测，对该区域内的敌人造成伤害
        ExplodeAtPosition(explosionPos);

        // 等待一段时间后销毁爆炸特效
        if (explosionEffect != null)
        {
            Destroy(explosionEffect, explosionEffectLifetime);
        }

        // 攻击冷却后恢复正常行为
        yield return new WaitForSeconds(attackCooldown);
        isChanting = false;
    }

    // 在指定位置进行范围爆炸伤害检测
    void ExplodeAtPosition(Vector2 center)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(center, explosionRadius);
        foreach (Collider2D hit in hitColliders)
        {
            // 仅对标记为 "Enemy" 的目标造成伤害
            if (hit.CompareTag("Enemy"))
            {
                Health_BC health = hit.GetComponent<Health_BC>();
                if (health != null)
                {
                    health.TakeDamage(explosionDamage);
                }
            }
        }
    }

    // 使角色面向目标位置
    void FaceTarget(Vector3 targetPos)
    {
        Vector2 direction = (targetPos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // 定时搜索场景中最近的敌人目标
    IEnumerator SearchTargetRoutine()
    {
        while (true)
        {
            FindNearestEnemy();
            yield return new WaitForSeconds(searchInterval);
        }
    }

    // 搜索所有 Tag 为 "Enemy" 的对象，并选出距离最近的作为目标
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

    // 在编辑器中可视化爆炸范围（用于调试）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = (currentTarget != null) ? currentTarget.position : transform.position;
        Gizmos.DrawWireSphere(center, explosionRadius);
    }
}
