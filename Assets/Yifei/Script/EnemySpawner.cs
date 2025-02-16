using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region 数据解析方法

/// <summary>
/// 敌人生成信息
/// </summary>
[Serializable]
public class EnemySpawnInfo
{
    public string enemyType;  // 敌人类型字符串，需要与预设中的 key 对应
    public int count;         // 生成该类型敌人的数量
}

/// <summary>
/// 敌人生成波次数据
/// </summary>
[Serializable]
public class WaveData
{
    public int waveNumber;
    public EnemySpawnInfo[] enemySpawns;
}

/// <summary>
/// 关卡波次数据的解析器（用于 JSON 文件加载）
/// </summary>
[Serializable]
public class WaveDataList
{
    public WaveData[] waves;
}

#endregion

/// <summary>
/// 敌人生成器：
/// 1. 如果配置了波次数据 JSON，则自动调用 JSON 解析生成
/// 2. 否则，如果手动配置了难度对应的波次数据，则使用 Inspector 中配置的波次和数量
///    如果未配置，则根据难度级别（1~5）随机生成敌人波次数据
/// 注意：在生成位置时不考虑障碍物和敌人之间的重叠！
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Wave Data")]
    [Tooltip("加载波次数据的 JSON 文件（可选），如果未配置则根据难度自动生成敌人波次数据")]
    public TextAsset waveDataJson;

    [Header("Difficulty Settings")]
    [Range(1, 5)]
    [Tooltip("难度级别，1 表示最简单，5 表示最困难")]
    public int difficultyLevel = 3;

    [Header("Direct Wave Settings (Optional)")]
    [Tooltip("是否手动配置波次数据（即直接生成）")]
    public bool useDirectWaveSettings = false;
    [Tooltip("配置的波次数（大于 0 表示有效）")]
    public int directWaveCount = 0;
    [Tooltip("配置的每波敌人数量（大于 0 表示有效）")]
    public int directEnemyCount = 0;

    [Header("Spawn Area & Obstacles")]
    [Tooltip("敌人生成区域的左下角坐标（用于定位场景）")]
    public Vector2 spawnAreaMin = new Vector2(-10, -10);
    [Tooltip("敌人生成区域的右上角坐标（用于定位场景）")]
    public Vector2 spawnAreaMax = new Vector2(10, 10);
    [Tooltip("场景中用于标识障碍物的图层（生成时避让障碍物）")]
    public LayerMask obstacleLayer;
    [Tooltip("生成位置时与障碍物或其他指定对象的最小避让距离")]
    public float avoidDistance = 3f;
    [Tooltip("指定在生成位置时需要避让的其他对象的 Transform（不用于检测敌人自身重叠）")]
    public Transform[] avoidTransforms;

    [Header("Spawn Timing")]
    [Tooltip("生成敌人之间的最小间隔（秒）")]
    public float minSpawnInterval = 0.5f;
    [Tooltip("生成敌人之间的最大间隔（秒）")]
    public float maxSpawnInterval = 2f;
    [Tooltip("两波敌人生成之间的间隔时间（秒）")]
    public float waveInterval = 3f;

    [Header("Enemy Prefab Mapping")]
    [Tooltip("预设映射：将 enemyType 字符串映射到对应的敌人预制体。举例：预制体的名称用于在场景中生成敌人")]
    public EnemyPrefabMapping[] enemyPrefabs;

    // 敌人全部死亡时的通知事件
    public event Action<bool> RoomClearEvent;
    // 当所有敌人死亡时，可调用 RoomClearEvent?.Invoke(true);

    // 当前存活的敌人数量
    private int aliveEnemyCount = 0;

    // 所有波次是否已经全部生成完成
    private bool spawningCompleted = false;

    [Serializable]
    public class EnemyPrefabMapping
    {
        public string enemyType;
        public GameObject prefab;
    }

    // 私有变量
    private Dictionary<string, GameObject> enemyPrefabDict;
    private WaveDataList waveDataList;

    private bool hasSpawned = false;

    public void StartSpawn()
    {
        if (hasSpawned)
            return;
        else
        {
            hasSpawned = true;                     
            spawningCompleted = false;             
            aliveEnemyCount = 0;                   
            StartCoroutine(SpawnWaves());          
        }
    }

    public void EnemyDie()
    {
        aliveEnemyCount--;   
                             
        if (spawningCompleted && aliveEnemyCount <= 0)
        {
            RoomClearEvent?.Invoke(true);
        }
    }

    private void Awake()
    {
        // 根据场景中名为 "Floor" 的子物体设置生成区域（如果存在）
        Transform floor = transform.Find("Floor");
        if (floor != null)
        {
            spawnAreaMax = floor.position + floor.localScale / 2;
            spawnAreaMin = floor.position - floor.localScale / 2;
        }

        // 构建预设映射字典
        enemyPrefabDict = new Dictionary<string, GameObject>();
        foreach (var mapping in enemyPrefabs)
        {
            if (!enemyPrefabDict.ContainsKey(mapping.enemyType))
                enemyPrefabDict.Add(mapping.enemyType, mapping.prefab);
        }
    }

    private void Start()
    {
        // 优先使用 JSON 数据，如果未配置，则根据难度自动生成敌人波次数据
        if (waveDataJson != null)
        {
            waveDataList = JsonUtility.FromJson<WaveDataList>(waveDataJson.text);
            Debug.Log("成功加载波次数据 JSON！");
        }
        else
        {
            waveDataList = GenerateRandomWaveData();
            Debug.Log("未配置波次数据 JSON，已根据难度自动生成敌人波次数据！");
        }
        // 在控制台打印波次数据
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
    /// 根据难度级别生成敌人波次数据
    /// </summary>
    /// <returns>生成的波次数据</returns>
    private WaveDataList GenerateRandomWaveData()
    {
        WaveDataList dataList = new WaveDataList();
        // 如果配置了手动波次设置（直接生成）且波次数和敌人数量大于 0
        if (useDirectWaveSettings && directWaveCount > 0 && directEnemyCount > 0)
        {
            dataList.waves = new WaveData[directWaveCount];
            // 计算允许生成的敌人类型数量：取 difficultyLevel 与预设映射数量中的较小值
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
                // 针对每一种允许生成的敌人类型，生成对应的敌人数量
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
            // 普通模式：波次数为难度级别，允许生成的敌人类型最多为 difficultyLevel 个
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
                // 对于每一种允许生成的敌人类型，以 80% 概率生成该类型的敌人，数量随机生成，范围为 [1, 1+difficultyLevel*2]
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
                // 如果一波中未生成任何敌人，则至少生成 1 个随机敌人
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
    /// 协程：逐波生成敌人
    /// </summary>
    private IEnumerator SpawnWaves()
    {
        yield return new WaitForSeconds(waveInterval);
        foreach (WaveData wave in waveDataList.waves)
        {
            Debug.Log("开始生成波次：" + wave.waveNumber);
            foreach (EnemySpawnInfo spawnInfo in wave.enemySpawns)
            {
                if (enemyPrefabDict.TryGetValue(spawnInfo.enemyType, out GameObject enemyPrefab))
                {
                    for (int i = 0; i < spawnInfo.count; i++)
                    {
                        Vector2 spawnPos = GetValidSpawnPosition();
                        GameObject enemyInstance =  Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                        EnemyHealth enemyHealth = enemyInstance.GetComponent<EnemyHealth>();
                        if (enemyHealth != null)
                        {
                            enemyHealth.SetSpawnner(this);
                        }
                        else
                        {
                            Debug.LogWarning("未在生成的敌人中找到 EnemyHealth 组件！");
                        }
                        aliveEnemyCount++;   
                        float delay = UnityEngine.Random.Range(minSpawnInterval, maxSpawnInterval);
                        yield return new WaitForSeconds(delay);
                    }
                }
                else
                {
                    Debug.LogWarning("未找到类型为 " + spawnInfo.enemyType + " 的敌人预制体！");
                }
            }
            yield return new WaitForSeconds(waveInterval);
        }
        spawningCompleted = true;   
        // 如果此时没有存活的敌人，则触发房间清空事件
        if (aliveEnemyCount <= 0)
        {
            RoomClearEvent?.Invoke(true); 
        }
    }


    /// <summary>
    /// 获取一个有效的生成位置：确保该位置不在障碍物内，并且与指定的对象保持足够距离
    /// </summary>
    /// <returns>生成位置</returns>
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

            // 检测该位置是否碰到障碍物
            Collider2D hit = Physics2D.OverlapCircle(candidate, 0.1f, obstacleLayer);
            if (hit != null)
                continue;

            // 检测该位置是否与指定对象距离过近
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
            Debug.LogWarning("经过 " + attempts + " 次尝试后仍未找到合适的生成位置，使用最后的候选位置！");

        return candidate;
    }
}


//{
//    "waves": [
//        {
//            "waveNumber": 1,
//            "enemySpawns": [
//                { "enemyType": "Slime", "count": 5 },
//                { "enemyType": "Goblin", "count": 3 }
//            ]
//        },
//        {
//            "waveNumber": 2,
//            "enemySpawns": [
//                { "enemyType": "Slime", "count": 7 },
//                { "enemyType": "Goblin", "count": 5 }
//            ]
//        }
//    ]
//}
