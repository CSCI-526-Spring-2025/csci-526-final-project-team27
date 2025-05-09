﻿using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : Health
{
    [Header("Coin Drop")]
    [SerializeField] private GameObject coinPrefab;

    [Header("EXP Drop")]
    [SerializeField] private GameObject expOrbPrefab;


    private SimpleSpawner enemySpawner;
    private FirebaseDataUploader dataUploader;

    [System.Serializable]
    // 用于死亡事件
    public class DeathEvent : UnityEvent<GameObject> { }
    // 用于分裂、召唤等情况，不确定放这儿合不合适。用法：OnIncrease.Invoke(this.gameObject, newEnemies);
    // 第一个参数是原始敌人，第二个参数是新生成的敌人
    // 需要保证OnIncrease先于OnDeath调用！
    public class IncreaseEvent : UnityEvent<GameObject, GameObject[]> { } 

    public DeathEvent OnDeath = new DeathEvent();
    public IncreaseEvent OnIncrease = new IncreaseEvent();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 获取 FirebaseDataUploader 组件
        dataUploader = FindFirstObjectByType<FirebaseDataUploader>();
        if (dataUploader == null)
        {
            Debug.LogWarning("No FirebaseDataUploader found in the scene");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSpawnner(SimpleSpawner spawner)
    {
        enemySpawner = spawner;
    }

    public override void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        // 生成红色治疗特效，偏移量使其出现在角色右上角
        ShowFloatingText("+" + amount + "!", Color.red, new Vector3(0.5f, 1f, 0));
    }

    public override void Die()
    {
        Debug.Log(this.gameObject.name + " is dead");

        // 3. renew EnemyKilled in FirebaseDataUploader
        if (dataUploader != null)
        {
            dataUploader.UpdateData("EnemyKilled", dataUploader.GetData("EnemyKilled") + 1);
        }

        // Notify spawner
        OnDeath.Invoke(this.gameObject);
 
        // 1. Spawn coin (if you have assigned coinPrefab)
        if (coinPrefab != null)
        {
            // Instantiate(coinPrefab, transform.position, Quaternion.identity); // 暂时移除金币
        }
        else
        {
            Debug.LogWarning("coinPrefab not set on " + gameObject.name);
        }

        // 生成经验球
        if(expOrbPrefab != null)
        {
            Instantiate(expOrbPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("expOrbPrefab not set on " + gameObject.name);
        }

        // 2. Call base.Die to handle anything else from the parent Health
        base.Die();
        //旧的生成器
        //enemySpawner.EnemyDie();

    }

    private void IncreaseEnemy()
    {
        //var newEnemies = new GameObject[increasePrefabs.Length];
        //for (int i = 0; i < increasePrefabs.Length; i++)
        //{
        //    newEnemies[i] = Instantiate(
        //        increasePrefabs[i],
        //        transform.position,
        //        Quaternion.identity
        //    );
        //}
        //OnIncrease.Invoke(gameObject, newEnemies);
    }
}
