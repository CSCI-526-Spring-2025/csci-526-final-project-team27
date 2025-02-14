using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region 数据结构定义

/// <summary>
/// 描述单个敌人生成信息
/// </summary>
[Serializable]
public class EnemySpawnInfo
{
    public string enemyType;  // 敌人类型标识，需要与预制体映射中的 key 对应
    public int count;         // 生成该类型敌人的数量
}

/// <summary>
/// 描述单个波次数据
/// </summary>
[Serializable]
public class WaveData
{
    public int waveNumber;
    public EnemySpawnInfo[] enemySpawns;
}

/// <summary>
/// 整体波次数据的容器（用于 JSON 反序列化）
/// </summary>
[Serializable]
public class WaveDataList
{
    public WaveData[] waves;
}

#endregion

/// <summary>
/// 敌人生成器：
/// 1. 如果指定了波次数据 JSON，则直接按 JSON 定义生成；
/// 2. 否则，如果勾选了直接指定波次数据，则使用 Inspector 中指定的波数和数量；
///    如果也未直接指定，则依据难度等级（1～5）自动生成默认波次数据。  
/// 同时，在生成位置时确保不在障碍物上，也不在玩家或队友附近。
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Wave Data")]
    [Tooltip("存放波次数据的 JSON 文件（可选）。未指定时将根据下列设置自动生成波次数据。")]
    public TextAsset waveDataJson;

    [Header("Difficulty Settings")]
    [Range(1, 5)]
    [Tooltip("难度等级，1 表示最低难度，5 表示最高难度")]
    public int difficultyLevel = 3;

    [Header("Direct Wave Settings (Optional)")]
    [Tooltip("是否直接指定波次数据（而非自动生成）。")]
    public bool useDirectWaveSettings = false;
    [Tooltip("直接指定的波次数（大于 0 表示有效）。")]
    public int directWaveCount = 0;
    [Tooltip("直接指定的每种敌人的生成数量（大于 0 表示有效）。")]
    public int directEnemyCount = 0;

    [Header("Spawn Area & Obstacles")]
    [Tooltip("生成区域的左下角坐标（地图有效区域）")]
    public Vector2 spawnAreaMin = new Vector2(-10, -10);
    [Tooltip("生成区域的右上角坐标（地图有效区域）")]
    public Vector2 spawnAreaMax = new Vector2(10, 10);
    [Tooltip("障碍物所在层，用于检测生成位置是否被遮挡")]
    public LayerMask obstacleLayer;
    [Tooltip("生成位置与玩家或队友之间的最小距离（避让距离）")]
    public float avoidDistance = 3f;
    [Tooltip("不允许生成在此数组中对象附近（例如主角和队友）")]
    public Transform[] avoidTransforms;

    [Header("Spawn Timing")]
    [Tooltip("生成敌人之间的最小延时（秒）")]
    public float minSpawnInterval = 0.5f;
    [Tooltip("生成敌人之间的最大延时（秒）")]
    public float maxSpawnInterval = 2f;
    [Tooltip("每波敌人间隔（秒）")]
    public float waveInterval = 3f;

    [Header("Enemy Prefab Mapping")]
    [Tooltip("预制体映射：将 enemyType 字符串映射到对应的敌人预制体。注意：预制体数组的排列顺序决定了敌人的强弱，数组后端的视为强力敌人。")]
    public EnemyPrefabMapping[] enemyPrefabs;

    [Serializable]
    public class EnemyPrefabMapping
    {
        public string enemyType;
        public GameObject prefab;
    }

    // 内部变量
    private Dictionary<string, GameObject> enemyPrefabDict;
    private WaveDataList waveDataList;

    private void Awake()
    {
        // 根据场景中 "Floor" 节点设置生成区域（如果存在）
        Transform floor = transform.Find("Floor");
        if (floor != null)
        {
            spawnAreaMax = floor.position + floor.localScale / 2;
            spawnAreaMin = floor.position - floor.localScale / 2;
        }

        // 构建预制体映射字典
        enemyPrefabDict = new Dictionary<string, GameObject>();
        foreach (var mapping in enemyPrefabs)
        {
            if (!enemyPrefabDict.ContainsKey(mapping.enemyType))
                enemyPrefabDict.Add(mapping.enemyType, mapping.prefab);
        }
    }

    private void Start()
    {
        // 优先使用 JSON 数据，如果未指定，则依据直接指定设置或自动生成
        if (waveDataJson != null)
        {
            waveDataList = JsonUtility.FromJson<WaveDataList>(waveDataJson.text);
            Debug.Log("加载波次数据 JSON 成功。");
        }
        else
        {
            waveDataList = GenerateRandomWaveData();
            Debug.Log("未指定波次数据 JSON，依据设置随机生成波次数据。");
        }
        //日志中打印波次数据
        foreach (WaveData wave in waveDataList.waves)
        {
            string waveInfo = "波次 " + wave.waveNumber + "：";
            foreach (EnemySpawnInfo spawnInfo in wave.enemySpawns)
            {
                waveInfo += " " + spawnInfo.count + " 个 " + spawnInfo.enemyType + ",";
            }
            Debug.Log(waveInfo);
        }
        StartCoroutine(SpawnWaves());
    }

    /// <summary>
    /// 根据难度等级及直接指定设置生成波次数据字典
    /// </summary>
    /// <returns>生成的波次数据</returns>
    private WaveDataList GenerateRandomWaveData()
    {
        WaveDataList dataList = new WaveDataList();
        // 如果直接指定波次数据（波数和每种敌人生成数量均大于 0），则使用该配置
        if (useDirectWaveSettings && directWaveCount > 0 && directEnemyCount > 0)
        {
            dataList.waves = new WaveData[directWaveCount];
            // 允许生成的敌人类型数量：只取 enemyPrefabs 数组中前 difficultyLevel 个（最多不超过总数）
            int allowedCount = Mathf.Min(difficultyLevel, enemyPrefabDict.Count);
            List<string> allowedTypes = new List<string>();
            for (int i = 0; i < enemyPrefabs.Length && allowedTypes.Count < allowedCount; i++)
            {
                if (enemyPrefabDict.ContainsKey(enemyPrefabs[i].enemyType))
                    allowedTypes.Add(enemyPrefabs[i].enemyType);
            }
            for (int i = 0; i < directWaveCount; i++)
            {
                WaveData wave = new WaveData();
                wave.waveNumber = i + 1;
                List<EnemySpawnInfo> spawnInfos = new List<EnemySpawnInfo>();
                // 每波中，每个允许的敌人类型都生成固定数量
                foreach (string enemyType in allowedTypes)
                {
                    EnemySpawnInfo info = new EnemySpawnInfo();
                    info.enemyType = enemyType;
                    info.count = directEnemyCount;
                    spawnInfos.Add(info);
                }
                wave.enemySpawns = spawnInfos.ToArray();
                dataList.waves[i] = wave;
            }
        }
        else
        {
            // 默认模式：波次数量等于 difficultyLevel，允许的敌人类型数量也取 difficultyLevel（不超过预制体总数）
            int numWaves = difficultyLevel;
            dataList.waves = new WaveData[numWaves];
            int allowedEnemyTypes = Mathf.Min(difficultyLevel, enemyPrefabDict.Count);
            List<string> allowedTypes = new List<string>();
            for (int i = 0; i < enemyPrefabs.Length && allowedTypes.Count < allowedEnemyTypes; i++)
            {
                if (enemyPrefabDict.ContainsKey(enemyPrefabs[i].enemyType))
                    allowedTypes.Add(enemyPrefabs[i].enemyType);
            }
            for (int i = 0; i < numWaves; i++)
            {
                WaveData wave = new WaveData();
                wave.waveNumber = i + 1;
                List<EnemySpawnInfo> spawnInfos = new List<EnemySpawnInfo>();
                // 对于每个允许的敌人类型，80% 的概率出现在该波次，数量随机（1 到 1+difficultyLevel*2）
                foreach (string enemyType in allowedTypes)
                {
                    if (UnityEngine.Random.value < 0.8f)
                    {
                        EnemySpawnInfo info = new EnemySpawnInfo();
                        info.enemyType = enemyType;
                        info.count = UnityEngine.Random.Range(1, 1 + difficultyLevel * 2 + 1);
                        spawnInfos.Add(info);
                    }
                }
                // 如果该波次未选择任何敌人，则至少随机选择一种出现 1 个
                if (spawnInfos.Count == 0 && allowedTypes.Count > 0)
                {
                    EnemySpawnInfo info = new EnemySpawnInfo();
                    info.enemyType = allowedTypes[UnityEngine.Random.Range(0, allowedTypes.Count)];
                    info.count = 1;
                    spawnInfos.Add(info);
                }
                wave.enemySpawns = spawnInfos.ToArray();
                dataList.waves[i] = wave;
            }
        }
        return dataList;
    }

    /// <summary>
    /// 协程：按波次顺序生成敌人单位
    /// </summary>
    private IEnumerator SpawnWaves()
    {
        //等待一段时间再开始生成敌人
        yield return new WaitForSeconds(waveInterval);
        foreach (WaveData wave in waveDataList.waves)
        {
            Debug.Log("开始生成波次：" + wave.waveNumber);
            foreach (EnemySpawnInfo spawnInfo in wave.enemySpawns)
            {
                // 查找对应预制体
                if (enemyPrefabDict.TryGetValue(spawnInfo.enemyType, out GameObject enemyPrefab))
                {
                    for (int i = 0; i < spawnInfo.count; i++)
                    {
                        // 获取一个有效的生成位置
                        Vector2 spawnPos = GetValidSpawnPosition();
                        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

                        // 每个敌人生成之间延时
                        float delay = UnityEngine.Random.Range(minSpawnInterval, maxSpawnInterval);
                        yield return new WaitForSeconds(delay);
                    }
                }
                else
                {
                    Debug.LogWarning("未找到类型为 " + spawnInfo.enemyType + " 的敌人预制体映射！");
                }
            }
            // 波次之间等待固定时间
            yield return new WaitForSeconds(waveInterval);
        }
    }

    /// <summary>
    /// 尝试在 spawnArea 范围内生成一个有效位置：该位置不能被障碍物占用，
    /// 且与 avoidTransforms 中的对象保持至少 avoidDistance 距离。
    /// </summary>
    /// <returns>有效生成位置</returns>
    private Vector2 GetValidSpawnPosition()
    {
        Vector2 candidate = Vector2.zero;
        bool valid = false;
        int attempts = 0;
        int maxAttempts = 20;

        while (!valid && attempts < maxAttempts)
        {
            attempts++;
            float x = UnityEngine.Random.Range(spawnAreaMin.x, spawnAreaMax.x);
            float y = UnityEngine.Random.Range(spawnAreaMin.y, spawnAreaMax.y);
            candidate = new Vector2(x, y);

            // 检查候选位置是否被障碍物占用
            Collider2D hit = Physics2D.OverlapCircle(candidate, 0.1f, obstacleLayer);
            if (hit != null)
                continue;

            // 检查候选位置与 avoidTransforms 中对象之间的距离
            bool tooClose = false;
            foreach (Transform t in avoidTransforms)
            {
                if (Vector2.Distance(candidate, t.position) < avoidDistance)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose)
                continue;

            valid = true;
        }

        if (!valid)
            Debug.LogWarning("经过 " + attempts + " 次尝试后仍未找到完全有效的生成位置，使用最后候选位置。");

        return candidate;
    }
}


//{
//    "waves": [
//        {
//        "waveNumber": 1,
//            "enemySpawns": [
//                { "enemyType": "Slime", "count": 5 },
//                { "enemyType": "Goblin", "count": 3 }
//            ]
//        },
//        {
//        "waveNumber": 2,
//            "enemySpawns": [
//                { "enemyType": "Slime", "count": 7 },
//                { "enemyType": "Goblin", "count": 5 }
//            ]
//        }
//    ]
//}
