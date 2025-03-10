using UnityEngine;
using System.Collections;
// public class EnemyRectAttackAttacker : IRectAttacker 
// {
//     public IEnumerator Attack(MonoBehaviour owner, Transform self, Transform target, float damage, float attackInterval, float attackWidth, float attackLength)
//     {
//         // 假设敌人已面向目标（self.right 为攻击方向）
//         // 计算矩形区域：中心位置为敌人当前位置向前偏移 attackLength/2
//         Vector2 center = (Vector2)self.position + (Vector2)self.right * (attackLength / 2f);
//         Vector2 size = new Vector2(attackLength, attackWidth);
//         float angle = self.eulerAngles.z;
        
//         // 检测矩形区域内所有碰撞体
//         Collider2D[] hitColliders = Physics2D.OverlapBoxAll(center, size, angle);
//         foreach (var hit in hitColliders)
//         {
//             // 排除自身，防止自伤
//             if (hit.transform == self)
//                 continue;

//             Health health = hit.GetComponent<Health>();
//             if (health != null)
//             {
//                 health.TakeDamage(damage);
//             }
//         }
//         yield return new WaitForSeconds(attackInterval);
//     }
// }
public class EnemyRectAttackAttacker : IRectAttacker 
{
    public IEnumerator Attack(MonoBehaviour owner, Transform self, Transform target, float damage, float attackInterval, float attackWidth, float attackLength)
    {
        // 假设敌人已面向目标（self.right 为攻击方向）
        // 矩形区域中心位置：敌人位置向前偏移 attackLength/2
        Vector2 center = (Vector2)self.position + (Vector2)self.right * (attackLength / 2f);
        Vector2 size = new Vector2(attackLength, attackWidth);
        float angle = self.eulerAngles.z;
        
        // 检测矩形区域内所有碰撞体
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(center, size, angle);
        foreach (var hit in hitColliders)
        {
            // 排除自身，防止自伤
            if (hit.transform == self)
                continue;

            Health health = hit.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
        yield return new WaitForSeconds(attackInterval);
    }
}