using UnityEngine;
using System.Collections;

/// <summary>
/// 远程矩形攻击接口
/// </summary>
public interface IRectAttacker 
{
    /// <summary>
    /// 执行矩形攻击。
    /// 参数 attackWidth 表示矩形宽度，attackLength 表示矩形长度。
    /// </summary>
    IEnumerator Attack(MonoBehaviour owner, Transform self, Transform target, float damage, float attackInterval, float attackWidth, float attackLength, Vector3 center, float angle);
}