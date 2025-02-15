using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerController_yf : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;                // 角色移动速度
    public Rigidbody2D rb;                      // 角色的 Rigidbody2D
    public Camera mainCamera;                   // 主摄像机，用于转换鼠标位置

    [Header("Attack Settings")]
    public Transform firePoint;                 // 发射点（子弹和技能通常从这里发射）
    public GameObject bulletPrefab;             // 远程子弹预制体（需包含 Bullet 脚本）
    public float bulletSpeed = 10f;
    public int bulletDamage = 10;

    [Header("Melee (Knockback) Attack Settings")]
    public GameObject meleeAttackPrefab;        // 近战击退攻击预制体（预制体内应有处理伤害、击退的逻辑）
    public float meleeRange = 2f;               // 近战检测范围
    public int meleeDamage = 15;                // 近战伤害
    public float meleeAngleThreshold = 30f;     // 攻击方向容差（角度，小于此角度认为在攻击扇形内）

    [Header("Skill Settings")]
    [Tooltip("技能预设数组（0：直接释放，1、2：目标/方向选择模式）")]
    public GameObject[] skillPrefabs;           // 三个技能预设（可替换）

    // 用于技能释放的内部变量
    private int selectedSkillIndex = -1;
    private bool isSkillTargetSelectionMode = false;

    void Update()
    {
        HandleMovement();
        //HandleRotation();

        // 默认攻击（左键）——如果未处于技能选择模式
        if (!isSkillTargetSelectionMode && Input.GetMouseButtonDown(0))
        {
            Vector2 aimDir = GetAimDirection();
            // 如果检测到敌人处于近战范围且在攻击方向上，则执行近战击退攻击，否则发射远程子弹
            if (IsEnemyInMeleeRange(aimDir))
                PerformMeleeAttack(aimDir);
            else
                ShootBullet(aimDir);
        }

        // 技能释放处理（以数字键 1~3 控制）
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // 假设技能1为直接释放型技能
            selectedSkillIndex = 0;
            UseSkill(selectedSkillIndex, GetAimDirection());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // 技能2需要先按键后点击确定释放方向
            selectedSkillIndex = 1;
            isSkillTargetSelectionMode = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            // 技能3也进入选择模式
            selectedSkillIndex = 2;
            isSkillTargetSelectionMode = true;
        }

        // 如果处于技能选择模式，则等待玩家点击左键确定释放方向
        if (isSkillTargetSelectionMode && Input.GetMouseButtonDown(0))
        {
            Vector2 targetDir = GetAimDirection();
            UseSkill(selectedSkillIndex, targetDir);
            isSkillTargetSelectionMode = false;
        }
    }

    /// <summary>
    /// 处理八向移动
    /// </summary>
    private void HandleMovement()
    {
        // 获取水平与垂直输入（注意：Input.GetAxisRaw 使输入即时响应）
        Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        rb.linearVelocity = moveInput * moveSpeed;
    }

    /// <summary>
    /// 角色朝向鼠标所在方向旋转
    /// </summary>
    private void HandleRotation()
    {
        Vector2 aimDirection = GetAimDirection();
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        // 根据角色初始朝向调整旋转角度（此处假设角色默认朝上，所以需减90°）
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    /// <summary>
    /// 获取角色朝向的瞄准方向（由鼠标位置决定）
    /// </summary>
    /// <returns>归一化的2D向量</returns>
    private Vector2 GetAimDirection()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;
        return direction;
    }

    /// <summary>
    /// 检测是否有敌人在近战范围内且位于攻击方向的扇形区域内
    /// </summary>
    /// <param name="attackDir">攻击方向</param>
    /// <returns>若存在则返回 true，否则 false</returns>
    private bool IsEnemyInMeleeRange(Vector2 attackDir)
    {
        // 在角色当前位置以 meleeRange 为半径检测敌人（假设敌人标记为 "Enemy"）
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, meleeRange);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Vector2 enemyDir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
                float angleDiff = Vector2.Angle(attackDir, enemyDir);
                if (angleDiff <= meleeAngleThreshold)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 执行远程攻击，发射子弹
    /// </summary>
    /// <param name="direction">攻击方向</param>
    private void ShootBullet(Vector2 direction)
    {
        // 实例化子弹，并通过 Bullet 脚本初始化（子弹 prefab 应包含 Bullet 脚本）
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            //bulletScript.Initialize(direction, bulletSpeed, bulletDamage);
        }
    }

    /// <summary>
    /// 执行近战击退攻击
    /// </summary>
    /// <param name="direction">攻击方向</param>
    private void PerformMeleeAttack(Vector2 direction)
    {
        // 实例化近战攻击预制体（预制体内需有处理伤害、击退的逻辑）
        GameObject meleeAttack = Instantiate(meleeAttackPrefab, transform.position, Quaternion.identity);
        // 使预制体朝向攻击方向
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        meleeAttack.transform.rotation = Quaternion.Euler(0, 0, angle);
        // 预制体中的脚本可在 OnTriggerEnter2D 中检测敌人，并调用其 Health.TakeDamage() 同时施加击退效果
    }

    /// <summary>
    /// 使用技能。技能预设必须有一个名为 Skill 的脚本，其中包含 Initialize(Vector2 direction) 方法。
    /// </summary>
    /// <param name="skillIndex">技能索引（0～2）</param>
    /// <param name="direction">释放方向</param>
    private void UseSkill(int skillIndex, Vector2 direction)
    {
        if (skillIndex < 0 || skillIndex >= skillPrefabs.Length)
            return;

        GameObject skillInstance = Instantiate(skillPrefabs[skillIndex], firePoint.position, Quaternion.identity);
        // 使技能效果朝向释放方向
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        skillInstance.transform.rotation = Quaternion.Euler(0, 0, angle);
        // 如果技能预设上有 Skill 脚本，则调用其 Initialize 方法
        //Skill skillScript = skillInstance.GetComponent<Skill>();
        //if (skillScript != null)
        //{
        //    skillScript.Initialize(direction);
        //}
    }

    #region Gizmos 绘制调试（可选）
    private void OnDrawGizmosSelected()
    {
        // 绘制近战检测范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
    }
    #endregion
}
