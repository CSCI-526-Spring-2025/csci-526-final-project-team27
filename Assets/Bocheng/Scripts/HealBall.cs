using UnityEngine;

public class HealBall : MonoBehaviour
{
    public float healAmount = 5f; // 治疗量

    void OnTriggerEnter2D(Collider2D other)
    {

        // 只对目标 Tag 触发
        if (other.CompareTag("Player") && other.CompareTag("Enemy") && other.CompareTag("Teammate"))
        {
            DoSomething(other.gameObject);
        }

        Destroy(gameObject);
    }

    // **执行治疗或其他行为**
    void DoSomething(GameObject target)
    {
        Health health = target.GetComponent<Health>();
        if (health != null)
        {
            health.Heal(healAmount);
        }
    }
}
