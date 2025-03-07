//暂时废弃


//using UnityEngine;
//using System.Collections.Generic;

//public enum RoomType { Combat, Boss, Elite, Other }

//[System.Serializable]
//public class SpawnEntry
//{
//    public GameObject enemyPrefab;       // 敌人预制体
//    public int count = 1;                // 本条目生成该敌人的数量
//    public bool useRandomSpawnPoints = false;  // 是否使用随机位置刷怪（true 则随机，否则使用固定点）
//    public float spawnInterval = 0f;     // 同一条目中每个敌人生成的间隔（0表示同时生成）
//}

//[System.Serializable]
//public class Wave
//{
//    public List<SpawnEntry> spawnEntries = new List<SpawnEntry>();
//    public float nextWaveDelay = 1f;   // 该波结束到下一波开始的延迟时间
//}

//[CreateAssetMenu(fileName = "WaveConfig", menuName = "ScriptableObjects/WaveConfig", order = 1)]
//public class WaveConfig : ScriptableObject
//{
//    public RoomType roomType;                   // 适用的房间类型（战斗房、Boss房等）
//    public List<Wave> waves = new List<Wave>(); // 波次列表
//}
//// 设计人员可以在 Unity 编辑器中创建多个 WaveConfig 资产，并编辑每波次的敌人类型、数量等数据。
