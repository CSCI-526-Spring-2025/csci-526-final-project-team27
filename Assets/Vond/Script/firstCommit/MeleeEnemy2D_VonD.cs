using System;
using UnityEngine;

public class MeleeEnemy2D_VonD : MonoBehaviour
{
    public enum TargetPriority { PlayerFirst, AllyFirst }
    public TargetPriority priority = TargetPriority.PlayerFirst;

    [Header("Settings")]
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float attackRange = 1.5f;
    [SerializeField] float attackCooldown = 2f;
    [SerializeField] int damage = 15;
    [SerializeField] LayerMask targetLayer;

    private Transform currentTarget;
    private Rigidbody2D rb;
    private float lastAttackTime;
    private ContactFilter2D contactFilter;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(targetLayer);
        FindTarget();
    }

    void Update()
    {
        if (!currentTarget)
        {
            FindTarget();
            Debug.Log("No target found");
        }
        else
        {
            Vector2 direction = (currentTarget.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;

            // ¹¥»÷¼ì²â
            if (Vector2.Distance(transform.position, currentTarget.position) <= attackRange)
            {
                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    Attack();
                    lastAttackTime = Time.time;
                }
            }
        }
    }

    void FindTarget()
    {
        // Ê¹ÓÃÔ²ÐÎ·¶Î§¼ì²â
        Collider2D[] targets = Physics2D.OverlapCircleAll(
            transform.position,
            10f,
            targetLayer
        );

        if (targets.Length > 0)
        {
            currentTarget = priority switch
            {
                TargetPriority.PlayerFirst => Array.Find(targets, t => t.CompareTag("Player"))?.transform,
                TargetPriority.AllyFirst => Array.Find(targets, t => t.CompareTag("Ally"))?.transform,
                _ => targets[0].transform
            };
        }
    }

    void Attack()
    {
        // ÉÈÐÎ¹¥»÷¼ì²â
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            attackRange,
            targetLayer
        );

        foreach (var hit in hits)
        {
            //if (hit.TryGetComponent<Health2D>(out var health))
            //{
            //    health.TakeDamage(damage);
            //}
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}