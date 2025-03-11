using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 点击地面释放的技能：对指定区域内的敌人施加减速效果
/// 目前，技能持续时间需要小于冷却时间，否则持续时间会出现问题
/// </summary>
public class GroundSlowSkill : Skill
{
    [Header("Ground Slow Skill Settings")]
    [Tooltip("技能影响区域的半径")]
    public float radius = 5f;
    [Tooltip("减速因子，1 表示原速，0.5 表示减速50%")]
    public float slowFactor = 0f;

    public GameObject groundSlowSkillX;   // 地面减速技能特效预制体

    // 记录当前被影响的敌人及其原始移动速度
    private List<GameObject> affectedEnemies = new List<GameObject>();
    private Dictionary<GameObject, float> originalSpeeds = new Dictionary<GameObject, float>();

    private CircleCollider2D circleCollider;
    // 保存技能特效实例引用
    private GameObject currentEffect;
    // 保存父对象引用
    private Transform parentTransform;



    /// <summary>
    /// 通过点击地面释放技能，传入的方向参数实际为释放位置
    /// </summary>
    /// <param name="direction">技能中心位置</param>
    protected override void OnSkillEffect(Vector2 direction)
    {
        transform.SetParent(null);
        transform.position = direction;
        // 将技能预制体移到点击位置
        //Debug.Log("GroundSlowSkill activated at position: " + direction);
        // 播放特效
        InstantiateSkillEffect();
        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider == null)
        {
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        circleCollider.isTrigger = true;
        circleCollider.radius = radius;
    }

    /// <summary>
    /// 通过目标释放技能，取目标位置作为技能中心
    /// </summary>
    /// <param name="target">技能目标对象</param>
    protected override void OnSkillEffect(GameObject target)
    {
        transform.SetParent(null);
        transform.position = target.transform.position;
        //Debug.Log("GroundSlowSkill activated at target position: " + transform.position);
        InstantiateSkillEffect();
        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider == null)
        {
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        circleCollider.isTrigger = true;
        circleCollider.radius = radius;
    }

    /// <summary>
    /// 当有碰撞体进入区域时检测是否为敌人，并应用减速效果
    /// </summary>
    /// <param name="collision">碰撞体</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 获取敌人主控脚本（假设其名为 BaseEnemy，并公开了 moveSpeed 属性）
        BaseEnemy enemyController = collision.GetComponent<BaseEnemy>();
        if (enemyController != null)
        {
            // 避免重复记录
            if (!affectedEnemies.Contains(collision.gameObject))
            {
                affectedEnemies.Add(collision.gameObject);
                // 保存原始速度并应用减速
                originalSpeeds[collision.gameObject] = enemyController.moveSpeed;
                enemyController.moveSpeed *= slowFactor;
                //Debug.Log("Enemy slowed: " + collision.gameObject.name);
            }
        }
    }

    /// <summary>
    /// 当碰撞体离开区域时恢复敌人原始移动速度
    /// </summary>
    /// <param name="collision">碰撞体</param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (affectedEnemies.Contains(collision.gameObject))
        {
            BaseEnemy enemyController = collision.GetComponent<BaseEnemy>();
            if (enemyController != null && originalSpeeds.ContainsKey(collision.gameObject))
            {
                enemyController.moveSpeed = originalSpeeds[collision.gameObject];
                //Debug.Log("Enemy speed restored: " + collision.gameObject.name);
            }
            affectedEnemies.Remove(collision.gameObject);
            if (originalSpeeds.ContainsKey(collision.gameObject))
            {
                originalSpeeds.Remove(collision.gameObject);
            }
        }
    }

    /// <summary>
    /// 技能执行协程，技能持续一段时间后恢复所有区域内敌人的速度
    /// 并销毁技能特效
    /// </summary>
    protected override IEnumerator SkillRoutine()
    {
        yield return new WaitForSeconds(skillDuration);

        // 恢复技能范围内所有敌人的移动速度
        foreach (var enemy in affectedEnemies)
        {
            if (enemy != null)
            {
                BaseEnemy enemyController = enemy.GetComponent<BaseEnemy>();
                if (enemyController != null && originalSpeeds.ContainsKey(enemy))
                {
                    enemyController.moveSpeed = originalSpeeds[enemy];
                    //Debug.Log("Enemy speed restored on skill end: " + enemy.name);
                }
            }
        }
        affectedEnemies.Clear();
        originalSpeeds.Clear();
        // 销毁碰撞体
        Destroy(circleCollider);
        transform.SetParent(parentTransform);
        // 销毁技能特效（注意，此时特效不再是子对象）
        if (currentEffect != null)
        {
            Destroy(currentEffect);
        }
    }

    /// <summary>
    /// 实例化技能特效，设置大小与技能半径匹配
    /// 注意：此处不设置父对象，避免随主角移动
    /// </summary>
    private void InstantiateSkillEffect()
    {
        if (groundSlowSkillX != null)
        {
            if (currentEffect != null)
            {
                Destroy(currentEffect);
            }
            //Debug.Log("Instantiate ground slow skill effect");
            // 实例化特效，不指定父对象，使其保持在世界空间中固定位置
            currentEffect = Instantiate(groundSlowSkillX, transform.position, Quaternion.identity);

            currentEffect.transform.localScale = new Vector3(radius * 1, radius * 1, 1);
        }
    }
}
