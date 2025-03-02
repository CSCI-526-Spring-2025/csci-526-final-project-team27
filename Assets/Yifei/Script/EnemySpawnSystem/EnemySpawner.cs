using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntelligentSpawner : MonoBehaviour
{
    [Header("核心配置")]
    [SerializeField] private RoomConfig roomConfig;
    [SerializeField] private GlobalEnemyLibrary enemyLibrary;
    [SerializeField] private Transform[] fixedSpawnPoints;
    private Vector2 roomSize = Vector2.one * 10f;

    [Header("运行时参数")]
    private List<EnemyConfig> validEnemies = new List<EnemyConfig>();
    private float currentDifficulty;

    // 敌人全部死亡时的通知事件
    public event Action<bool> RoomClearEvent;

    // 敌人数量监控
    private List<GameObject> activeEnemies = new List<GameObject>();
    private int totalEnemiesSpawned;
    private int enemiesRemaining;
    private int roomCount; // 可用于计算难度，从房间管理器获取？

    private void Start()
    {
        InitializeRoom();

        Transform floor = transform.Find("Floor");
        if (floor != null)
            roomSize = floor.localScale;
        else
            Debug.LogWarning("Room " + name + " has no floor!");

        CalculateDifficulty();
        FilterValidEnemies();

        switch (roomConfig.spawnMode)
        {
            case RoomConfig.SpawnMode.Dynamic:
                StartCoroutine(SpawnDynamicWave());
                break;
            case RoomConfig.SpawnMode.Static:
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
    }

    public void RegisterEnemy(GameObject enemy)
    {
        enemy.transform.SetParent(transform);
        enemy.tag = "Enemy";
        activeEnemies.Add(enemy);
        totalEnemiesSpawned++;
        enemiesRemaining++;

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
        }
    }
    #endregion

    #region 动态生成
    private IEnumerator SpawnDynamicWave()
    {
        int spawnCount = Mathf.RoundToInt(
            UnityEngine.Random.Range(
                roomConfig.dynamicSpawnRange.x,
                roomConfig.dynamicSpawnRange.y + 1
            ) * roomConfig.difficultyCurve.Evaluate(currentDifficulty)
        );

        for (int i = 0; i < spawnCount; i++)
        {
            var enemy = SelectEnemyByWeight();
            SpawnEnemy(enemy, GetDynamicSpawnPosition());
            yield return new WaitForSeconds(0.5f);
        }
    }

    private Vector2 GetDynamicSpawnPosition()
    {
        // 暂时的动态位置生成逻辑
        return roomConfig.roomType switch
        {
            RoomConfig.RoomType.Boss => GetBossSpawnPosition(),
            _ => GetPerimeterPosition()
        };
    }
    private Vector2 GetPerimeterPosition()
    {
        float angle = UnityEngine.Random.Range(0, 360f);
        Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad),
                             Mathf.Sin(angle * Mathf.Deg2Rad));
        return (Vector2)transform.position + dir * roomSize / 2;
    }

    private Vector2 GetBossSpawnPosition()
    {
        return transform.position;
    }
    #endregion

    #region 静态波次
    private IEnumerator SpawnStaticWaves()
    {
        foreach (var wave in roomConfig.waveGroups)
        {
            yield return new WaitForSeconds(wave.preDelay);

            List<Coroutine> spawnRoutines = new List<Coroutine>();
            foreach (var unit in wave.units)
            {
                spawnRoutines.Add(StartCoroutine(
                    SpawnUnit(unit)
                ));
            }

            foreach (var routine in spawnRoutines)
            {
                yield return routine;
            }

            //yield return new WaitForSeconds(wave.postDelay);
        }
    }

    private IEnumerator SpawnUnit(RoomConfig.EnemySpawnUnit unit)
    {
        for (int i = 0; i < unit.count; i++)
        {
            SpawnEnemy(unit.enemy, GetStaticSpawnPosition(unit.positionType));
            yield return new WaitForSeconds(unit.interval);
        }
    }

    private Vector2 GetStaticSpawnPosition(RoomConfig.SpawnPosition posType)
    {
        return posType switch
        {
            RoomConfig.SpawnPosition.FixedPoints => GetFixedPointPosition(),
            RoomConfig.SpawnPosition.Perimeter => GetPerimeterPosition(),
            _ => GetRandomPosition()
        };
    }

    private Vector2 GetFixedPointPosition()
    {
        return fixedSpawnPoints[UnityEngine.Random.Range(0, fixedSpawnPoints.Length)].position;
    }

    private Vector2 GetRandomPosition()
    {
        return (Vector2)transform.position + new Vector2(
            UnityEngine.Random.Range(-roomSize.x / 2, roomSize.x / 2),
            UnityEngine.Random.Range(-roomSize.y / 2, roomSize.y / 2)
        );
    }
    #endregion

    #region 通用逻辑
    private void CalculateDifficulty()
    {
        // 难度计算公式
        //currentDifficulty = 1f + roomCount;
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
        bool validType = roomConfig.roomType switch
        {
            RoomConfig.RoomType.Normal => enemy.allowInNormal,
            RoomConfig.RoomType.Elite => enemy.allowInElite,
            RoomConfig.RoomType.Boss => enemy.allowInBoss,
            _ => false
        };

        return validType &&
               currentDifficulty >= enemy.difficultyThreshold;
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
        Instantiate(config.prefab, position, Quaternion.identity);
    }
    #endregion
}