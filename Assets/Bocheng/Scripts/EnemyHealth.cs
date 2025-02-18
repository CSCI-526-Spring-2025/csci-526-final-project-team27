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

    public override void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        // ���ɺ�ɫ������Ч��ƫ����ʹ������ڽ�ɫ���Ͻ�
        ShowFloatingText("+" + amount + "!", Color.red, new Vector3(0.5f, 1f, 0));
    }

    public override void Die()
    {
        Debug.Log(this.gameObject.name + " is dead");
        enemySpawner.EnemyDie();
        base.Die();
    }
}
