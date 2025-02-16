using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Êý¾Ý½á¹¹¶¨Òå

/// <summary>
/// ÃèÊöµ¥¸öµÐÈËÉú³ÉÐÅÏ¢
/// </summary>
[Serializable]
public class EnemySpawnInfo
{
    public string enemyType;  // µÐÈËÀàÐÍ±êÊ¶£¬ÐèÒªÓëÔ¤ÖÆÌåÓ³ÉäÖÐµÄ key ¶ÔÓ¦
    public int count;         // Éú³É¸ÃÀàÐÍµÐÈËµÄÊýÁ¿
}

/// <summary>
/// ÃèÊöµ¥¸ö²¨´ÎÊý¾Ý
/// </summary>
[Serializable]
public class WaveData
{
    public int waveNumber;
    public EnemySpawnInfo[] enemySpawns;
}

/// <summary>
/// ÕûÌå²¨´ÎÊý¾ÝµÄÈÝÆ÷£¨ÓÃÓÚ JSON ·´ÐòÁÐ»¯£©
/// </summary>
[Serializable]
public class WaveDataList
{
    public WaveData[] waves;
}

#endregion

/// <summary>
/// µÐÈËÉú³ÉÆ÷£º
/// 1. Èç¹ûÖ¸¶¨ÁË²¨´ÎÊý¾Ý JSON£¬ÔòÖ±½Ó°´ JSON ¶¨ÒåÉú³É£»
/// 2. ·ñÔò£¬Èç¹û¹´Ñ¡ÁËÖ±½ÓÖ¸¶¨²¨´ÎÊý¾Ý£¬ÔòÊ¹ÓÃ Inspector ÖÐÖ¸¶¨µÄ²¨ÊýºÍÊýÁ¿£»
///    Èç¹ûÒ²Î´Ö±½ÓÖ¸¶¨£¬ÔòÒÀ¾ÝÄÑ¶ÈµÈ¼¶£¨1¡«5£©×Ô¶¯Éú³ÉÄ¬ÈÏ²¨´ÎÊý¾Ý¡£  
/// Í¬Ê±£¬ÔÚÉú³ÉÎ»ÖÃÊ±È·±£²»ÔÚÕÏ°­ÎïÉÏ£¬Ò²²»ÔÚÍæ¼Ò»ò¶ÓÓÑ¸½½ü¡£
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Wave Data")]
    [Tooltip("´æ·Å²¨´ÎÊý¾ÝµÄ JSON ÎÄ¼þ£¨¿ÉÑ¡£©¡£Î´Ö¸¶¨Ê±½«¸ù¾ÝÏÂÁÐÉèÖÃ×Ô¶¯Éú³É²¨´ÎÊý¾Ý¡£")]
    public TextAsset waveDataJson;

    [Header("Difficulty Settings")]
    [Range(1, 5)]
    [Tooltip("ÄÑ¶ÈµÈ¼¶£¬1 ±íÊ¾×îµÍÄÑ¶È£¬5 ±íÊ¾×î¸ßÄÑ¶È")]
    public int difficultyLevel = 3;

    [Header("Direct Wave Settings (Optional)")]
    [Tooltip("ÊÇ·ñÖ±½ÓÖ¸¶¨²¨´ÎÊý¾Ý£¨¶ø·Ç×Ô¶¯Éú³É£©¡£")]
    public bool useDirectWaveSettings = false;
    [Tooltip("Ö±½ÓÖ¸¶¨µÄ²¨´ÎÊý£¨´óÓÚ 0 ±íÊ¾ÓÐÐ§£©¡£")]
    public int directWaveCount = 0;
    [Tooltip("Ö±½ÓÖ¸¶¨µÄÃ¿ÖÖµÐÈËµÄÉú³ÉÊýÁ¿£¨´óÓÚ 0 ±íÊ¾ÓÐÐ§£©¡£")]
    public int directEnemyCount = 0;

    [Header("Spawn Area & Obstacles")]
    [Tooltip("Éú³ÉÇøÓòµÄ×óÏÂ½Ç×ø±ê£¨µØÍ¼ÓÐÐ§ÇøÓò£©")]
    public Vector2 spawnAreaMin = new Vector2(-10, -10);
    [Tooltip("Éú³ÉÇøÓòµÄÓÒÉÏ½Ç×ø±ê£¨µØÍ¼ÓÐÐ§ÇøÓò£©")]
    public Vector2 spawnAreaMax = new Vector2(10, 10);
    [Tooltip("ÕÏ°­ÎïËùÔÚ²ã£¬ÓÃÓÚ¼ì²âÉú³ÉÎ»ÖÃÊÇ·ñ±»ÕÚµ²")]
    public LayerMask obstacleLayer;
    [Tooltip("Éú³ÉÎ»ÖÃÓëÍæ¼Ò»ò¶ÓÓÑÖ®¼äµÄ×îÐ¡¾àÀë£¨±ÜÈÃ¾àÀë£©")]
    public float avoidDistance = 3f;
    [Tooltip("²»ÔÊÐíÉú³ÉÔÚ´ËÊý×éÖÐ¶ÔÏó¸½½ü£¨ÀýÈçÖ÷½ÇºÍ¶ÓÓÑ£©")]
    public Transform[] avoidTransforms;

    [Header("Spawn Timing")]
    [Tooltip("Éú³ÉµÐÈËÖ®¼äµÄ×îÐ¡ÑÓÊ±£¨Ãë£©")]
    public float minSpawnInterval = 0.5f;
    [Tooltip("Éú³ÉµÐÈËÖ®¼äµÄ×î´óÑÓÊ±£¨Ãë£©")]
    public float maxSpawnInterval = 2f;
    [Tooltip("Ã¿²¨µÐÈË¼ä¸ô£¨Ãë£©")]
    public float waveInterval = 3f;

    [Header("Enemy Prefab Mapping")]
    [Tooltip("Ô¤ÖÆÌåÓ³Éä£º½« enemyType ×Ö·û´®Ó³Éäµ½¶ÔÓ¦µÄµÐÈËÔ¤ÖÆÌå¡£×¢Òâ£ºÔ¤ÖÆÌåÊý×éµÄÅÅÁÐË³Ðò¾ö¶¨ÁËµÐÈËµÄÇ¿Èõ£¬Êý×éºó¶ËµÄÊÓÎªÇ¿Á¦µÐÈË¡£")]
    public EnemyPrefabMapping[] enemyPrefabs;

    // 敌人全部死亡的通知
    public event Action<bool> RoomClearEvent;
    // 当所有敌人死亡时，RoomClearEvent?.Invoke(true);

    [Serializable]
    public class EnemyPrefabMapping
    {
        public string enemyType;
        public GameObject prefab;
    }

    // ÄÚ²¿±äÁ¿
    private Dictionary<string, GameObject> enemyPrefabDict;
    private WaveDataList waveDataList;

    private bool hasSpawned = false;

    public void StartSpawn()
    {
        if (hasSpawned)
            return;
        else
        {
            //do something
        }
    }

    public void EnemyDie()
    {
        //do something
    }

    private void Awake()
    {
        // ¸ù¾Ý³¡¾°ÖÐ "Floor" ½ÚµãÉèÖÃÉú³ÉÇøÓò£¨Èç¹û´æÔÚ£©
        Transform floor = transform.Find("Floor");
        if (floor != null)
        {
            spawnAreaMax = floor.position + floor.localScale / 2;
            spawnAreaMin = floor.position - floor.localScale / 2;
        }

        // ¹¹½¨Ô¤ÖÆÌåÓ³Éä×Öµä
        enemyPrefabDict = new Dictionary<string, GameObject>();
        foreach (var mapping in enemyPrefabs)
        {
            if (!enemyPrefabDict.ContainsKey(mapping.enemyType))
                enemyPrefabDict.Add(mapping.enemyType, mapping.prefab);
        }
    }

    private void Start()
    {
        // ÓÅÏÈÊ¹ÓÃ JSON Êý¾Ý£¬Èç¹ûÎ´Ö¸¶¨£¬ÔòÒÀ¾ÝÖ±½ÓÖ¸¶¨ÉèÖÃ»ò×Ô¶¯Éú³É
        if (waveDataJson != null)
        {
            waveDataList = JsonUtility.FromJson<WaveDataList>(waveDataJson.text);
            Debug.Log("¼ÓÔØ²¨´ÎÊý¾Ý JSON ³É¹¦¡£");
        }
        else
        {
            waveDataList = GenerateRandomWaveData();
            Debug.Log("Î´Ö¸¶¨²¨´ÎÊý¾Ý JSON£¬ÒÀ¾ÝÉèÖÃËæ»úÉú³É²¨´ÎÊý¾Ý¡£");
        }
        //ÈÕÖ¾ÖÐ´òÓ¡²¨´ÎÊý¾Ý
        foreach (WaveData wave in waveDataList.waves)
        {
            string waveInfo = "²¨´Î " + wave.waveNumber + "£º";
            foreach (EnemySpawnInfo spawnInfo in wave.enemySpawns)
            {
                waveInfo += " " + spawnInfo.count + " ¸ö " + spawnInfo.enemyType + ",";
            }
            Debug.Log(waveInfo);
        }
        StartCoroutine(SpawnWaves());
    }

    /// <summary>
    /// ¸ù¾ÝÄÑ¶ÈµÈ¼¶¼°Ö±½ÓÖ¸¶¨ÉèÖÃÉú³É²¨´ÎÊý¾Ý×Öµä
    /// </summary>
    /// <returns>Éú³ÉµÄ²¨´ÎÊý¾Ý</returns>
    private WaveDataList GenerateRandomWaveData()
    {
        WaveDataList dataList = new WaveDataList();
        // Èç¹ûÖ±½ÓÖ¸¶¨²¨´ÎÊý¾Ý£¨²¨ÊýºÍÃ¿ÖÖµÐÈËÉú³ÉÊýÁ¿¾ù´óÓÚ 0£©£¬ÔòÊ¹ÓÃ¸ÃÅäÖÃ
        if (useDirectWaveSettings && directWaveCount > 0 && directEnemyCount > 0)
        {
            dataList.waves = new WaveData[directWaveCount];
            // ÔÊÐíÉú³ÉµÄµÐÈËÀàÐÍÊýÁ¿£ºÖ»È¡ enemyPrefabs Êý×éÖÐÇ° difficultyLevel ¸ö£¨×î¶à²»³¬¹ý×ÜÊý£©
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
                // Ã¿²¨ÖÐ£¬Ã¿¸öÔÊÐíµÄµÐÈËÀàÐÍ¶¼Éú³É¹Ì¶¨ÊýÁ¿
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
            // Ä¬ÈÏÄ£Ê½£º²¨´ÎÊýÁ¿µÈÓÚ difficultyLevel£¬ÔÊÐíµÄµÐÈËÀàÐÍÊýÁ¿Ò²È¡ difficultyLevel£¨²»³¬¹ýÔ¤ÖÆÌå×ÜÊý£©
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
                // ¶ÔÓÚÃ¿¸öÔÊÐíµÄµÐÈËÀàÐÍ£¬80% µÄ¸ÅÂÊ³öÏÖÔÚ¸Ã²¨´Î£¬ÊýÁ¿Ëæ»ú£¨1 µ½ 1+difficultyLevel*2£©
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
                // Èç¹û¸Ã²¨´ÎÎ´Ñ¡ÔñÈÎºÎµÐÈË£¬ÔòÖÁÉÙËæ»úÑ¡ÔñÒ»ÖÖ³öÏÖ 1 ¸ö
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
    /// Ð­³Ì£º°´²¨´ÎË³ÐòÉú³ÉµÐÈËµ¥Î»
    /// </summary>
    private IEnumerator SpawnWaves()
    {
        //µÈ´ýÒ»¶ÎÊ±¼äÔÙ¿ªÊ¼Éú³ÉµÐÈË
        yield return new WaitForSeconds(waveInterval);
        foreach (WaveData wave in waveDataList.waves)
        {
            Debug.Log("¿ªÊ¼Éú³É²¨´Î£º" + wave.waveNumber);
            foreach (EnemySpawnInfo spawnInfo in wave.enemySpawns)
            {
                // ²éÕÒ¶ÔÓ¦Ô¤ÖÆÌå
                if (enemyPrefabDict.TryGetValue(spawnInfo.enemyType, out GameObject enemyPrefab))
                {
                    for (int i = 0; i < spawnInfo.count; i++)
                    {
                        // »ñÈ¡Ò»¸öÓÐÐ§µÄÉú³ÉÎ»ÖÃ
                        Vector2 spawnPos = GetValidSpawnPosition();
                        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

                        // Ã¿¸öµÐÈËÉú³ÉÖ®¼äÑÓÊ±
                        float delay = UnityEngine.Random.Range(minSpawnInterval, maxSpawnInterval);
                        yield return new WaitForSeconds(delay);
                    }
                }
                else
                {
                    Debug.LogWarning("Î´ÕÒµ½ÀàÐÍÎª " + spawnInfo.enemyType + " µÄµÐÈËÔ¤ÖÆÌåÓ³Éä£¡");
                }
            }
            // ²¨´ÎÖ®¼äµÈ´ý¹Ì¶¨Ê±¼ä
            yield return new WaitForSeconds(waveInterval);
        }
    }

    /// <summary>
    /// ³¢ÊÔÔÚ spawnArea ·¶Î§ÄÚÉú³ÉÒ»¸öÓÐÐ§Î»ÖÃ£º¸ÃÎ»ÖÃ²»ÄÜ±»ÕÏ°­ÎïÕ¼ÓÃ£¬
    /// ÇÒÓë avoidTransforms ÖÐµÄ¶ÔÏó±£³ÖÖÁÉÙ avoidDistance ¾àÀë¡£
    /// </summary>
    /// <returns>ÓÐÐ§Éú³ÉÎ»ÖÃ</returns>
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

            // ¼ì²éºòÑ¡Î»ÖÃÊÇ·ñ±»ÕÏ°­ÎïÕ¼ÓÃ
            Collider2D hit = Physics2D.OverlapCircle(candidate, 0.1f, obstacleLayer);
            if (hit != null)
                continue;

            // ¼ì²éºòÑ¡Î»ÖÃÓë avoidTransforms ÖÐ¶ÔÏóÖ®¼äµÄ¾àÀë
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
            Debug.LogWarning("¾­¹ý " + attempts + " ´Î³¢ÊÔºóÈÔÎ´ÕÒµ½ÍêÈ«ÓÐÐ§µÄÉú³ÉÎ»ÖÃ£¬Ê¹ÓÃ×îºóºòÑ¡Î»ÖÃ¡£");

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
