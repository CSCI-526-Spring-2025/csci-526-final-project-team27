using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeleeEnemy))]
public class MeleeEnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("当敌人与目标距离小于该值时发起攻击")]
    public float attackRange = 1.5f;
    [Tooltip("两次攻击之间的间隔")]
    public float attackInterval = 1.0f;
    [Tooltip("Hitbox 预制体，内置 Hitbox 脚本实现伤害和击退效果")]
    public GameObject hitboxPrefab;
    [Tooltip("Hitbox 生成时相对于敌人位置的偏移量（沿着攻击方向）")]
    public float hitboxOffset = 1.0f;

    public float damage = 10.0f;

    private MeleeEnemy meleeEnemy;
    private bool canAttack = true;

    void Start()
    {
        meleeEnemy = GetComponent<MeleeEnemy>();
    }

    void Update()
    {
        // 确保已有目标（这里假定你已将 MeleeEnemy 中的 currentTarget 改为 public 或提供了访问接口）
        if (meleeEnemy.currentTarget != null && canAttack)
        {
            float distance = Vector2.Distance(transform.position, meleeEnemy.currentTarget.position);
            if (distance <= attackRange)
            {
                // 攻击前先面向目标
                FaceTarget(meleeEnemy.currentTarget.position);
                StartCoroutine(Attack());
            }
        }
    }

    IEnumerator Attack()
    {
        canAttack = false;

        meleeEnemy.isAttacking = true;

        // 计算攻击方向
        Vector2 attackDirection = (meleeEnemy.currentTarget.position - transform.position).normalized;
        // 计算生成 Hitbox 的位置（沿攻击方向偏移一定距离）
        Vector2 spawnPosition = (Vector2)transform.position + attackDirection * hitboxOffset;
        // 计算旋转角度，使 Hitbox 的右侧朝向目标（Hitbox 内部使用 transform.right 作为击退方向）
        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        // 实例化 Hitbox
        Instantiate(hitboxPrefab, spawnPosition, rotation);

        Health targetHealth = meleeEnemy.currentTarget.GetComponent<Health>();
        if (targetHealth != null)
        {
            //Debug.Log($"{this.gameObject.name} 攻击 {meleeEnemy.currentTarget.name} 造成 {damage} 点伤害"); 
            targetHealth.TakeDamage(damage);
        }
        else
        {
            Debug.LogWarning("目标缺少 Health 组件，无法造成伤害");
        }

        // 等待攻击间隔
        yield return new WaitForSeconds(attackInterval);

        meleeEnemy.isAttacking = false;

        canAttack = true;
    }

    // 使敌人面向目标（旋转 Z 轴）
    void FaceTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
