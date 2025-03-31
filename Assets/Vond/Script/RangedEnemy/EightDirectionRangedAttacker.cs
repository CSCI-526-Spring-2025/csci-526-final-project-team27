using UnityEngine;
using System.Collections;

/// <summary>
/// 八方向远程攻击实现：一次发射八个方向的子弹
/// </summary>
public class EightDirectionRangedAttacker : IRangedAttacker
{
    public IEnumerator Attack(MonoBehaviour owner, Transform self, Transform target, float attackInterval, GameObject bulletPrefab, Transform firePoint, float bulletSpeed, float bulletLifetime)
    {
        // 发射八个方向，角度间隔45度
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;
            // 以角度生成子弹旋转角度
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            GameObject bullet = GameObject.Instantiate(bulletPrefab, firePoint.position, rotation);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                // 根据角度计算子弹运动方向
                Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                bulletRb.linearVelocity = direction * bulletSpeed;
            }
            // 保证子弹在一定时间后销毁
            GameObject.Destroy(bullet, bulletLifetime);
        }
        // 等待下一次攻击
        yield return new WaitForSeconds(attackInterval);
    }
}
