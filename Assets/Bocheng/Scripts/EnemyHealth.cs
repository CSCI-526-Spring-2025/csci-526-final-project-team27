using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : Health
{
    private EnemySpawner enemySpawner;

    [System.Serializable]
    // ���������¼�
    public class DeathEvent : UnityEvent<GameObject> { }
    // ���ڷ��ѡ��ٻ����������ȷ��������ϲ����ʡ��÷���OnIncrease.Invoke(this.gameObject, newEnemies);
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

        // ���ɺ�ɫ������Ч��ƫ����ʹ������ڽ�ɫ���Ͻ�
        ShowFloatingText("+" + amount + "!", Color.red, new Vector3(0.5f, 1f, 0));
    }

    public override void Die()
    {
        Debug.Log(this.gameObject.name + " is dead");
        base.Die();
        //�����µ�������
        OnDeath.Invoke(this.gameObject);

        //�ɵ�������
        enemySpawner.EnemyDie();
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
