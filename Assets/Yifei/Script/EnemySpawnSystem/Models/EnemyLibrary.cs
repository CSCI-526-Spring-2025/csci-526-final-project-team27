using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Enemy Library")]
public class GlobalEnemyLibrary : ScriptableObject
{
    public List<EnemyConfig> allEnemies = new List<EnemyConfig>();

    // 可视化排序按钮
    public void SortByDifficulty()
    {
        allEnemies.Sort((a, b) =>
            a.difficultyThreshold.CompareTo(b.difficultyThreshold));
    }
}