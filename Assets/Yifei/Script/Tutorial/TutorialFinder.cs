using UnityEngine;

/// <summary>
/// 教程中目标查找实现，固定查找队友
/// </summary>
public class TutorialTeammateFinder : MonoBehaviour, ITargetFinder
{
    public Transform FindTarget(Transform self)
    {

        float minDistance = Mathf.Infinity;
        Transform nearestTarget = null;

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
        return nearestTarget;
    }
}
