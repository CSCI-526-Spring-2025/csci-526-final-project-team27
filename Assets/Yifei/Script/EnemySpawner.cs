using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region ���ݽṹ����

/// <summary>
/// ������������������Ϣ
/// </summary>
[Serializable]
public class EnemySpawnInfo
{
    public string enemyType;  // �������ͱ�ʶ����Ҫ��Ԥ����ӳ���е� key ��Ӧ
    public int count;         // ���ɸ����͵��˵�����
}

/// <summary>
/// ����������������
/// </summary>
[Serializable]
public class WaveData
{
    public int waveNumber;
    public EnemySpawnInfo[] enemySpawns;
}

/// <summary>
/// ���岨�����ݵ����������� JSON �����л���
/// </summary>
[Serializable]
public class WaveDataList
{
    public WaveData[] waves;
}

#endregion

/// <summary>
/// ������������
/// 1. ���ָ���˲������� JSON����ֱ�Ӱ� JSON �������ɣ�
/// 2. ���������ѡ��ֱ��ָ���������ݣ���ʹ�� Inspector ��ָ���Ĳ�����������
///    ���Ҳδֱ��ָ�����������Ѷȵȼ���1��5���Զ�����Ĭ�ϲ������ݡ�  
/// ͬʱ��������λ��ʱȷ�������ϰ����ϣ�Ҳ������һ���Ѹ�����
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Wave Data")]
    [Tooltip("��Ų������ݵ� JSON �ļ�����ѡ����δָ��ʱ���������������Զ����ɲ������ݡ�")]
    public TextAsset waveDataJson;

    [Header("Difficulty Settings")]
    [Range(1, 5)]
    [Tooltip("�Ѷȵȼ���1 ��ʾ����Ѷȣ�5 ��ʾ����Ѷ�")]
    public int difficultyLevel = 3;

    [Header("Direct Wave Settings (Optional)")]
    [Tooltip("�Ƿ�ֱ��ָ���������ݣ������Զ����ɣ���")]
    public bool useDirectWaveSettings = false;
    [Tooltip("ֱ��ָ���Ĳ����������� 0 ��ʾ��Ч����")]
    public int directWaveCount = 0;
    [Tooltip("ֱ��ָ����ÿ�ֵ��˵��������������� 0 ��ʾ��Ч����")]
    public int directEnemyCount = 0;

    [Header("Spawn Area & Obstacles")]
    [Tooltip("������������½����꣨��ͼ��Ч����")]
    public Vector2 spawnAreaMin = new Vector2(-10, -10);
    [Tooltip("������������Ͻ����꣨��ͼ��Ч����")]
    public Vector2 spawnAreaMax = new Vector2(10, 10);
    [Tooltip("�ϰ������ڲ㣬���ڼ������λ���Ƿ��ڵ�")]
    public LayerMask obstacleLayer;
    [Tooltip("����λ������һ����֮�����С���루���þ��룩")]
    public float avoidDistance = 3f;
    [Tooltip("�����������ڴ������ж��󸽽����������ǺͶ��ѣ�")]
    public Transform[] avoidTransforms;

    [Header("Spawn Timing")]
    [Tooltip("���ɵ���֮�����С��ʱ���룩")]
    public float minSpawnInterval = 0.5f;
    [Tooltip("���ɵ���֮��������ʱ���룩")]
    public float maxSpawnInterval = 2f;
    [Tooltip("ÿ�����˼�����룩")]
    public float waveInterval = 3f;

    [Header("Enemy Prefab Mapping")]
    [Tooltip("Ԥ����ӳ�䣺�� enemyType �ַ���ӳ�䵽��Ӧ�ĵ���Ԥ���塣ע�⣺Ԥ�������������˳������˵��˵�ǿ���������˵���Ϊǿ�����ˡ�")]
    public EnemyPrefabMapping[] enemyPrefabs;

    [Serializable]
    public class EnemyPrefabMapping
    {
        public string enemyType;
        public GameObject prefab;
    }

    // �ڲ�����
    private Dictionary<string, GameObject> enemyPrefabDict;
    private WaveDataList waveDataList;

    private void Awake()
    {
        // ���ݳ����� "Floor" �ڵ�������������������ڣ�
        Transform floor = transform.Find("Floor");
        if (floor != null)
        {
            spawnAreaMax = floor.position + floor.localScale / 2;
            spawnAreaMin = floor.position - floor.localScale / 2;
        }

        // ����Ԥ����ӳ���ֵ�
        enemyPrefabDict = new Dictionary<string, GameObject>();
        foreach (var mapping in enemyPrefabs)
        {
            if (!enemyPrefabDict.ContainsKey(mapping.enemyType))
                enemyPrefabDict.Add(mapping.enemyType, mapping.prefab);
        }
    }

    private void Start()
    {
        // ����ʹ�� JSON ���ݣ����δָ����������ֱ��ָ�����û��Զ�����
        if (waveDataJson != null)
        {
            waveDataList = JsonUtility.FromJson<WaveDataList>(waveDataJson.text);
            Debug.Log("���ز������� JSON �ɹ���");
        }
        else
        {
            waveDataList = GenerateRandomWaveData();
            Debug.Log("δָ���������� JSON����������������ɲ������ݡ�");
        }
        //��־�д�ӡ��������
        foreach (WaveData wave in waveDataList.waves)
        {
            string waveInfo = "���� " + wave.waveNumber + "��";
            foreach (EnemySpawnInfo spawnInfo in wave.enemySpawns)
            {
                waveInfo += " " + spawnInfo.count + " �� " + spawnInfo.enemyType + ",";
            }
            Debug.Log(waveInfo);
        }
        StartCoroutine(SpawnWaves());
    }

    /// <summary>
    /// �����Ѷȵȼ���ֱ��ָ���������ɲ��������ֵ�
    /// </summary>
    /// <returns>���ɵĲ�������</returns>
    private WaveDataList GenerateRandomWaveData()
    {
        WaveDataList dataList = new WaveDataList();
        // ���ֱ��ָ���������ݣ�������ÿ�ֵ����������������� 0������ʹ�ø�����
        if (useDirectWaveSettings && directWaveCount > 0 && directEnemyCount > 0)
        {
            dataList.waves = new WaveData[directWaveCount];
            // �������ɵĵ�������������ֻȡ enemyPrefabs ������ǰ difficultyLevel ������಻����������
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
                // ÿ���У�ÿ������ĵ������Ͷ����ɹ̶�����
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
            // Ĭ��ģʽ�������������� difficultyLevel������ĵ�����������Ҳȡ difficultyLevel��������Ԥ����������
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
                // ����ÿ������ĵ������ͣ�80% �ĸ��ʳ����ڸò��Σ����������1 �� 1+difficultyLevel*2��
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
                // ����ò���δѡ���κε��ˣ����������ѡ��һ�ֳ��� 1 ��
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
    /// Э�̣�������˳�����ɵ��˵�λ
    /// </summary>
    private IEnumerator SpawnWaves()
    {
        //�ȴ�һ��ʱ���ٿ�ʼ���ɵ���
        yield return new WaitForSeconds(waveInterval);
        foreach (WaveData wave in waveDataList.waves)
        {
            Debug.Log("��ʼ���ɲ��Σ�" + wave.waveNumber);
            foreach (EnemySpawnInfo spawnInfo in wave.enemySpawns)
            {
                // ���Ҷ�ӦԤ����
                if (enemyPrefabDict.TryGetValue(spawnInfo.enemyType, out GameObject enemyPrefab))
                {
                    for (int i = 0; i < spawnInfo.count; i++)
                    {
                        // ��ȡһ����Ч������λ��
                        Vector2 spawnPos = GetValidSpawnPosition();
                        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

                        // ÿ����������֮����ʱ
                        float delay = UnityEngine.Random.Range(minSpawnInterval, maxSpawnInterval);
                        yield return new WaitForSeconds(delay);
                    }
                }
                else
                {
                    Debug.LogWarning("δ�ҵ�����Ϊ " + spawnInfo.enemyType + " �ĵ���Ԥ����ӳ�䣡");
                }
            }
            // ����֮��ȴ��̶�ʱ��
            yield return new WaitForSeconds(waveInterval);
        }
    }

    /// <summary>
    /// ������ spawnArea ��Χ������һ����Чλ�ã���λ�ò��ܱ��ϰ���ռ�ã�
    /// ���� avoidTransforms �еĶ��󱣳����� avoidDistance ���롣
    /// </summary>
    /// <returns>��Ч����λ��</returns>
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

            // ����ѡλ���Ƿ��ϰ���ռ��
            Collider2D hit = Physics2D.OverlapCircle(candidate, 0.1f, obstacleLayer);
            if (hit != null)
                continue;

            // ����ѡλ���� avoidTransforms �ж���֮��ľ���
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
            Debug.LogWarning("���� " + attempts + " �γ��Ժ���δ�ҵ���ȫ��Ч������λ�ã�ʹ������ѡλ�á�");

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
