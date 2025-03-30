using UnityEngine;

public class ArrowIndicator : MonoBehaviour
{
    [Header("移动参数")]
    [Tooltip("沿箭头方向的移动距离")]
    public float moveDistance = 1.0f;
    [Tooltip("移动速度")]
    public float moveSpeed = 1.0f;

    // 记录初始位置
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // PingPong 返回一个在 0 ~ moveDistance 范围内往复变化的值
        float offset = Mathf.PingPong(Time.time * moveSpeed, moveDistance);
        // 始终沿着 transform.up（箭头当前朝向）移动
        transform.position = startPos + transform.up * offset;
    }
}
