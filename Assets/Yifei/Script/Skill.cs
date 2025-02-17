using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("Skill Metadata")]
    [Tooltip("技能释放模式（直接释放、点地释放或点击单位释放）")]
    public SkillReleaseType releaseType = SkillReleaseType.Direct;

    // 静态字典记录每种技能类型上次使用的时间（按类型名区分）
    private static Dictionary<string, float> lastUsedTimeBySkill = new Dictionary<string, float>();

    /// <summary>
    /// 检查当前技能是否处于冷却状态
    /// </summary>
    protected bool IsOnCooldown()
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
        }
        return false;
    }

    /// <summary>
    /// 设置当前技能的冷却起始时间为当前时间
    /// </summary>
    protected void SetCooldown()
    {
        string key = GetType().Name;
        lastUsedTimeBySkill[key] = Time.time;
        //Debug.Log($"重置技能 {key} 的冷却时间，持续 {cooldownTime} 秒");
    }

    /// <summary>
    /// 尝试初始化技能（使用方向），内部会检查冷却状态，只有不在冷却时才触发技能效果。
    /// </summary>
    /// <param name="direction">释放方向</param>
    public void TryInitialize(Vector2 direction)
    {
        if (IsOnCooldown())
        {
            Destroy(gameObject);
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
        Vector2 direction = Vector2.zero;
        if (target != null)
        {
            direction = ((Vector2)target.transform.position - (Vector2)transform.position).normalized;
        }
        TryInitialize(direction);
    }

    /// <summary>
    /// 抽象方法：由派生类实现具体技能效果。参数 direction 表示技能释放的方向或目标指向。
    /// </summary>
    protected abstract void OnSkillEffect(Vector2 direction);

    /// <summary>
    /// 默认的技能执行协程，等待 skillDuration 秒后销毁技能预制体。
    /// 子类可以重写此协程以实现更复杂的技能生命周期控制。
    /// </summary>
    protected virtual IEnumerator SkillRoutine()
    {
        yield return new WaitForSeconds(skillDuration);
        Destroy(gameObject);
    }
}
