using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 30; // 敌人血量

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("敌人受到伤害，剩余血量：" + health);

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("敌人死亡！");
        Destroy(gameObject); // 敌人销毁
    }
}
