using System.Collections;
using UnityEngine;

/// <summary>
/// 模拟史莱姆跳跃（抛物线轨迹）的移动行为组件：
/// 在二维平面（X/Y）上跳跃，通过在Y轴方向添加一个抛物线偏移来模拟真实跳跃效果。 
/// 如果目标在跳跃距离内，则直接跳到目标位置；否则跳跃固定距离。
/// </summary>
public class ParabolicSlimeJumpMovementBehavior2D : MonoBehaviour, IMover
{

    [Tooltip("跳跃的总时长（秒）")]
    [SerializeField] private float jumpDuration = 0.7f;

    [Tooltip("跳跃时的最大高度（抛物线顶点处的偏移值）")]
    [SerializeField] private float maxJumpHeight = 1f;

    [Tooltip("两次跳跃之间的间隔（秒）")]
    [SerializeField] private float jumpInterval = 1.5f;

    private float lastJumpTime = 0f;
    private bool isJumping = false;
    private float jumpDistance = 5f;

    /// <summary>
    /// 根据目标方向发起一次跳跃。
    /// 跳跃距离由 moveSpeed 决定。
    /// </summary>
    /// <param name="self">当前角色的 Transform（使用传入的 self 代替内部 transform）</param>
    /// <param name="rb">角色的 Rigidbody2D（本实现中未直接使用）</param>
    /// <param name="target">目标对象的 Transform</param>
    /// <param name="moveSpeed">移动速度参数，决定跳跃距离</param>
    public void Move(Transform self, Rigidbody2D rb, Transform target, float moveSpeed)
    {
        if (target == null) return;
        if (!isJumping && Time.time - lastJumpTime >= jumpInterval)
        {
            jumpDistance = moveSpeed; // 使用 moveSpeed 作为跳跃距离
            StartCoroutine(JumpTowardsTarget(self, target));
        }
    }

    void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(0, 0, 180);
    }


    /// <summary>
    /// 协程：执行一次抛物线跳跃，并在二维平面上更新角色位置。
    /// 如果目标在跳跃距离内，则跳到目标位置；否则按照固定跳跃距离计算落点。
    /// </summary>
    /// <param name="self">当前角色的 Transform</param>
    /// <param name="target">目标 Transform</param>
    /// <returns></returns>
    private IEnumerator JumpTowardsTarget(Transform self, Transform target)
    {
        isJumping = true;
        lastJumpTime = Time.time;

        // 记录跳跃起点（使用传入的 self 位置）
        Vector2 startPos = self.position;
        // 计算目标与起点的距离
        float targetDistance = Vector2.Distance(startPos, (Vector2)target.position);
        // 如果目标距离小于跳跃距离，则实际跳跃距离为目标距离，否则为设定的跳跃距离
        float actualJumpDistance = targetDistance < jumpDistance ? targetDistance : jumpDistance;
        // 计算从起点到目标的方向（标准化）
        Vector2 direction = ((Vector2)target.position - startPos).normalized;
        // 根据实际跳跃距离计算落点
        Vector2 endPos = startPos + direction * actualJumpDistance;

        float elapsed = 0f;
        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            // 归一化时间 t ∈ [0,1]
            float t = Mathf.Clamp01(elapsed / jumpDuration);
            // 先用线性插值计算水平位置
            Vector2 pos = Vector2.Lerp(startPos, endPos, t);
            // 使用简单二次函数模拟抛物线：t*(1-t)在 t=0 与 t=1 时为 0，在 t=0.5 时取得最大值
            float verticalOffset = 4 * maxJumpHeight * t * (1 - t);
            pos.y += verticalOffset;

            self.position = pos;
            yield return null;
        }

        // 确保跳跃结束时位置精确落在预定的落点
        self.position = endPos;
        isJumping = false;
    }
}
