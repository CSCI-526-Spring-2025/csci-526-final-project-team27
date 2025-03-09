using UnityEngine;
using System.Collections;

public class SuicideEnemy : BaseEnemy
{
    [Header("警戒设置")]
    [Tooltip("当玩家或队友进入该范围时触发警戒状态")]
    public float alertRange = 5f;
    [Tooltip("冲刺速度（自爆前的高速移动）")]
    public float rushSpeed = 10f;
    [Tooltip("触发爆炸前的延时")]
    public float explosionDelay = 1f;

    [Header("爆炸设置")]
    [Tooltip("爆炸的影响半径")]
    public float explosionRadius = 3f;
    [Tooltip("爆炸造成的伤害")]
    public float explosionDamage = 20f;

    // 复用已有的移动实现
    public IMover mover;

    private Rigidbody2D rb;
    private bool isAlerted = false;
    private bool hasExploded = false;
    private Transform currentTarget;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 默认使用 SimpleMover 实现移动
        if (mover == null)
            mover = new SimpleMover();
    }

    void Update()
    {
        // 如果尚未警戒，则检测 alertRange 内是否有玩家或队友进入
        if (!isAlerted)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, alertRange);
            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Player") || hit.CompareTag("Teammate"))
                {
                    currentTarget = hit.transform;
                    isAlerted = true;
                    StartCoroutine(ActivateAndExplode());
                    break;
                }
            }
        }
    }

    void FixedUpdate()
    {
        // 处于警戒状态且目标存在时以高速冲刺，否则静止
        if (isAlerted && currentTarget != null && !hasExploded)
        {
            mover.Move(transform, rb, currentTarget, rushSpeed);
        }
        else
        {
            rb.linearVelocity  = Vector2.zero;
        }
    }

    IEnumerator ActivateAndExplode()
    {
        // 警戒状态持续一段时间后触发爆炸
        yield return new WaitForSeconds(explosionDelay);
        Explode();
    }

    void Explode()
    {
        if (hasExploded)
            return;

        hasExploded = true;

        // 在爆炸半径内查找所有目标并造成伤害
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hit in hitColliders)
        {
            if (hit.CompareTag("Player") || hit.CompareTag("Teammate"))
            {
                Health targetHealth = hit.GetComponent<Health>();
                if (targetHealth != null)
                {
                    targetHealth.TakeDamage(explosionDamage);
                }
            }
        }

        // 此处可播放爆炸特效、音效等，之后销毁自身
        Destroy(gameObject);
    }

    // 在 Scene 视图中可视化警戒范围和爆炸半径
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, alertRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
