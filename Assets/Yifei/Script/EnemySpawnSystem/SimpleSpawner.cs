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
    [Tooltip("����Ԥ���壨������д�����и÷���ĸ����壩")]
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
        [Tooltip("������������")]
        public EnemyConfig enemy;
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
    private List<EnemyConfig> validEnemies = new List<EnemyConfig>();
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
            //Debug.Log("Enemy " + enemy.name + " dead");
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

    private void CheckRoomClear()
    {
        if (enemiesRemaining <= 0)
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
            UnityEngine.Random.Range(dynamicSpawnRangeMin,
            Mathf.Max(dynamicSpawnRangeMax, dynamicSpawnRangeMin))
            * difficultyCurve.Evaluate(currentDifficulty)
        );

        for (int i = 0; i < spawnCount; i++)
        {
            EnemyConfig enemy = SelectEnemyByWeight();
            SpawnEnemy(enemy, GetDynamicSpawnPosition());
            yield return new WaitForSeconds(UnityEngine.Random.Range(spawnIntervalMin, spawnIntervalMax));
        }
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
            yield return new WaitForSeconds(wave.preDelay);

            List<Coroutine> spawnRoutines = new List<Coroutine>();
            foreach (var unit in wave.units)
            {
                spawnRoutines.Add(StartCoroutine(SpawnUnit(unit)));
            }

            foreach (var routine in spawnRoutines)
            {
                yield return routine;
            }
        }
    }

    private IEnumerator SpawnUnit(EnemySpawnUnit unit)
    {
        for (int i = 0; i < unit.count; i++)
        {
            SpawnEnemy(unit.enemy, GetStaticSpawnPosition(unit));
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
            Debug.LogWarning($"��λ {unit.enemy.displayName} δ��������λ�ã�ʹ�����λ��");
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
        foreach (var enemy in enemyLibrary.allEnemies)
        {
            if (IsEnemyValid(enemy))
                validEnemies.Add(enemy);
        }
    }

    private bool IsEnemyValid(EnemyConfig enemy)
    {
        bool validType = roomType switch
        {
            RoomType.Normal => enemy.allowInNormal,
            RoomType.Elite => enemy.allowInElite,
            RoomType.Boss => enemy.allowInBoss,
            _ => false
        };

        return validType && currentDifficulty >= enemy.difficultyThreshold;
    }

    private EnemyConfig SelectEnemyByWeight()
    {
        float totalWeight = 0f;
        foreach (var e in validEnemies)
            totalWeight += e.spawnWeight;

        float randomPoint = UnityEngine.Random.Range(0, totalWeight);
        foreach (var e in validEnemies)
        {
            if (randomPoint < e.spawnWeight)
                return e;
            randomPoint -= e.spawnWeight;
        }
        return validEnemies[0];
    }

    private void SpawnEnemy(EnemyConfig config, Vector2 position)
    {
        GameObject enemyInstance = Instantiate(config.prefab, position, Quaternion.identity);
        RegisterEnemy(enemyInstance);
    }
    #endregion
}
