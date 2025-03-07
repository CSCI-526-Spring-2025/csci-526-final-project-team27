using UnityEngine;

public class DamageBallTeam : MonoBehaviour
{
    public float damageAmount = 10f; // 造成的伤害值

    void OnTriggerEnter2D(Collider2D other)
    {
        // 检查是否碰到了玩家
        if (other.CompareTag("Enemy"))
        {
            // 尝试获取玩家的 Health_BC 组件
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damageAmount);
            }
            // 造成伤害后销毁子弹
            Destroy(gameObject);
        }
        else
        {
            // 可选：碰到其他目标时处理（如环境、墙体），视需求而定
        }
    }
}
