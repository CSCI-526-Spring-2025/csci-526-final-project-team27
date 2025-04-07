using UnityEngine;
using System.Collections;

/// <summary>
/// 侧向攻击实现：每次攻击发射两枚子弹，分别偏转一定角度向目标两侧
/// </summary>
public class SideRangedAttacker : IRangedAttacker
{
    // 子弹发射角度偏移量（单位：度），可根据需求调整
    public float angleOffset = 10f;

    public IEnumerator Attack(MonoBehaviour owner, Transform self, Transform target, float attackInterval, GameObject bulletPrefab, Transform firePoint, float bulletSpeed, float bulletLifetime)
    {
        // 计算指向目标的基础方向（单位向量）
        Vector2 baseDirection = (target.position - firePoint.position).normalized;

        // 分别计算左右两侧的子弹发射方向
        Vector2 leftDirection = Quaternion.Euler(0, 0, angleOffset) * baseDirection;
        Vector2 rightDirection = Quaternion.Euler(0, 0, -angleOffset) * baseDirection;

        // 实例化左侧子弹
        GameObject leftBullet = GameObject.Instantiate(bulletPrefab, firePoint.position, 
            Quaternion.Euler(0, 0, Mathf.Atan2(leftDirection.y, leftDirection.x) * Mathf.Rad2Deg));
        Rigidbody2D leftRb = leftBullet.GetComponent<Rigidbody2D>();
        if (leftRb != null)
        {
            leftRb.linearVelocity = leftDirection * bulletSpeed;
        }
        GameObject.Destroy(leftBullet, bulletLifetime);

        // 实例化右侧子弹
        GameObject rightBullet = GameObject.Instantiate(bulletPrefab, firePoint.position, 
            Quaternion.Euler(0, 0, Mathf.Atan2(rightDirection.y, rightDirection.x) * Mathf.Rad2Deg));
        Rigidbody2D rightRb = rightBullet.GetComponent<Rigidbody2D>();
        if (rightRb != null)
        {
            rightRb.linearVelocity = rightDirection * bulletSpeed;
        }
        GameObject.Destroy(rightBullet, bulletLifetime);

        // 等待攻击间隔后再进行下一次攻击
        yield return new WaitForSeconds(attackInterval);
    }
}
