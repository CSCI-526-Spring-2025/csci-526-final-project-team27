using UnityEngine;
using System.Collections;

public class MageEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;          // 追踪玩家时的移动速度
    public float attackRange = 7f;        // 当玩家进入此距离时，停止移动并开始施法

    [Header("Chanting & Explosion Settings")]
    public float chantTime = 2f;          // 吟唱时间（施法准备时间）
    public float attackCooldown = 3f;     // 爆炸后到下一次施法的间隔
    public float explosionRadius = 3f;    // 爆炸的范围半径
    public float explosionDamage = 15f;   // 爆炸造成的伤害
    [Tooltip("爆炸特效存在的时间，之后会自动销毁")]
    public float explosionEffectLifetime = 1f; 

    [Header("Effects")]
    public GameObject chantEffectPrefab;      // 吟唱时生成的阴影特效（圆形阴影）
    public GameObject explosionEffectPrefab;  // 爆炸特效

    private Transform playerTransform;    // 玩家目标
    private Rigidbody2D rb;
    private bool isChanting = false;        // 施法状态

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 获取玩家（假设玩家 Tag 为 "Player"）
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if(playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    void Update()
    {
        if (playerTransform == null)
            return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        // 玩家不在攻击范围且当前不在施法状态时，追踪玩家
        if (distance > attackRange && !isChanting)
        {
            MoveTowardsPlayer();
        }
        // 玩家进入攻击范围且不在施法状态时，开始施法
        else if (distance <= attackRange && !isChanting)
        {
            StartCoroutine(ChantAndExplode());
        }
    }

    // 移动追踪玩家
    void MoveTowardsPlayer()
    {
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;

        // 让敌人始终面向玩家（可选）
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // 吟唱并爆炸的协程
    IEnumerator ChantAndExplode()
    {
        isChanting = true;
        rb.linearVelocity = Vector2.zero; // 停止移动

        // 让敌人面向玩家
        FaceTarget(playerTransform.position);

        // 在玩家当前位置生成吟唱阴影
        GameObject chantEffect = null;
        if (chantEffectPrefab != null)
        {
            chantEffect = Instantiate(chantEffectPrefab, playerTransform.position, Quaternion.identity);
        }

        // 等待吟唱时间
        yield return new WaitForSeconds(chantTime);

        // 确定爆炸位置：使用吟唱阴影生成时的位置
        Vector3 explosionPos = Vector3.zero;
        if (chantEffect != null)
        {
            explosionPos = chantEffect.transform.position;
            Destroy(chantEffect);  // 先销毁吟唱阴影
        }
        else
        {
            // 如果没有吟唱阴影，则退回玩家当前位置（一般不应出现这种情况）
            explosionPos = playerTransform.position;
        }

        // 在吟唱阴影原位置生成爆炸特效
        GameObject explosionEffect = null;
        if (explosionEffectPrefab != null)
        {
            explosionEffect = Instantiate(explosionEffectPrefab, explosionPos, Quaternion.identity);
        }

        // 执行爆炸检测，对区域内玩家造成伤害
        ExplodeAtPosition(explosionPos);

        // 等待一定时间后销毁爆炸特效
        if (explosionEffect != null)
        {
            Destroy(explosionEffect, explosionEffectLifetime);
        }

        // 攻击冷却后才能再次施法
        yield return new WaitForSeconds(attackCooldown);
        isChanting = false;
    }

    // 在指定位置进行范围爆炸伤害检测
    void ExplodeAtPosition(Vector2 center)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(center, explosionRadius);
        foreach (Collider2D hit in hitColliders)
        {
            // 对玩家造成伤害（假设玩家身上挂有 Health_BC 组件）
            if (hit.CompareTag("Player"))
            {
                Health_BC health = hit.GetComponent<Health_BC>();
                if (health != null)
                {
                    health.TakeDamage(explosionDamage);
                }
            }
        }
    }

    // 使敌人面向目标
    void FaceTarget(Vector3 targetPos)
    {
        Vector2 direction = (targetPos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // 在编辑器中可视化爆炸范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = (playerTransform != null) ? playerTransform.position : transform.position;
        Gizmos.DrawWireSphere(center, explosionRadius);
    }
}
