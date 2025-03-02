using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rooms/Room")]
public class RoomConfig : ScriptableObject
{
    public enum RoomType { Normal, Elite, Boss }
    public enum SpawnMode { Dynamic, Static }

    [Header("基础设置")]
    public RoomType roomType;
    public SpawnMode spawnMode;

    [Header("敌人动态生成")]
    [Tooltip("动态生成时的生成数量范围")]
    public Vector2Int dynamicSpawnRange = new Vector2Int(5, 8);
    /*
     * x: 最低生成数量（基础值）
     * y: 最高生成数量（基础值）
     * 实际数量会根据难度曲线调整
     */

    [Tooltip("随难度变化的生成数量曲线")]
    public AnimationCurve difficultyCurve = AnimationCurve.Linear(0, 1, 1, 1);
    /*
     * X轴：标准化后的当前难度（0-1）
     * Y轴：生成数量乘数
     */

    [Header("敌人静态波次")]
    public List<WaveGroup> waveGroups = new List<WaveGroup>();

    [System.Serializable]
    public class WaveGroup
    {
        public string waveName = "Wave";
        [Min(0)] public float preDelay;
        //[Min(0)] public float postDelay;
        public List<EnemySpawnUnit> units = new List<EnemySpawnUnit>();
    }

    [System.Serializable]
    public class EnemySpawnUnit
    {
        public EnemyConfig enemy;
        [Min(1)] public int count = 3;
        [Min(0)] public float interval = 0.5f;
        public SpawnPosition positionType;
    }

    public enum SpawnPosition { Random, FixedPoints, Perimeter }
}