using UnityEngine;
using System.Collections;

/// <summary>
/// 默认的攻击实现：生成 Hitbox 并造成伤害
/// </summary>
public class TeammateMeleeAttacker : ITeammateMelee 
{
    public IEnumerator Attack(MonoBehaviour owner, Transform self, Transform target, float damage, GameObject hitboxPrefab, float hitboxOffset, float attackCooldown) 
    {
        // 计算攻击方向和 Hitbox 的生成位置
        Vector2 attackDirection = (target.position - self.position).normalized;
        Vector2 spawnPos = (Vector2)self.position + attackDirection * hitboxOffset;
        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        // 实例化 Hitbox
        if (hitboxPrefab != null) 
        {
            GameObject.Instantiate(hitboxPrefab, spawnPos, rotation);
        }

        // 对目标造成伤害
        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth != null) 
        {
            Debug.Log($"{self.gameObject.name} 攻击 {target.name} 造成 {damage} 点伤害");
            targetHealth.TakeDamage(damage);
        }
        else 
        {
            Debug.LogWarning("目标缺少 Health 组件，无法造成伤害");
        }

        // 攻击后等待冷却时间
        yield return new WaitForSeconds(attackCooldown);
    }
}
