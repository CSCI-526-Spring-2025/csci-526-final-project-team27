using UnityEngine;
using System.Collections;

/// <summary>
/// 远程攻击行为接口，定义了发射子弹的攻击方法
/// </summary>
public interface IRangedAttacker 
{
    IEnumerator Attack(MonoBehaviour owner, Transform self, Transform target, float attackInterval, GameObject bulletPrefab, Transform firePoint, float bulletSpeed, float bulletLifetime);
}
