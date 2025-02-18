using UnityEngine;

public class Health_BC : Health
{
    public HealthBar healthBar;
    public float defenseBuff = 1.0f;
    public float damageBuff = 1.0f;

    void Start()
    {
        currentHealth = maxHealth;
        //测试用
        // currentHealth = maxHealth * 0.7f;
        if(healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }
    }

    override public void TakeDamage(float damage) 
    {
        currentHealth -= damage * defenseBuff;
        // 生成红色伤害特效，偏移量使其出现在角色右上角
        ShowFloatingText("-" + damage, Color.red, new Vector3(0.5f, 1f, 0));

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    override public void Heal(float heal)
    {
        currentHealth = Mathf.Clamp(currentHealth + heal * defenseBuff, 0, maxHealth);
        // 生成绿色治疗特效，偏移量使其出现在角色右上角
        ShowFloatingText("+" + heal, Color.green, new Vector3(0.5f, 1f, 0));
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }
    }
}
