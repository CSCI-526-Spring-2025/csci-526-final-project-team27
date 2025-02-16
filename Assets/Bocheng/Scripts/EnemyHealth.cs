using UnityEngine;

public class EnemyHealth : Health
{
    private EnemySpawner enemySpawner;
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

    public override void Die()
    {
        enemySpawner.EnemyDie();
        base.Die();
    }
}
