using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : Health
{
    private EnemySpawner enemySpawner;

    [System.Serializable]
    // 用于死亡事件
    public class DeathEvent : UnityEvent<GameObject> { }
    // 用于分裂、召唤等情况，不确定放这儿合不合适。用法：OnIncrease.Invoke(this.gameObject, newEnemies);
    public class IncreaseEvent : UnityEvent<GameObject, GameObject[]> { } 

    public DeathEvent OnDeath = new DeathEvent();
    public IncreaseEvent OnIncrease = new IncreaseEvent();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSpawnner(EnemySpawner spawner)
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
        base.Die();
        //适配新的生成器
        OnDeath.Invoke(this.gameObject);

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
