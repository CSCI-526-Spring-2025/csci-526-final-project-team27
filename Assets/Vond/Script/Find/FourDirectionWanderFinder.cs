using UnityEngine;

/// <summary>
/// 四方向随机目标查找器：用于产生上下左右随机目标
/// </summary>
public class FourDirectionWanderFinder : ITargetFinder
{
    public float wanderRange = 5f; // 游荡范围
    private Transform wanderTarget;

    public FourDirectionWanderFinder(Transform owner)
    {
        // 创建一个空物体作为目标
        GameObject targetObj = new GameObject("FourDirectionWanderTarget");
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

    private void UpdateWanderTarget(Vector3 origin)
    {
        // 从上下左右中随机选择一个方向
        Vector2[] directions = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        Vector2 randomDir = directions[Random.Range(0, directions.Length)];
        wanderTarget.position = origin + (Vector3)(randomDir * wanderRange);
    }
}
