using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSpawner : MonoBehaviour
{
    #region 房间配置
    public enum RoomType { Normal, Elite, Boss }
    public enum SpawnMode { Dynamic, Static }
    public enum SpawnPosition { Random, FixedPoints, Perimeter }

    [Header("房间配置")]
    [Tooltip("房间预制体")]
    public GameObject roomPrefab;
    [Tooltip("房间类型")]
    public RoomType roomType;
    [Tooltip("生成模式")]
    public SpawnMode spawnMode;

    [Header("敌人动态生成")]
    [Tooltip("动态生成时的生成数量范围最小值")]
    public int dynamicSpawnRangeMin = 1;
    [Tooltip("动态生成时的生成数量范围最大值")]
    public int dynamicSpawnRangeMax = 10;
    [Tooltip("随难度变化的生成数量曲线")] // X轴: 标准化难度(0-1) Y轴: 数量乘数
    public AnimationCurve difficultyCurve = AnimationCurve.Linear(0, 1, 1, 1);
    [Tooltip("生成位置（仅随机或边缘）")]
    public SpawnPosition dynamicSpawnPosition = SpawnPosition.Random;

    [Header("敌人静态波次")]
    public List<WaveGroup> waveGroups = new List<WaveGroup>();

    [Serializable]
    public class WaveGroup
    {
        [Tooltip("波次名称（可选）")]
        public string waveName = "Wave";
        [Tooltip("波次开始前的延迟")]
        [Min(0)] public float preDelay;
        [Tooltip("敌人生成单元")]
        public List<EnemySpawnUnit> units = new List<EnemySpawnUnit>();
    }

    [Serializable]
    public class EnemySpawnUnit
    {
        [Tooltip("敌人预制体（需挂载 EnemyBase 组件）")]
        public GameObject enemyPrefab;
        [Tooltip("生成数量")]
        [Min(1)] public int count = 3;
        [Tooltip("生成间隔（秒）")]
        [Min(0)] public float interval = 0.5f;
        [Tooltip("生成位置模式")]
        public SpawnPosition positionType;
        [Tooltip("生成位置列表（相对房间中心）")]
        public List<Vector2> spawnPositions = new List<Vector2>();
    }
    #endregion

    #region 运行时参数
    [Header("运行时参数")]
    [SerializeField] private GlobalEnemyLibrary enemyLibrary;
    private Vector2 roomSize = Vector2.one * 10f;
    private List<GameObject> validEnemies = new List<GameObject>();
    [SerializeField] private float currentDifficulty;
    [SerializeField] private float spawnIntervalMin = 1f;
    [SerializeField] private float spawnIntervalMax = 3f;

    // 敌人全部死亡时的通知事件
    public event Action<bool> RoomClearEvent;
    // 敌人数量监控
    private List<GameObject> activeEnemies = new List<GameObject>();
    private int totalEnemiesSpawned;
    private int enemiesRemaining;
    private int roomCount; // 可用于计算难度，从房间管理器获取？

    // 标识是否所有敌人都已生成完成
    private bool spawningFinished = false;
    #endregion

    private void Start()
    {
        InitializeRoom();
        Debug.Log("Room " + name + " initialized");

        // 获取房间大小（以房间中名为 Floor 的子物体为准）
        Transform floor = transform.Find("Floor");
        if (floor != null)
            roomSize = new Vector2(floor.localScale.x, floor.localScale.z);
        else
            Debug.LogWarning("Room " + name + " has no floor!");

        CalculateDifficulty();
        Debug.Log("Current difficulty: " + currentDifficulty);
        FilterValidEnemies();
        Debug.Log("Valid enemies: " + validEnemies.Count);

        switch (spawnMode)
        {
            case SpawnMode.Dynamic:
                Debug.Log("Dynamic spawning");
                StartCoroutine(SpawnDynamicWave());
                break;
            case SpawnMode.Static:
                Debug.Log("Static spawning");
                StartCoroutine(SpawnStaticWaves());
                break;
        }
    }

    #region 房间管理
    private void InitializeRoom()
    {
        activeEnemies.Clear();
        totalEnemiesSpawned = 0;
        enemiesRemaining = 0;
        spawningFinished = false;
    }

    public void RegisterEnemy(GameObject enemy)
    {
        enemy.transform.SetParent(transform);
        enemy.tag = "Enemy";
        activeEnemies.Add(enemy);
        totalEnemiesSpawned++;
        enemiesRemaining++;
        Debug.Log("Enemy " + enemy.name + " spawned");
        Debug.Log("Enemies Remaining: " + enemiesRemaining);

        var health = enemy.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.OnDeath.AddListener(HandleEnemyDeath);
            health.OnIncrease.AddListener(HandleEnemyIncrease);
        }
    }

    private void HandleEnemyDeath(GameObject enemy)
    {
        enemiesRemaining--;
        Debug.Log("Enemies Remaining: " + enemiesRemaining);
        CheckRoomClear();
        activeEnemies.Remove(enemy);
    }

    private void HandleEnemyIncrease(GameObject original, GameObject[] newEnemies)
    {
        foreach (var e in newEnemies)
        {
            RegisterEnemy(e);
        }
    }

    // 修改后的清空判断，只有当所有敌人生成完毕且敌人数量归零时，才算房间清空
    private void CheckRoomClear()
    {
        if (spawningFinished && enemiesRemaining <= 0)
        {
            RoomClearEvent?.Invoke(true);
            Debug.Log("Room " + name + " clear");
        }
    }
    #endregion

    #region 动态生成
    private IEnumerator SpawnDynamicWave()
    {
        int spawnCount = Mathf.RoundToInt(
            UnityEngine.Random.Range(dynamicSpawnRangeMin, Mathf.Max(dynamicSpawnRangeMax, dynamicSpawnRangeMin))
            * difficultyCurve.Evaluate(currentDifficulty)
        );

        for (int i = 0; i < spawnCount; i++)
        {
            GameObject enemyPrefab = SelectEnemyByWeight();
            SpawnEnemy(enemyPrefab, GetDynamicSpawnPosition());
            yield return new WaitForSeconds(UnityEngine.Random.Range(spawnIntervalMin, spawnIntervalMax));
        }
        // 动态生成完成后设置标志，并检查房间是否清空
        spawningFinished = true;
        CheckRoomClear();
    }

    private Vector2 GetDynamicSpawnPosition()
    {
        // 如果房间类型为 Boss，则采用 Boss 专用生成逻辑
        if (roomType == RoomType.Boss)
        {
            return GetBossSpawnPosition();
        }

        // 根据动态生成的生成位置模式来选择生成逻辑
        return dynamicSpawnPosition switch
        {
            SpawnPosition.Random => GetRandomPosition(),
            SpawnPosition.Perimeter => GetPerimeterPosition(),
            // FixedPoints 不适用于动态生成，默认采用随机位置
            SpawnPosition.FixedPoints => GetRandomPosition(),
            _ => GetRandomPosition()
        };
    }

    // 在房间边缘生成一个位置
    private Vector2 GetPerimeterPosition()
    {
        Vector2 roomCenter = transform.position;
        float angle = UnityEngine.Random.Range(0, 360f);
        Vector2 dir = new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        );

        return roomCenter + dir * (roomSize / 2f) * 0.9f;
    }

    private Vector2 GetBossSpawnPosition()
    {
        return transform.position;
    }
    #endregion

    #region 静态波次生成
    private IEnumerator SpawnStaticWaves()
    {
        foreach (var wave in waveGroups)
        {
            Debug.Log("Starting wave: " + wave.waveName + " in " + name);
            yield return new WaitForSeconds(wave.preDelay);

            List<Coroutine> spawnRoutines = new List<Coroutine>();
            foreach (var unit in wave.units)
            {
                Debug.Log("Spawning " + unit.count + " " + unit.enemyPrefab.name);
                spawnRoutines.Add(StartCoroutine(SpawnUnit(unit)));
            }

            foreach (var routine in spawnRoutines)
            {
                yield return routine;
            }
        }
        // 静态波次生成全部完成后设置标志，并检查房间是否清空
        spawningFinished = true;
        CheckRoomClear();
    }

    private IEnumerator SpawnUnit(EnemySpawnUnit unit)
    {
        for (int i = 0; i < unit.count; i++)
        {
            SpawnEnemy(unit.enemyPrefab, GetStaticSpawnPosition(unit));
            yield return new WaitForSeconds(unit.interval);
        }
    }

    private Vector2 GetStaticSpawnPosition(EnemySpawnUnit unit)
    {
        return unit.positionType switch
        {
            SpawnPosition.FixedPoints => GetConfiguredFixedPosition(unit),
            SpawnPosition.Perimeter => GetPerimeterPosition(),
            _ => GetRandomPosition()
        };
    }

    private Vector2 GetRandomPosition()
    {
        return (Vector2)transform.position + new Vector2(
            UnityEngine.Random.Range(-roomSize.x / 2, roomSize.x / 2),
            UnityEngine.Random.Range(-roomSize.y / 2, roomSize.y / 2)
        );
    }

    private Vector2 GetConfiguredFixedPosition(EnemySpawnUnit unit)
    {
        if (unit.spawnPositions.Count == 0)
        {
            // 获取 BaseEnemy 中的 displayName
            var enemyBase = unit.enemyPrefab.GetComponent<BaseEnemy>();
            string enemyName = enemyBase != null ? enemyBase.displayName : "Unknown";
            Debug.LogWarning($"单位 {enemyName} 未配置生成位置，使用随机位置");
            return GetRandomPosition();
        }

        // 随机选择一个预设位置，并转换为世界坐标（考虑房间的变换）
        Vector2 localPos = unit.spawnPositions[UnityEngine.Random.Range(0, unit.spawnPositions.Count)];
        return transform.TransformPoint(localPos);
    }
    #endregion

    #region 通用逻辑
    private void CalculateDifficulty()
    {
        // 这里可以根据房间数量或其它逻辑调整难度，目前固定为 1
        currentDifficulty = 1f;
    }

    private void FilterValidEnemies()
    {
        validEnemies.Clear();
        foreach (var enemyPrefab in enemyLibrary.enemyPrefabs)
        {
            if (IsEnemyValid(enemyPrefab))
                validEnemies.Add(enemyPrefab);
        }
    }

    private bool IsEnemyValid(GameObject enemyPrefab)
    {
        BaseEnemy enemyBase = enemyPrefab.GetComponent<BaseEnemy>();
        if (enemyBase == null)
        {
            Debug.LogWarning($"预制体 {enemyPrefab.name} 没有 EnemyBase 组件");
            return false;
        }

        bool validType = roomType switch
        {
            RoomType.Normal => enemyBase.allowInNormal,
            RoomType.Elite => enemyBase.allowInElite,
            RoomType.Boss => enemyBase.allowInBoss,
            _ => false
        };

        return validType && currentDifficulty >= enemyBase.difficultyThreshold;
    }

    private GameObject SelectEnemyByWeight()
    {
        float totalWeight = 0f;
        foreach (var enemyPrefab in validEnemies)
        {
            BaseEnemy enemyBase = enemyPrefab.GetComponent<BaseEnemy>();
            totalWeight += enemyBase.spawnWeight;
        }

        float randomPoint = UnityEngine.Random.Range(0, totalWeight);
        foreach (var enemyPrefab in validEnemies)
        {
            BaseEnemy enemyBase = enemyPrefab.GetComponent<BaseEnemy>();
            if (randomPoint < enemyBase.spawnWeight)
                return enemyPrefab;
            randomPoint -= enemyBase.spawnWeight;
        }
        return validEnemies[0];
    }

    private void SpawnEnemy(GameObject enemyPrefab, Vector2 position)
    {
        GameObject enemyInstance = Instantiate(enemyPrefab, position, Quaternion.identity);
        RegisterEnemy(enemyInstance);
    }
    #endregion
}
