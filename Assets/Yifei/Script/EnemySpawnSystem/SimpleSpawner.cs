using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSpawner : MonoBehaviour
{
    #region ��������
    public enum RoomType { Normal, Elite, Boss }
    public enum SpawnMode { Dynamic, Static }
    public enum SpawnPosition { Random, FixedPoints, Perimeter }

    [Header("��������")]
    [Tooltip("����Ԥ����")]
    public GameObject roomPrefab;
    [Tooltip("��������")]
    public RoomType roomType;
    [Tooltip("����ģʽ")]
    public SpawnMode spawnMode;

    [Header("���˶�̬����")]
    [Tooltip("��̬����ʱ������������Χ��Сֵ")]
    public int dynamicSpawnRangeMin = 1;
    [Tooltip("��̬����ʱ������������Χ���ֵ")]
    public int dynamicSpawnRangeMax = 10;
    [Tooltip("���Ѷȱ仯��������������")] // X��: ��׼���Ѷ�(0-1) Y��: ��������
    public AnimationCurve difficultyCurve = AnimationCurve.Linear(0, 1, 1, 1);
    [Tooltip("����λ�ã���������Ե��")]
    public SpawnPosition dynamicSpawnPosition = SpawnPosition.Random;

    [Header("���˾�̬����")]
    public List<WaveGroup> waveGroups = new List<WaveGroup>();

    [Serializable]
    public class WaveGroup
    {
        [Tooltip("�������ƣ���ѡ��")]
        public string waveName = "Wave";
        [Tooltip("���ο�ʼǰ���ӳ�")]
        [Min(0)] public float preDelay;
        [Tooltip("�������ɵ�Ԫ")]
        public List<EnemySpawnUnit> units = new List<EnemySpawnUnit>();
    }

    [Serializable]
    public class EnemySpawnUnit
    {
        [Tooltip("����Ԥ���壨����� EnemyBase �����")]
        public GameObject enemyPrefab;
        [Tooltip("��������")]
        [Min(1)] public int count = 3;
        [Tooltip("���ɼ�����룩")]
        [Min(0)] public float interval = 0.5f;
        [Tooltip("����λ��ģʽ")]
        public SpawnPosition positionType;
        [Tooltip("����λ���б���Է������ģ�")]
        public List<Vector2> spawnPositions = new List<Vector2>();
    }
    #endregion

    #region ����ʱ����
    [Header("����ʱ����")]
    [SerializeField] private GlobalEnemyLibrary enemyLibrary;
    private Vector2 roomSize = Vector2.one * 10f;
    private List<GameObject> validEnemies = new List<GameObject>();
    [SerializeField] private float currentDifficulty;
    [SerializeField] private float spawnIntervalMin = 1f;
    [SerializeField] private float spawnIntervalMax = 3f;

    // ����ȫ������ʱ��֪ͨ�¼�
    public event Action<bool> RoomClearEvent;
    // �����������
    private List<GameObject> activeEnemies = new List<GameObject>();
    private int totalEnemiesSpawned;
    private int enemiesRemaining;
    private int roomCount; // �����ڼ����Ѷȣ��ӷ����������ȡ��

    // ��ʶ�Ƿ����е��˶����������
    private bool spawningFinished = false;
    #endregion

    private void Start()
    {
        InitializeRoom();
        Debug.Log("Room " + name + " initialized");

        // ��ȡ�����С���Է�������Ϊ Floor ��������Ϊ׼��
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

    #region �������
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

    // �޸ĺ������жϣ�ֻ�е����е�����������ҵ�����������ʱ�����㷿�����
    private void CheckRoomClear()
    {
        if (spawningFinished && enemiesRemaining <= 0)
        {
            RoomClearEvent?.Invoke(true);
            Debug.Log("Room " + name + " clear");
        }
    }
    #endregion

    #region ��̬����
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
        // ��̬������ɺ����ñ�־������鷿���Ƿ����
        spawningFinished = true;
        CheckRoomClear();
    }

    private Vector2 GetDynamicSpawnPosition()
    {
        // �����������Ϊ Boss������� Boss ר�������߼�
        if (roomType == RoomType.Boss)
        {
            return GetBossSpawnPosition();
        }

        // ���ݶ�̬���ɵ�����λ��ģʽ��ѡ�������߼�
        return dynamicSpawnPosition switch
        {
            SpawnPosition.Random => GetRandomPosition(),
            SpawnPosition.Perimeter => GetPerimeterPosition(),
            // FixedPoints �������ڶ�̬���ɣ�Ĭ�ϲ������λ��
            SpawnPosition.FixedPoints => GetRandomPosition(),
            _ => GetRandomPosition()
        };
    }

    // �ڷ����Ե����һ��λ��
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

    #region ��̬��������
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
        // ��̬��������ȫ����ɺ����ñ�־������鷿���Ƿ����
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
            // ��ȡ BaseEnemy �е� displayName
            var enemyBase = unit.enemyPrefab.GetComponent<BaseEnemy>();
            string enemyName = enemyBase != null ? enemyBase.displayName : "Unknown";
            Debug.LogWarning($"��λ {enemyName} δ��������λ�ã�ʹ�����λ��");
            return GetRandomPosition();
        }

        // ���ѡ��һ��Ԥ��λ�ã���ת��Ϊ�������꣨���Ƿ���ı任��
        Vector2 localPos = unit.spawnPositions[UnityEngine.Random.Range(0, unit.spawnPositions.Count)];
        return transform.TransformPoint(localPos);
    }
    #endregion

    #region ͨ���߼�
    private void CalculateDifficulty()
    {
        // ������Ը��ݷ��������������߼������Ѷȣ�Ŀǰ�̶�Ϊ 1
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
            Debug.LogWarning($"Ԥ���� {enemyPrefab.name} û�� EnemyBase ���");
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
