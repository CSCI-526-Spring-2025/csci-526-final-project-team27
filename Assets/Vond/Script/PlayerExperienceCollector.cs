using UnityEngine;

public class PlayerExperienceCollector : MonoBehaviour
{
    [Header("吸收设置")]
    [Tooltip("检测范围半径，范围内的经验球会被吸入玩家体内")]
    public float detectionRadius = 5f;
    
    [Tooltip("检测经验球的Layer Mask")]
    public LayerMask orbLayerMask;

    private void Update()
    {
        // 每帧检测玩家周围的经验球
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, orbLayerMask);
        foreach (var hit in hits)
        {
            ExperienceOrb orb = hit.GetComponent<ExperienceOrb>();
            if (orb != null)
            {
                // 启动经验球吸附逻辑，将玩家传给经验球
                orb.StartAttraction(transform);
            }
        }
    }

    // 调试时显示检测范围
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}

