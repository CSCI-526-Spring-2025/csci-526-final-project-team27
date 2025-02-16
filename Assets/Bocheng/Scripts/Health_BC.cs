using UnityEngine;

public class Health_BC : Health
{
    public HealthBar healthBar;                   

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
        currentHealth -= damage;
        if(healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    override public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        if(healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }
    }
}
