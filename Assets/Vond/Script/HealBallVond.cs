using UnityEngine;

public class HealBallVond : MonoBehaviour
{
    public float healAmount = 10f; // 造成的伤害值

    void OnTriggerEnter2D(Collider2D other)
    {
        // 检查是否碰到了玩家
        if (other.CompareTag("Enemy") || other.CompareTag("Teammate"))
        {
            // 尝试获取玩家的 Health_BC 组件
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.Heal(healAmount);
            }
            // 造成伤害后销毁子弹
            Destroy(gameObject);
        }
        else
        {
            // 可选：碰到其他目标时处理（如环境、墙体），视需求而定
            if(!other.CompareTag("Player"))
            {
                Destroy(gameObject);
            }
            
        }
    }
}
