using UnityEngine;
using System.Collections;

/// <summary>
/// 远程攻击的默认实现：从发射点生成子弹，并赋予其速度，等待攻击冷却后返回
/// </summary>
public class RangedAttacker : IRangedAttacker 
{
    public float delayTime = 0;
    public IEnumerator Attack(MonoBehaviour owner, Transform self, Transform target, float attackInterval, GameObject bulletPrefab, Transform firePoint, float bulletSpeed, float bulletLifetime)
    {
        // 等待延迟时间
        if (delayTime > 0)
        {
            yield return new WaitForSeconds(delayTime);
        }
        // 如果目标为空，直接返回
        if (target == null) yield break;

        // 计算从 firePoint 到目标的方向
        Vector2 attackDirection = (target.position - firePoint.position).normalized;
        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        
        // 实例化子弹
        GameObject bullet = GameObject.Instantiate(bulletPrefab, firePoint.position, rotation);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = attackDirection * bulletSpeed;
        }
        GameObject.Destroy(bullet, bulletLifetime);

        // 等待下一次攻击
        yield return new WaitForSeconds(attackInterval - delayTime);
    }
}
