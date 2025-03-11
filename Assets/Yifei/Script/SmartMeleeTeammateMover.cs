using UnityEngine;

public class SmartMeleeMover : MonoBehaviour, IMover
{
    // ������뾶
    public float separationRadius = 1.0f;
    // ������Ȩ��
    public float separationForce = 1.0f;
    // ����Ŀ�꣨��ң�����С�ڴ�ֵʱֹͣ�ƶ�����ֹ����
    public float stopDistance = 2f;

    public void Move(Transform self, Rigidbody2D rb, Transform target, float moveSpeed)
    {
        if (target == null) return;

        // ���Ŀ����������Ѿ��㹻��������ֹͣ�ƶ�
        if (target.CompareTag("Player"))
        {
            float distanceToPlayer = Vector2.Distance(self.position, target.position);
            if (distanceToPlayer <= stopDistance)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }
        }

        // ����ָ��Ŀ�����������
        Vector2 desiredDirection = ((Vector2)target.position - (Vector2)self.position).normalized;

        // ��⸽���Ķ��Ѻ������Լ��������
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(self.position, separationRadius);
        Vector2 separation = Vector2.zero;
        foreach (Collider2D col in nearbyColliders)
        {
            if (col.transform == self) continue;

            // ��Ŀ�������ʱ��������Ҳ������ų���
            if (target.CompareTag("Player") && col.CompareTag("Player"))
                continue;

            // ���ڶ��ѻ��������ǣ����ݾ�������ų�����
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

        // ��Ŀ��׷������������ӣ�����һ���õ������ƶ�����
        Vector2 finalDirection = (desiredDirection + separationForce * separation).normalized;

        // ���ø�����ٶ�
        rb.linearVelocity = finalDirection * moveSpeed;
    }
}
