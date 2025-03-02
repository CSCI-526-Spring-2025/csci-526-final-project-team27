using UnityEngine;

/// <summary>
/// 查找目标的接口
/// </summary>
public interface ITargetFinder 
{
    Transform FindTarget(Transform self);
}
