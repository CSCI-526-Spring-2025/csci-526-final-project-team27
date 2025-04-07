using UnityEngine;
using System.Collections;

/// <summary>
/// 追踪远程攻击实现：发射带追踪功能的子弹
/// </summary>
public class TrackRangedAttacker : IRangedAttacker
{
    // 追踪力度，可根据需要调节
    public float homingStrength = 2f;

    public IEnumerator Attack(MonoBehaviour owner, Transform self, Transform target, float attackInterval, GameObject bulletPrefab, Transform firePoint, float bulletSpeed, float bulletLifetime)
    {
        // 实例化追踪子弹（初始旋转可设为默认）
        GameObject bullet = GameObject.Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        
        // 尝试获取 TrackingBullet 脚本，初始化追踪参数
        TrackingBullet tb = bullet.GetComponent<TrackingBullet>();
        if (tb != null)
        {
            tb.Initialize(target, bulletSpeed, homingStrength);
        }
        else
        {
            // 如果没有找到追踪脚本，则退化为普通子弹
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                Vector2 attackDirection = (target.position - firePoint.position).normalized;
                bulletRb.linearVelocity = attackDirection * bulletSpeed;
            }
        }

        // 销毁子弹，保证存在时间
        GameObject.Destroy(bullet, bulletLifetime);
        yield return new WaitForSeconds(attackInterval);
    }
}
