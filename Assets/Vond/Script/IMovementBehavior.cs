// IMovementBehavior.cs
using UnityEngine;

public interface IMovementBehavior
{
    /// <summary>
    /// ����Ŀ��λ�ú��ƶ��ٶȿ��ƽ�ɫ���ƶ�
    /// </summary>
    /// <param name="target">Ŀ������Transform</param>
    /// <param name="rb">��ɫ��Rigidbody2D</param>
    /// <param name="moveSpeed">�ƶ��ٶ�</param>
    void Move(Transform target, Rigidbody2D rb, float moveSpeed);
}
