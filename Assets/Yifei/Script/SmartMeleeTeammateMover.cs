using UnityEngine;

public class SmartMeleeMover : MonoBehaviour, IMover
{
    // 分离检测半径
    public float separationRadius = 1.0f;
    // 分离力权重
    public float separationForce = 1.0f;
    // 当与目标（玩家）距离小于此值时停止移动，防止抖动
    public float stopDistance = 2f;

    public void Move(Transform self, Rigidbody2D rb, Transform target, float moveSpeed)
    {
        if (target == null) return;

        // 如果目标是玩家且已经足够靠近，则停止移动
        if (target.CompareTag("Player"))
        {
            float distanceToPlayer = Vector2.Distance(self.position, target.position);
            if (distanceToPlayer <= stopDistance)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }
        }

        // 计算指向目标的期望方向
        Vector2 desiredDirection = ((Vector2)target.position - (Vector2)self.position).normalized;

        // 检测附近的队友和主角以计算分离力
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(self.position, separationRadius);
        Vector2 separation = Vector2.zero;
        foreach (Collider2D col in nearbyColliders)
        {
            if (col.transform == self) continue;

            // 当目标是玩家时，忽略玩家产生的排斥力
            if (target.CompareTag("Player") && col.CompareTag("Player"))
                continue;

            // 对于队友或其他主角，根据距离计算排斥向量
            if (col.CompareTag("Teammate") || col.CompareTag("Player"))
            {
                Vector2 diff = (Vector2)self.position - (Vector2)col.transform.position;
                float distance = diff.magnitude;
                if (distance > 0)
                {
                    separation += diff.normalized / distance;
                }
            }
        }

        // 将目标追踪与分离力叠加，并归一化得到最终移动方向
        Vector2 finalDirection = (desiredDirection + separationForce * separation).normalized;

        // 设置刚体的速度
        rb.linearVelocity = finalDirection * moveSpeed;
    }
}
