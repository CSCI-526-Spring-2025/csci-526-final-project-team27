using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 3f; // 子弹生存时间
    public int damage = 10; // 伤害值

    void Start()
    {
        Destroy(gameObject, lifeTime); // 一段时间后自动销毁子弹
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy")) // 如果子弹碰到敌人
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage); // 对敌人造成伤害
            }
            Destroy(gameObject); // 子弹销毁
        }
    }
}
