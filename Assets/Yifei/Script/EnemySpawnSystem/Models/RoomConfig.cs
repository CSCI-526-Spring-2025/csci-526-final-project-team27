using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rooms/Room")]
public class RoomConfig : ScriptableObject
{
    public enum RoomType { Normal, Elite, Boss }
    public enum SpawnMode { Dynamic, Static }

    [Header("房间预制体")]
    public GameObject roomPrefab;

    [Header("基础设置")]
    public RoomType roomType;
    public SpawnMode spawnMode;

    [Header("敌人动态生成")]
    [Tooltip("动态生成时的生成数量范围最小值")]
    //public Vector2Int dynamicSpawnRange = new Vector2Int(5, 8);
    public int dynamicSpawnRangeMin = 1;
    [Tooltip("动态生成时的生成数量范围最大值")]
    public int dynamicSpawnRangeMax = 10;


    [Tooltip("随难度变化的生成数量曲线")] // X轴: 标准化难度(0-1) Y轴: 数量乘数
    public AnimationCurve difficultyCurve = AnimationCurve.Linear(0, 1, 1, 1);


    [Tooltip("生成位置（仅随机或边缘）")]
    public SpawnPosition dynamicSpawnPosition = SpawnPosition.Random;


    [Header("敌人静态波次")]
    public List<WaveGroup> waveGroups = new List<WaveGroup>();

    // 后续加入地形控制？
    //[Header("地形设置")]
    //public TerrainType allowedTerrain;

    //[System.Serializable]
    //public class TerrainType
    //{
    //    public bool allowWall = true;
    //    public bool allowWater;
    //    public bool allowLava;
    //}

    //// 在生成位置检测时
    //private bool IsValidSpawnPosition(Vector2 pos)
    //{
    //    Collider2D[] colliders = Physics2D.OverlapCircleAll(pos, 0.5f);
    //    foreach (var col in colliders)
    //    {
    //        if (col.CompareTag("Obstacle") && !allowedTerrain.allowWall)
    //            return false;
    //    }
    //    return true;
    //}

    [System.Serializable]
    public class WaveGroup
    {
        [Tooltip("波次名称（可选）")]
        public string waveName = "Wave";

        [Tooltip("波次开始前的延迟")]
        [Min(0)] public float preDelay;

        [Tooltip("敌人生成单元")]
        public List<EnemySpawnUnit> units = new List<EnemySpawnUnit>();

        //[HideInInspector]
        //public bool isExpanded = true; // 编辑器用
    }

    [System.Serializable]
    public class EnemySpawnUnit
    {
        [Tooltip("敌人类型配置")]
        public EnemyConfig enemy;

        [Tooltip("生成数量")]
        [Min(1)] public int count = 3;

        [Tooltip("生成间隔（秒）")]
        [Min(0)] public float interval = 0.5f;

        [Tooltip("生成位置模式")]
        public SpawnPosition positionType;

        [Tooltip("生成位置列表（相对房间中心）")]
        public List<Vector2> spawnPositions = new List<Vector2>();
    }

    public enum SpawnPosition
    {
        [Tooltip("房间内随机位置")]
        Random,
        [Tooltip("使用预设生成点")]
        FixedPoints,
        [Tooltip("房间边缘")]
        Perimeter
    }
}