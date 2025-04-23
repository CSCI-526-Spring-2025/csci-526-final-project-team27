using UnityEngine;

/// <summary>
/// 默认的移动实现：向目标移动
/// </summary>
public class SimpleMover : IMover 
{
    public Animator animator;
    public void Move(Transform self, Rigidbody2D rb, Transform target, float moveSpeed) 
    {
        Vector2 direction = (target.position - self.position).normalized;
        rb.linearVelocity = direction * moveSpeed;

        if (animator != null)
        {
            if (rb.linearVelocity.magnitude > 0.1f)
            {
                animator.SetBool("isWalking", true);
            }
            else
            {
                animator.SetBool("isWalking", false);
            }
        }
    }
}
