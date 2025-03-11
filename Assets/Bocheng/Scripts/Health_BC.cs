using UnityEngine;




public class Health_BC : Health
{
    public HealthBar healthBar;

    public float defenseBuff = 1.0f;
    public float damageBuff = 1.0f;

    private float lastHurtingTime = 0;
    private float lastHelpingTime = 0;
    private string[] helps = { "Help!", "I'm dying!", "Save me!" };

    void Start()
    {
        if(currentHealth == 0)
            currentHealth = maxHealth;
        //测试用
        // currentHealth = maxHealth * 0.7f;
        if(healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }
    }

    void Update()
    {
        // 队友低血量呼救
        if (transform.CompareTag("Teammate"))
        {
            if (currentHealth < maxHealth * 0.5f)
            {
                // 随机时间间隔呼救
                if (Time.time - lastHelpingTime > Random.Range(5, 10))
                {
                    lastHelpingTime = Time.time;
                    // 随机生成呼救特效
                    ShowFloatingText(helps[Random.Range(0, helps.Length)], Color.yellow, new Vector3(0, 1f, 0));
                }

            }
        }

    }

    override public void TakeDamage(float damage) 
    {
        // 限制受伤频率
        if (Time.time - lastHurtingTime < 0.3f)
        {
            return;
        }
        lastHurtingTime = Time.time;

        currentHealth -= damage * defenseBuff;
        // 生成红色伤害特效，偏移量使其出现在角色右上角
        ShowFloatingText("-" + damage * defenseBuff, Color.red, new Vector3(0.5f, 1f, 0));

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
        currentHealth = Mathf.Clamp(currentHealth + heal, 0, maxHealth);
        // 生成绿色治疗特效，偏移量使其出现在角色右上角
        ShowFloatingText("+" + heal, Color.green, new Vector3(0.5f, 1f, 0));
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }
    }
    public void SetHealthBar(HealthBar healthBar)
    {
        this.healthBar = healthBar;
        healthBar.SetHealth(currentHealth, maxHealth);
    }
}
