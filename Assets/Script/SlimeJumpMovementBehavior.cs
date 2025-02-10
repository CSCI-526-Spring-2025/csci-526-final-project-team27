using System.Collections;
using UnityEngine;

/// <summary>
/// ģ��ʷ��ķ��Ծ�������߹켣�����ƶ���Ϊ�����
/// �ڶ�άƽ�棨X/Y������Ծ��ͨ����Y�᷽�����һ��������ƫ����ģ����ʵ��ԾЧ����
/// ���Ŀ������Ծ�����ڣ���ֱ������Ŀ��λ�ã�������Ծ�̶����롣
/// </summary>
public class ParabolicSlimeJumpMovementBehavior2D : MonoBehaviour, IMovementBehavior
{
    [Header("Jump Settings")]
    [Tooltip("ÿ����Ծˮƽ������Ŀ�귽�򣩵�������")]
    [SerializeField] private float jumpDistance = 3f;

    [Tooltip("��Ծ����ʱ�����룩")]
    [SerializeField] private float jumpDuration = 0.7f;

    [Tooltip("��Ծʱ�����߶ȣ������߶��㴦��ƫ��ֵ��")]
    [SerializeField] private float maxJumpHeight = 1f;

    [Tooltip("������Ծ֮��ļ�����룩")]
    [SerializeField] private float jumpInterval = 1.5f;

    private float lastJumpTime = 0f;
    private bool isJumping = false;

    /// <summary>
    /// ����Ŀ�귽����һ����Ծ��
    /// </summary>
    /// <param name="target">Ŀ������Transform</param>
    /// <param name="rb">��ɫ��Rigidbody2D����ʵ����δֱ��ʹ�ã�</param>
    /// <param name="moveSpeed">�ƶ��ٶȲ�������ʵ���в�ʹ�ã���Ծ������jumpDistance���ƣ�</param>
    public void Move(Transform target, Rigidbody2D rb, float moveSpeed)
    {
        if (target == null) return;
        if (!isJumping && Time.time - lastJumpTime >= jumpInterval)
        {
            StartCoroutine(JumpTowardsTarget(target));
        }
    }

    /// <summary>
    /// Э�̣�ִ��һ����������Ծ�����ڶ�άƽ���ϸ��½�ɫλ�á�
    /// ���Ŀ������Ծ�����ڣ�������Ŀ��λ�ã������չ̶���Ծ���������㡣
    /// </summary>
    /// <param name="target">Ŀ��Transform</param>
    /// <returns></returns>
    private IEnumerator JumpTowardsTarget(Transform target)
    {
        isJumping = true;
        lastJumpTime = Time.time;

        // ��¼��Ծ��㣨��ǰ��άƽ��λ�ã�
        Vector2 startPos = transform.position;
        // ����Ŀ�������ľ���
        float targetDistance = Vector2.Distance(startPos, (Vector2)target.position);
        // ���Ŀ�����С����Ծ���룬��ʵ����Ծ����ΪĿ����룬����Ϊ�趨����Ծ����
        float actualJumpDistance = targetDistance < jumpDistance ? targetDistance : jumpDistance;
        // �������㵽Ŀ��ķ��򣨱�׼����
        Vector2 direction = ((Vector2)target.position - startPos).normalized;
        // ����ʵ����Ծ����������
        Vector2 endPos = startPos + direction * actualJumpDistance;

        float elapsed = 0f;
        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            // ��һ��ʱ�� t �� [0,1]
            float t = Mathf.Clamp01(elapsed / jumpDuration);
            // �������Բ�ֵ����ˮƽλ��
            Vector2 pos = Vector2.Lerp(startPos, endPos, t);
            // ʹ�ü򵥶��κ���ģ�������ߣ�t*(1-t)�� t=0 �� t=1 ʱΪ 0���� t=0.5 ʱȡ�����ֵ
            float verticalOffset = 4 * maxJumpHeight * t * (1 - t);
            // ��ƫ�Ƽӵ�Y���ϣ�ģ����Ծ����
            pos.y += verticalOffset;

            transform.position = pos;
            yield return null;  // ��ִͣ�У��ȴ���һ֡�ټ���
        }

        // ȷ����Ծ����ʱλ�þ�ȷ����Ԥ�������
        transform.position = endPos;
        isJumping = false;
    }
}
