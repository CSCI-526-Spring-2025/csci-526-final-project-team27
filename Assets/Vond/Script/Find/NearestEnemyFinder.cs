using UnityEngine;

/// <summary>
/// 默认的目标查找器：查找场景中最近的“Enemy”
/// </summary>
public class NearestEnemyFinder : ITargetFinder 
{
    public Transform FindTarget(Transform self) 
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float minDistance = Mathf.Infinity;
        Transform nearest = null;
        foreach (GameObject enemy in enemies) 
        {
            if (enemy == null)
                continue;
            float d = Vector2.Distance(self.position, enemy.transform.position);
            if (d < minDistance) 
            {
                minDistance = d;
                nearest = enemy.transform;
            }
        }
        return nearest;
    }
}
