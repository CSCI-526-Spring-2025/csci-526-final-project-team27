// IMovementBehavior.cs
using UnityEngine;

public interface IMovementBehavior
{
    /// <summary>
    /// 根据目标位置和移动速度控制角色的移动
    /// </summary>
    /// <param name="target">目标对象的Transform</param>
    /// <param name="rb">角色的Rigidbody2D</param>
    /// <param name="moveSpeed">移动速度</param>
    void Move(Transform target, Rigidbody2D rb, float moveSpeed);
}
