using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum SkillReleaseType
{
    Direct,   // 直接释放
    Ground,   // 点地释放
    Target    // 点击单位释放
}

/// <summary>
/// Skill 基类：所有技能预制体应挂载此组件（的派生类）。
/// 集成了冷却时间控制、技能生命周期管理和技能效果触发逻辑。
/// 派生类只需实现 OnSkillEffect 方法来处理技能效果，无需关心冷却检测和销毁逻辑。
/// </summary>
public abstract class Skill : MonoBehaviour
{
    [Header("Skill Settings")]
    [Tooltip("技能持续时间，技能执行完后自动销毁")]
    public float skillDuration = 1f;

    [Tooltip("技能冷却时间，在此时间内相同类型的技能无法再次释放")]
    public float cooldownTime = 1f;
    
    [Tooltip("技能影响范围半径，用于显示指示器")]
    public float skillRadius = 1f;

    [Header("Skill Metadata")]
    [Tooltip("技能释放模式（直接释放、点地释放或点击单位释放）")]
    public SkillReleaseType releaseType = SkillReleaseType.Direct;

    [Header("Skill UI")]
    public string skillName;

    // 静态字典记录每种技能类型上次使用的时间（按类型名区分）
    public static Dictionary<string, float> lastUsedTimeBySkill = new Dictionary<string, float>();
    
    // 静态字典记录每种技能类型的冷却结束时间（按类型名区分）
    private static Dictionary<string, float> cooldownEndTimeBySkill = new Dictionary<string, float>();
    
    // 静态字典记录每种技能类型的闲置时间（可用但未使用的时间）
    private static Dictionary<string, float> idleTimeBySkill = new Dictionary<string, float>();
    
    // 静态字典记录每种技能第一次可用的时间
    private static Dictionary<string, float> firstAvailableTimeBySkill = new Dictionary<string, float>();
    
    // 游戏开始时间
    private static float gameStartTime = 0f;

    // 记录上一次更新闲置时间的时刻
    private static float lastIdleTimeUpdateTime = 0f;
    
    // 更新闲置时间的间隔（秒）
    private static float idleTimeUpdateInterval = 0.5f;

    public void Awake()
    {
        string key = GetType().Name;
        if (!lastUsedTimeBySkill.ContainsKey(key))
        {
            lastUsedTimeBySkill[key] = -100;
        }
        
        if (!cooldownEndTimeBySkill.ContainsKey(key))
        {
            cooldownEndTimeBySkill[key] = 0f;
        }
        
        if (!idleTimeBySkill.ContainsKey(key))
        {
            idleTimeBySkill[key] = 0f;
        }
        
        if (!firstAvailableTimeBySkill.ContainsKey(key))
        {
            firstAvailableTimeBySkill[key] = Time.time;
        }
        
        // 只在第一次调用时初始化游戏开始时间
        if (gameStartTime == 0f)
        {
            gameStartTime = Time.time;
            lastIdleTimeUpdateTime = gameStartTime;
            
            // 启动定期更新闲置时间的协程
            StartCoroutine(UpdateAllSkillsIdleTime());
        }
    }

    /// <summary>
    /// 定期更新所有技能的闲置时间
    /// </summary>
    private IEnumerator UpdateAllSkillsIdleTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(idleTimeUpdateInterval);
            float currentTime = Time.time;
            float deltaTime = currentTime - lastIdleTimeUpdateTime;
            
            foreach (var key in cooldownEndTimeBySkill.Keys)
            {
                if (currentTime >= cooldownEndTimeBySkill[key])
                {
                    // 技能当前可用，增加闲置时间
                    if (!idleTimeBySkill.ContainsKey(key))
                    {
                        idleTimeBySkill[key] = 0f;
                    }
                    idleTimeBySkill[key] += deltaTime;
                    
                    // 更新数据追踪器
                    UpdateSkillIdleDurationInTracker(key);
                }
            }
            
