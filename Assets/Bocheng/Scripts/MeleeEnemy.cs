using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeleeEnemy : BaseEnemy
{
    public enum MoveMode { Aggressive, PlayerOnly } // 行动模式
    public MoveMode moveMode = MoveMode.Aggressive; // 默认行为模式

    public float moveSpeed = 3f;          // 移动速度
    public float searchInterval = 1.0f;   // 目标搜索间隔（可配置）

    private Transform playerTransform;    // 存储 Player 目标
    private List<Transform> teammates = new List<Transform>(); // 存储 Teammate 目标
    public Transform currentTarget;      // 当前锁定的目标
    private Rigidbody2D rb;
    public bool isAttacking = false;      // 攻击状态标志

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        CacheTargets(); // 一次性存储所有目标
        StartCoroutine(SearchTargetRoutine()); // 定期检查最近目标
    }

    void FixedUpdate()
    {
        // 当正在攻击时，确保速度归零
        if (isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        
        if (currentTarget != null)
        {
            MoveTowardsTarget();
        }
    }

    // **缓存所有 Player 和 Teammate（仅执行一次）**
    void CacheTargets()
    {
        // **存储唯一的 Player**
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj) playerTransform = playerObj.transform;

        // **存储所有 Teammate**
        teammates.Clear();
        foreach (GameObject teammateObj in GameObject.FindGameObjectsWithTag("Teammate"))
        {
            teammates.Add(teammateObj.transform);
        }

        // **默认锁定最近目标**
        FindNearestTarget();
    }

    // **移动到当前目标**
    void MoveTowardsTarget()
    {
        Vector2 direction = (currentTarget.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    // **定期重新检查最近目标**
    IEnumerator SearchTargetRoutine()
    {
        while (true)
        {
            FindNearestTarget();
            yield return new WaitForSeconds(searchInterval);
        }
    }

    // **搜索最近的目标**
    void FindNearestTarget()
    {
        if (moveMode == MoveMode.PlayerOnly)
        {
            currentTarget = playerTransform; // **只追踪 Player**
            return;
        }

        float minDistance = Mathf.Infinity;
        Transform nearestTarget = playerTransform; // 默认先设置为 Player

        // **检查 Teammate 距离**
        foreach (Transform teammate in teammates)
        {
            if (teammate == null) continue; // 防止存储的对象被销毁
            float distance = Vector2.Distance(transform.position, teammate.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestTarget = teammate;
            }
        }

        if(playerTransform != null)
        {
            // **检查 Player 距离**
            float playerDistance = Vector2.Distance(transform.position, playerTransform.position);
            if (playerDistance < minDistance)
            {
                minDistance = playerDistance;
                nearestTarget = playerTransform;
            }
        }

        currentTarget = nearestTarget;
    }

    // **目标消失或死亡时，清空目标**
    public void ClearTarget(Transform target)
    {
        if (currentTarget == target)
        {
            currentTarget = null;
        }
    }
}
