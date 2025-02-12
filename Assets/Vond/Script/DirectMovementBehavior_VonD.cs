using UnityEngine;

/// <summary>
/// Ĭ�ϵ�ֱ���ƶ���Ϊ��ʵ���� IMovementBehavior �ӿڡ�
/// �������ʹ��ɫ��Ŀ�귽���ƶ�����ͬʱ����ˮƽ����ʹ�ֱ������ٶȡ�
/// </summary>
public class DirectMovementBehavior_VonD : MonoBehaviour, IMovementBehavior
{
    public void Move(Transform target, Rigidbody2D rb, float moveSpeed)
    {
        if (target == null) return;

        // ����ӽ�ɫ��ǰλ��ָ��Ŀ��λ�õķ�������������һ��
        Vector2 direction = ((Vector2)target.position - rb.position).normalized;

        // ֱ�����ø����ٶȣ�ʹ��ɫ��Ŀ���ƶ�
        rb.linearVelocity = direction * moveSpeed;
    }
}
