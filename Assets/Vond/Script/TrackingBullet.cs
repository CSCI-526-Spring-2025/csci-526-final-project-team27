using UnityEngine;

/// <summary>
/// 追踪子弹：会持续追踪目标方向
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class TrackingBullet : MonoBehaviour
{
    private Transform target;
    private float speed;
    private float homingStrength; // 控制追踪力度（转向速度）
    private Rigidbody2D rb;
    public float damageAmount = 10f; // 造成的伤害值

    void OnTriggerEnter2D(Collider2D other)
    {
        // 检查是否碰到了玩家
        if (other.CompareTag("Player") || other.CompareTag("Teammate"))
        {
            // 尝试获取玩家的 Health_BC 组件
            Health_BC health = other.GetComponent<Health_BC>();
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
            if (other.CompareTag("Wall") || other.CompareTag("Door"))
            {
               Destroy(gameObject);
            }
        }
    }
    /// <summary>
    /// 初始化追踪子弹参数
    /// </summary>
    /// <param name="target">子弹追踪目标</param>
    /// <param name="speed">移动速度</param>
    /// <param name="homingStrength">追踪强度</param>
    public void Initialize(Transform target, float speed, float homingStrength)
    {
        this.target = target;
        this.speed = speed;
        this.homingStrength = homingStrength;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            // 无目标则直线前进
            rb.linearVelocity = transform.right * speed;
            return;
        }
        
        // 计算目标方向
        Vector2 desiredDirection = ((Vector2)target.position - rb.position).normalized;
        // 平滑调整当前速度趋向目标方向
        Vector2 newVelocity = Vector2.Lerp(rb.linearVelocity, desiredDirection * speed, homingStrength * Time.fixedDeltaTime);
        rb.linearVelocity = newVelocity;

        // 使子弹朝向移动方向
        if (newVelocity.sqrMagnitude > 0.1f)
        {
            float angle = Mathf.Atan2(newVelocity.y, newVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
