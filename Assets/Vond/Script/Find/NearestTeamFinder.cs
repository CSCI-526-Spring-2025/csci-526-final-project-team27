using UnityEngine;

/// <summary>
/// MeleeEnemy 的目标查找实现
/// </summary>
public class NearestTeamFinder : ITargetFinder
{
    public Transform FindTarget(Transform self)
    {
        // 获取 Player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Transform playerTransform = playerObj ? playerObj.transform : null;

        // 在 Player 与 Teammate 中选取最近者
        float minDistance = Mathf.Infinity;
        Transform nearestTarget = playerTransform;

        // 查找所有 Teammate
        GameObject[] teammates = GameObject.FindGameObjectsWithTag("Teammate");
        foreach (GameObject teammate in teammates)
        {
            if (teammate == null)
                continue;
            float distance = Vector2.Distance(self.position, teammate.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestTarget = teammate.transform;
            }
        }
        if (playerTransform != null)
        {
            float playerDistance = Vector2.Distance(self.position, playerTransform.position);
            if (playerDistance < minDistance)
            {
                nearestTarget = playerTransform;
            }
        }
        return nearestTarget;
    }
}
