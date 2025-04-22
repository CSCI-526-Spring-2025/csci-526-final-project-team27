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
    [Tooltip("技能影响区域的半径 - 注意：此值会自动与基类的skillRadius同步")]
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

    private void Awake()
    {
        // 确保基类的Awake方法被调用
        base.Awake();
        
        // 将本技能的radius同步到基类的skillRadius
        // 这确保了技能范围指示器的大小与实际效果区域一致
        skillRadius = radius;
        
        // 保存父对象引用
        parentTransform = transform.parent;
    }

    // 当在Unity编辑器中修改组件值时调用
    private void OnValidate()
    {
        // 在编辑器中修改radius值时，同步到skillRadius
        // 这确保了当开发者调整半径时，指示器能够正确显示
        skillRadius = radius;
    }

    /// <summary>
    /// 通过点击地面释放技能，传入的方向参数实际为释放位置
    /// </summary>
    /// <param name="direction">技能中心位置</param>
    protected override void OnSkillEffect(Vector2 direction)
    {
        // 双向同步radius和skillRadius，确保它们值相同
        // 在运行时，有可能是通过修改skillRadius来改变范围
        radius = skillRadius;
        
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
        // 双向同步radius和skillRadius，确保它们值相同
        // 在运行时，有可能是通过修改skillRadius来改变范围
        radius = skillRadius;
        
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
    /// 同时禁用敌人的攻击能力
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
        RangedEnemy rangedEnemy = collision.GetComponent<RangedEnemy>(); // 临时代码，处理 RangedEnemy 的减速
        if (rangedEnemy != null)
        {
            // 避免重复记录
            if (!affectedEnemies.Contains(collision.gameObject))
            {
                affectedEnemies.Add(collision.gameObject);
                // 保存原始速度并应用减速
                originalSpeeds[collision.gameObject] = rangedEnemy.moveSpeed;
                rangedEnemy.moveSpeed *= slowFactor;
                //Debug.Log("Enemy slowed: " + collision.gameObject.name);
            }
        }
        
        // 禁用敌人的攻击能力 - 分别检查每种类型
        DisableAttackForAllEnemyTypes(collision.gameObject);
    }
    
    /// <summary>
    /// 为所有可能的敌人类型禁用攻击能力
    /// </summary>
    private void DisableAttackForAllEnemyTypes(GameObject enemy)
    {
        // RangedEnemy目录下的敌人类型
        DisableAttackForComponent<TrackRangedEnemy>(enemy);
        DisableAttackForComponent<RangedEnemy>(enemy);
        DisableAttackForComponent<SideRangedEnemy>(enemy);
        DisableAttackForComponent<EightDirectionRangedEnemy>(enemy);
        
        // 特殊命名的敌人类型
        DisableAttackForComponent<RangedRectangularEnemy>(enemy); // RectEnemy.cs中的类
        DisableAttackForComponent<RangedFanSprayEnemy>(enemy);    // SprayEnemy.cs中的类
        
        // MeleeEnemy目录下的敌人类型
        DisableAttackForComponent<MeleeEnemy>(enemy);
        DisableAttackForComponent<WanderingEnemy>(enemy);
        DisableAttackForComponent<FourDirectionWanderingEnemy>(enemy);
        DisableAttackForComponent<SuicideEnemy>(enemy);
        DisableAttackForComponent<ChargingEnemy>(enemy);
    }
    
    /// <summary>
    /// 为指定类型的组件禁用攻击能力
    /// </summary>
    private void DisableAttackForComponent<T>(GameObject enemy) where T : MonoBehaviour
    {
        T component = enemy.GetComponent<T>();
        if (component != null)
        {
            var field = typeof(T).GetField("attackDisabledBySkill");
            if (field != null)
            {
                field.SetValue(component, true);
            }
        }
    }

    /// <summary>
    /// 当碰撞体离开区域时恢复敌人原始移动速度
    /// 并恢复攻击能力
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
        
        // 恢复敌人的攻击能力
        EnableAttackForAllEnemyTypes(collision.gameObject);
    }
    
    /// <summary>
    /// 为所有可能的敌人类型恢复攻击能力
    /// </summary>
    private void EnableAttackForAllEnemyTypes(GameObject enemy)
    {
        // RangedEnemy目录下的敌人类型
        EnableAttackForComponent<TrackRangedEnemy>(enemy);
        EnableAttackForComponent<RangedEnemy>(enemy);
        EnableAttackForComponent<SideRangedEnemy>(enemy);
        EnableAttackForComponent<EightDirectionRangedEnemy>(enemy);
        
        // 特殊命名的敌人类型
        EnableAttackForComponent<RangedRectangularEnemy>(enemy); // RectEnemy.cs中的类
        EnableAttackForComponent<RangedFanSprayEnemy>(enemy);    // SprayEnemy.cs中的类
        
        // MeleeEnemy目录下的敌人类型
        EnableAttackForComponent<MeleeEnemy>(enemy);
        EnableAttackForComponent<WanderingEnemy>(enemy);
        EnableAttackForComponent<FourDirectionWanderingEnemy>(enemy);
        EnableAttackForComponent<SuicideEnemy>(enemy);
        EnableAttackForComponent<ChargingEnemy>(enemy);
    }
    
    /// <summary>
    /// 为指定类型的组件恢复攻击能力
    /// </summary>
    private void EnableAttackForComponent<T>(GameObject enemy) where T : MonoBehaviour
    {
        T component = enemy.GetComponent<T>();
        if (component != null)
        {
            var field = typeof(T).GetField("attackDisabledBySkill");
            if (field != null)
            {
                field.SetValue(component, false);
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
                
                // 恢复敌人的攻击能力
                EnableAttackForAllEnemyTypes(enemy);
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

            // 确保特效大小与技能的实际影响半径一致
            // 这样视觉效果和实际影响范围能够匹配
            currentEffect.transform.localScale = new Vector3(radius, radius, 1);
        }

        if (animator != null)
        {
            animator.Play(animationStateName);
        }
    }
}
