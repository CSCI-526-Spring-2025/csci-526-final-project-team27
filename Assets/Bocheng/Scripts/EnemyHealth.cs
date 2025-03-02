using UnityEngine;

public class EnemyHealth : Health
{
    [Header("Coin Drop")]
    [SerializeField] private GameObject coinPrefab;

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

    public override void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        // ���ɺ�ɫ������Ч��ƫ����ʹ������ڽ�ɫ���Ͻ�
        ShowFloatingText("+" + amount + "!", Color.red, new Vector3(0.5f, 1f, 0));
    }

    public override void Die()
    {
        Debug.Log(this.gameObject.name + " is dead");

        // Notify spawner
        if (enemySpawner != null)
        {
            enemySpawner.EnemyDie();
        }

        // 1. Spawn coin (if you have assigned coinPrefab)
        if (coinPrefab != null)
        {
            Instantiate(coinPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("coinPrefab not set on " + gameObject.name);
        }

        // 2. Call base.Die to handle anything else from the parent Health
        base.Die();
    }
}
