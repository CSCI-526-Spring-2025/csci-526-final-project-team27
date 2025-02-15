using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class RangedEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;          // 追踪玩家的移动速度
    public float detectionInterval = 0.5f; // 目标检测间隔

    [Header("Attack Settings")]
    public float attackRange = 5f;        // 远程攻击射程
    public float attackInterval = 1.5f;   // 两次攻击之间的间隔
    public GameObject bulletPrefab;       // 子弹预制体（可复用 ShootingController 中的子弹）
    public Transform firePoint;           // 子弹发射位置（一般设置在敌人身上合适的位置）
    public float bulletSpeed = 10f;       // 子弹移动速度
    public float bulletLifetime = 2f;     // 子弹存在时间

    private Transform playerTransform;    // 玩家目标
    private Rigidbody2D rb;
    private bool canAttack = true;          // 攻击冷却标志

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 获取玩家目标（假定玩家 Tag 为 "Player"）
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj)
        {
            playerTransform = playerObj.transform;
        }

        // 定时更新目标（防止玩家移动太快时出现延迟）
        StartCoroutine(UpdateTargetRoutine());
    }

    void Update()
    {
        if (playerTransform == null)
            return;

        // 计算敌人与玩家的距离
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        // 当玩家在射程内时：停止移动、面向玩家并攻击
        if (distance <= attackRange)
        {
            // 停止移动
            rb.linearVelocity = Vector2.zero;

            // 让敌人面向玩家
            FaceTarget(playerTransform.position);

            // 攻击（如果冷却允许）
            if (canAttack)
            {
                StartCoroutine(Attack());
            }
        }
        else // 玩家不在射程内时：继续追踪
        {
            MoveTowardsTarget();
        }
    }

    // 定时更新目标（此处简单更新目标，可扩展为多目标搜索）
    IEnumerator UpdateTargetRoutine()
    {
        while (true)
        {
            if (playerTransform == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj)
                {
                    playerTransform = playerObj.transform;
                }
            }
            yield return new WaitForSeconds(detectionInterval);
        }
    }

    // 移动追踪玩家
    void MoveTowardsTarget()
    {
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;

        // 使敌人始终面向移动方向（可选）
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // 发射子弹攻击玩家
    IEnumerator Attack()
    {
        canAttack = false;

        // 计算发射方向：从 firePoint 指向玩家
        Vector2 attackDirection = (playerTransform.position - firePoint.position).normalized;
        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        // 实例化子弹
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, rotation);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = attackDirection * bulletSpeed;
        }
        // 销毁子弹
        Destroy(bullet, bulletLifetime);

        // 等待攻击间隔
        yield return new WaitForSeconds(attackInterval);
        canAttack = true;
    }

    // 使敌人面向指定目标
    void FaceTarget(Vector3 targetPos)
    {
        Vector2 direction = (targetPos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
