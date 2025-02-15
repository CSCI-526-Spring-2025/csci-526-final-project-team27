using UnityEngine;

public class Health_BC : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] public float maxHealth = 100;

    public HealthBar healthBar;

    private float currentHealth;                     

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

    public void TakeDamage(float damage)
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


    private void Die()
    {
        Destroy(gameObject);
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        if(healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }
    }
}
