using UnityEngine;
using System.Collections;

/// <summary>
/// 远程扇形喷雾攻击接口
/// </summary>
public interface ISprayAttacker
{
    /// <summary>
    /// 执行扇形喷雾攻击。
    /// 参数 sprayAngle 为喷雾扇形角度，sprayRange 为喷雾半径。
    /// </summary>
    IEnumerator Attack(MonoBehaviour owner, Transform self, Transform target, float damage, float attackInterval, float sprayAngle, float sprayRange);
}