using UnityEngine;

/// <summary>
/// 随机目标查找器：用于产生随机游荡目标
/// </summary>
public class RandomWanderFinder : ITargetFinder
{
    public float wanderRange = 5f; // 游荡范围
    private Transform wanderTarget;
    private float lastUpdateDistance = 0f;

    public RandomWanderFinder(Transform owner)
    {
        // 创建一个空物体作为目标
        GameObject targetObj = new GameObject("WanderTarget");
        wanderTarget = targetObj.transform;
        UpdateWanderTarget(owner.position);
    }

    public Transform FindTarget(Transform self)
    {
        // 当距离当前目标足够近时，更新目标位置
        if (Vector2.Distance(self.position, wanderTarget.position) < 0.5f)
        {
            UpdateWanderTarget(self.position);
        }
        return wanderTarget;
    }

    public void UpdateWanderTarget(Vector3 origin)
    {
        // 生成随机方向，并计算新目标位置
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        wanderTarget.position = origin + (Vector3)(randomDir * wanderRange);
    }

    public void ForceUpdateTarget(Vector3 currentPos)
    {
        UpdateWanderTarget(currentPos);
    }


    // 修改：新增 GetWanderTarget 方法，以便获取当前目标
    public Transform GetWanderTarget()
    {
        return wanderTarget;
    }
}
