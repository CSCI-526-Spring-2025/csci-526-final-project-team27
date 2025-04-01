// using UnityEngine;

// /// <summary>
// /// 四方向随机目标查找器：用于产生上下左右随机目标
// /// </summary>
// public class FourDirectionWanderFinder : ITargetFinder
// {
//     public float wanderRange = 5f; // 游荡范围
//     private Transform wanderTarget;

//     public FourDirectionWanderFinder(Transform owner)
//     {
//         // 创建一个空物体作为目标
//         GameObject targetObj = new GameObject("FourDirectionWanderTarget");
//         wanderTarget = targetObj.transform;
//         UpdateWanderTarget(owner.position);
//     }

//     public Transform FindTarget(Transform self)
//     {
//         // 当距离当前目标足够近时，更新目标位置
//         if (Vector2.Distance(self.position, wanderTarget.position) < 0.5f)
//         {
//             UpdateWanderTarget(self.position);
//         }
//         return wanderTarget;
//     }

//     private void UpdateWanderTarget(Vector3 origin)
//     {
//         // 从上下左右中随机选择一个方向
//         Vector2[] directions = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
//         Vector2 randomDir = directions[Random.Range(0, directions.Length)];
//         wanderTarget.position = origin + (Vector3)(randomDir * wanderRange);
//     }
// }

using UnityEngine;

/// <summary>
/// 四方向随机目标查找器：用于产生上下左右随机目标
/// </summary>
public class FourDirectionWanderFinder : ITargetFinder
{
    public float wanderRange = 5f;
    private Transform wanderTarget;

    private Vector2 lastDirection = Vector2.zero;  // 上一次移动方向
    private bool reverseNext = false;              // 是否反向移动

    public FourDirectionWanderFinder(Transform owner)
    {
        GameObject targetObj = new GameObject("FourDirectionWanderTarget");
        wanderTarget = targetObj.transform;
        UpdateWanderTarget(owner.position);
    }

    public Transform FindTarget(Transform self)
    {
        if (Vector2.Distance(self.position, wanderTarget.position) < 0.25f)
        {
            UpdateWanderTarget(self.position);
        }
        return wanderTarget;
    }

    private void UpdateWanderTarget(Vector3 origin)
    {
        Vector2[] directions = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        Vector2 chosenDir;

        if (reverseNext && lastDirection != Vector2.zero)
        {
            // 使用上次方向的反方向
            chosenDir = -lastDirection;
            reverseNext = false;
        }
        else
        {
            chosenDir = directions[Random.Range(0, directions.Length)];
        }

        lastDirection = chosenDir;
        wanderTarget.position = origin + (Vector3)(chosenDir * wanderRange);
    }

    public void ForceUpdateTarget(Vector3 currentPos)
    {
        UpdateWanderTarget(currentPos);
    }

    /// <summary>
    /// 碰到墙壁后调用，强制下一次更新为反方向
    /// </summary>
    public void ReverseDirection()
    {
        reverseNext = true;
    }
}
