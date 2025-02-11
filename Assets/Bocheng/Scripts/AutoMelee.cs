using UnityEngine;
using System.Collections;

public class AutoMelee : MonoBehaviour
{
    public float detectionRadius = 5f;  // 检测范围
    public string enemyTag = "Enemy";   // 目标敌人Tag
    public GameObject projectilePrefab; // 攻击预制体
    public float attackInterval = 1.0f; // 攻击间隔
    public float offset = 1.0f;         // 生成子弹的偏移量
    public LayerMask enemyLayer;        // 仅检测敌人层

    private Transform targetEnemy;
    private bool canAttack = true;

    void Update()
    {
        FindNearestEnemy();

        if (targetEnemy != null && canAttack)
        {
            StartCoroutine(Attack());
        }
    }

    // 找到最近的敌人
    void FindNearestEnemy()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);
        float minDistance = Mathf.Infinity;
        targetEnemy = null;

        foreach (Collider2D enemy in enemies)
        {
            if (!enemy.CompareTag(enemyTag)) continue; // 确保匹配Tag

            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                targetEnemy = enemy.transform;
            }
        }
    }

    // 攻击协程
    IEnumerator Attack()
    {
        canAttack = false;

        if (targetEnemy != null)
        {
            Vector2 attackDirection = (targetEnemy.position - transform.position).normalized;
            Vector2 spawnPosition = (Vector2)transform.position + attackDirection * offset;

            // 计算旋转角度，让X轴朝向敌人
            float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            Instantiate(projectilePrefab, spawnPosition, rotation);

        }

        yield return new WaitForSeconds(attackInterval);
        canAttack = true;
    }

    // 可视化检测范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
