using UnityEngine;

/// <summary>
/// 基本的生命值管理组件，用于2D角色或对象
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;  // 最大生命值
    private int currentHealth;                     // 当前生命值

    void Start()
    {
        // 游戏开始时，初始化当前生命值为最大生命值
        currentHealth = maxHealth;
    }

    /// <summary>
    /// 受到伤害，减少当前生命值，并检测是否死亡
    /// </summary>
    /// <param name="damage">造成的伤害值</param>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} 受到了 {damage} 点伤害，剩余生命值：{currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 角色死亡逻辑：可以在这里添加播放动画、音效等
    /// </summary>
    private void Die()
    {
        Debug.Log($"{gameObject.name} 已经死亡！");
        // 此处可以添加更多死亡时的处理逻辑，例如播放死亡动画、生成掉落物等
        Destroy(gameObject);
    }

    /// <summary>
    /// 可选：恢复生命值的方法
    /// </summary>
    /// <param name="amount">恢复的生命值数值</param>
    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        Debug.Log($"{gameObject.name} 治愈了 {amount} 点生命值，当前生命值：{currentHealth}");
    }
}
