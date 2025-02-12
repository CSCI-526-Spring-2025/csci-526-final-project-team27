using UnityEngine;

/// <summary>
/// 默认的直接移动行为，实现了 IMovementBehavior 接口。
/// 该组件会使角色向目标方向移动，并同时更新水平方向和垂直方向的速度。
/// </summary>
public class DirectMovementBehavior_VonD : MonoBehaviour, IMovementBehavior
{
    public void Move(Transform target, Rigidbody2D rb, float moveSpeed)
    {
        if (target == null) return;

        // 计算从角色当前位置指向目标位置的方向向量，并归一化
        Vector2 direction = ((Vector2)target.position - rb.position).normalized;

        // 直接设置刚体速度，使角色向目标移动
        rb.linearVelocity = direction * moveSpeed;
    }
}
