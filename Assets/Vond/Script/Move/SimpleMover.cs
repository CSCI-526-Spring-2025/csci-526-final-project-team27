using UnityEngine;

/// <summary>
/// 默认的移动实现：向目标移动
/// </summary>
public class SimpleMover : IMover 
{
    public void Move(Transform self, Rigidbody2D rb, Transform target, float moveSpeed) 
    {
        Vector2 direction = (target.position - self.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }
}