            lastIdleTimeUpdateTime = currentTime;
        }
    }

    /// <summary>
    /// 更新技能闲置时间比例到数据追踪器
    /// </summary>
    private void UpdateSkillIdleDurationInTracker(string skillKey)
    {
        FirebaseDataUploader dataUploader = FindObjectOfType<FirebaseDataUploader>();
        if (dataUploader != null)
        {
            float totalTime = Time.time - firstAvailableTimeBySkill[skillKey];
            if (totalTime > 0)
            {
                float idleRatio = idleTimeBySkill[skillKey] / totalTime;
                dataUploader.TrackSkillIdleDuration(skillKey, idleRatio);
            }
        }
    }

    /// <summary>
    /// 检查当前技能是否处于冷却状态
    /// </summary>
    public bool IsOnCooldown()
    {
        string key = GetType().Name;
        if (lastUsedTimeBySkill.ContainsKey(key))
        {
            float remaining = cooldownTime - (Time.time - lastUsedTimeBySkill[key]);
            if (remaining > 0)
            {
                Debug.Log($"技能 {key} 处于冷却中，剩余时间：{remaining} 秒");
                return true;
            }
            else
            {
                // 确保冷却结束时间已更新
                cooldownEndTimeBySkill[key] = lastUsedTimeBySkill[key] + cooldownTime;
            }
        }
        return false;
    }

    /// <summary>
    /// 设置当前技能的冷却起始时间为当前时间
    /// </summary>
    protected void SetCooldown()
    {
        string key = GetType().Name;
        float currentTime = Time.time;
        
        // 更新冷却结束时间
        cooldownEndTimeBySkill[key] = currentTime + cooldownTime;
        lastUsedTimeBySkill[key] = currentTime;
        
        // 更新技能使用次数
        FirebaseDataUploader dataUploader = FindObjectOfType<FirebaseDataUploader>();
        if (dataUploader != null)
        {
            dataUploader.TrackSkillUsage(key);
        }
    }

    /// <summary>
    /// 获取技能闲置时间比例
    /// </summary>
    public static float GetSkillIdleRatio(string skillKey)
    {
        if (firstAvailableTimeBySkill.ContainsKey(skillKey))
        {
            float totalTime = Time.time - firstAvailableTimeBySkill[skillKey];
            if (totalTime > 0 && idleTimeBySkill.ContainsKey(skillKey))
            {
                return idleTimeBySkill[skillKey] / totalTime;
            }
        }
        return 0f;
    }

    /// <summary>
    /// 尝试初始化技能（使用方向），内部会检查冷却状态，只有不在冷却时才触发技能效果。
    /// </summary>
    /// <param name="direction">释放方向</param>
    public void TryInitialize(Vector2 direction)
    {
        if (IsOnCooldown())
        {
            //Destroy(gameObject);
            return;
        }
        SetCooldown();
        OnSkillEffect(direction);
        StartCoroutine(SkillRoutine());
    }

    /// <summary>
    /// 尝试初始化技能（使用目标），内部将计算目标方向后调用 TryInitialize(Vector2)
    /// </summary>
    /// <param name="target">技能目标对象</param>
    public void TryInitialize(GameObject target)
    {
        if (IsOnCooldown())
        {
            //Destroy(gameObject);
            return;
        }
        SetCooldown();
        OnSkillEffect(target);
        StartCoroutine(SkillRoutine());
    }

    /// <summary>
    /// 抽象方法：由派生类实现具体技能效果。参数 direction 表示技能释放的方向或目标指向。
    /// </summary>
    protected abstract void OnSkillEffect(Vector2 direction);

    protected abstract void OnSkillEffect(GameObject target);

    /// <summary>
    /// 默认的技能执行协程，等待 skillDuration 秒后销毁技能预制体。
    /// 子类可以重写此协程以实现更复杂的技能生命周期控制。
    /// </summary>
    protected virtual IEnumerator SkillRoutine()
    {
        yield return new WaitForSeconds(skillDuration);
        //Destroy(gameObject);
    }


}
