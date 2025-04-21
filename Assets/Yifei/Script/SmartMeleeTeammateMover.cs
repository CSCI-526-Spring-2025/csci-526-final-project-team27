using UnityEngine;

public class SmartMeleeTeammateMover : MonoBehaviour, IMover
{
    // 分离半径
    public float separationRadius = 1.0f;
    // 分离力权重
    public float separationForce = 1.0f;
    // 距离目标（玩家）小于此值时停止移动，防止堵塞
    public float stopDistance = 2f;
    // 碰撞体边缘距离小于此值视为接触
    public float touchThreshold = 0.05f;

    public Animator animator;
    
    // 缓存自己的碰撞体
    private Collider2D selfCollider;

    void Awake()
    {
        selfCollider = GetComponent<Collider2D>();
        if (selfCollider == null)
        {
            Debug.LogWarning("SmartMeleeTeammateMover没有找到Collider2D组件！");
        }
    }

    void Start()
    {

    }

    public void Move(Transform self, Rigidbody2D rb, Transform target, float moveSpeed)
    {
        if (target == null) return;

        // 如果目标是玩家且已经足够近，则停止移动
        if (target.CompareTag("Player"))
        {
            float distanceToPlayer = Vector2.Distance(self.position, target.position);
            if (distanceToPlayer <= stopDistance)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }
        }
        
        // 使用碰撞体距离检测，只针对当前目标
        Collider2D targetCollider = target.GetComponent<Collider2D>();
        if (selfCollider != null && targetCollider != null)
        {
            ColliderDistance2D colDistance = selfCollider.Distance(targetCollider);
            
            // 如果与目标碰撞体的距离很近，停止移动以避免推动目标
            if (colDistance.distance <= touchThreshold)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }
            
            // 如果正在接触目标碰撞体，也停止移动
            if (selfCollider.IsTouching(targetCollider))
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }
        }

        // 计算指向目标的方向向量
        Vector2 desiredDirection = ((Vector2)target.position - (Vector2)self.position).normalized;

        // 检测附近的队友和玩家以避免相互拥挤
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(self.position, separationRadius);
        Vector2 separation = Vector2.zero;
        foreach (Collider2D col in nearbyColliders)
        {
            if (col.transform == self) continue;

            // 当目标是玩家时，玩家也不需要排斥力
            if (target.CompareTag("Player") && col.CompareTag("Player"))
                continue;

            // 对于队友或玩家，根据距离生成排斥力
            if (col.CompareTag("Teammate") || col.CompareTag("Player"))
            {
                Vector2 diff = (Vector2)self.position - (Vector2)col.transform.position;
                float distance = diff.magnitude;
                if (distance > 0)
                {
                    separation += diff.normalized / distance;
                }
            }
        }

        // 将目标追踪方向和排斥力结合，得到一个较好的移动方向
        Vector2 finalDirection = (desiredDirection + separationForce * separation).normalized;

        // 设置刚体速度
        rb.linearVelocity = finalDirection * moveSpeed;

        if (animator != null)
        {
            if(rb.linearVelocity.magnitude > 0.1f)
            {
                animator.SetBool("isWalking", true);
            }
            else
            {
                animator.SetBool("isWalking", false);
            }
        }
    }
}
