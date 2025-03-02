using UnityEngine;

/// <summary>
/// 移动行为的接口
/// </summary>
public interface IMover 
{
    void Move(Transform self, Rigidbody2D rb, Transform target, float moveSpeed);
}
