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
        if(healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }
    }
}
