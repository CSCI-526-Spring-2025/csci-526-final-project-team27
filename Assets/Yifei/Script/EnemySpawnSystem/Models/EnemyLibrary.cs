//using System.Collections.Generic;
//using UnityEngine;

//[CreateAssetMenu(menuName = "Enemies/Enemy Library")]
//public class GlobalEnemyLibrary : ScriptableObject
//{
//    public List<EnemyConfig> allEnemies = new List<EnemyConfig>();

//    // 可视化排序按钮
//    public void SortByDifficulty()
//    {
//        allEnemies.Sort((a, b) =>
//            a.difficultyThreshold.CompareTo(b.difficultyThreshold));
//    }
//}

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Enemy Library")]
public class GlobalEnemyLibrary : ScriptableObject
{
    [Tooltip("所有敌人的预制体，每个预制体上必须挂载 EnemyBase 组件")]
    public List<GameObject> enemyPrefabs = new List<GameObject>();

    // 可视化排序按钮，根据 EnemyBase 上的难度阈值排序 
    public void SortByDifficulty()
    {
        enemyPrefabs.Sort((a, b) =>
        {
            float diffA = a.GetComponent<BaseEnemy>()?.difficultyThreshold ?? 0f;
            float diffB = b.GetComponent<BaseEnemy>()?.difficultyThreshold ?? 0f;
            return diffA.CompareTo(diffB);
        });
    }
}