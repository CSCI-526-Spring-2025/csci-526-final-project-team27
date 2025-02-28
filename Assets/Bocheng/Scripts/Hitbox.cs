using UnityEngine;
using System.Collections.Generic; // 使用 HashSet

public class Hitbox : MonoBehaviour
{
    public string targetTag = "Enemy";   // 需要检测的Tag
    public float hitboxDuration = 0.5f;  // Hitbox 存在时间
    public LayerMask targetLayer;        // 仅检测指定层
    public float knockbackForce = 5f;    // 物理击退力度（用于 Rigidbody2D）
    public float knockbackDistance = 2f; // 直接位移的距离（用于无 Rigidbody2D 的对象）
    public bool usePhysicsKnockback = true; // 是否使用物理击退（否则直接位移）

    private HashSet<GameObject> hitTargets = new HashSet<GameObject>(); // 记录已触发的目标

    void Start()
    {
        // hitboxDuration 时间后自动销毁
        Destroy(gameObject, hitboxDuration);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 确保物体在目标层内，并且 Tag 符合
        if (((1 << other.gameObject.layer) & targetLayer) != 0 && other.CompareTag(targetTag))
        {
            // 只触发一次
            if (!hitTargets.Contains(other.gameObject))
            {
                hitTargets.Add(other.gameObject);
                DoSomething(other.gameObject);
                ApplyKnockback(other.gameObject);
            }
        }
    }

    // 触发后执行的函数（如伤害、特效）
    void DoSomething(GameObject target)
    {
        Debug.Log("命中目标: " + target.name);
        // 这里可以添加伤害逻辑
    }

    // 施加击退效果
    void ApplyKnockback(GameObject target)
    {

        //Vector2 knockbackDirection = (target.transform.position - transform.position).normalized; // 方向 = 敌人 - 玩家
        Vector2 knockbackDirection = transform.right; // 朝向右侧
        if (usePhysicsKnockback)
        {
            // 使用 Rigidbody2D 物理冲击
            Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            }
        }
        else
        {
            // 直接调整位置（位移）
            target.transform.position += (Vector3)(knockbackDirection * knockbackDistance);
        }
    }
}
