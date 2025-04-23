using UnityEngine;
using System.Collections;

public class EnemyFanSprayAttacker : ISprayAttacker 
{
    public IEnumerator Attack(MonoBehaviour owner, Transform self, Transform target, float damage, float attackInterval, float sprayAngle, float sprayRange, bool rotate = true)
    {

        // 计算敌人到目标的方向，并获得面向角度
        Vector3 direction = (target.position - self.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        // 使敌人面向目标（以 self.right 为正方向）
        if (rotate)
        {
            self.rotation = Quaternion.Euler(0, 0, angle);
        }
        // self.rotation = Quaternion.Euler(0, 0, angle);

        // 查找喷雾范围内的所有碰撞体（以敌人当前位置为圆心）
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(self.position, sprayRange);
        foreach (var hit in hitColliders)
        {
            // 排除自身，避免自伤
            if (hit.transform == self)
                continue;

            // 计算从敌人到检测目标的方向，并判断是否在喷雾扇形内
            Vector2 toHit = (hit.transform.position - self.position).normalized;
            float deltaAngle = Vector2.Angle(self.right, toHit);
            if (deltaAngle <= sprayAngle / 2f)
            {
                // 对具有 Health 组件的对象造成伤害
                Health health = hit.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                }
            }
        }
        yield return new WaitForSeconds(attackInterval);
    }
}