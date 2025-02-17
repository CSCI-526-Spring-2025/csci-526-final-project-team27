using System.Collections;
using UnityEngine;

/// <summary>
/// 友方全体回复技能：
/// 按下技能键后，对场上所有具有指定友方标签（可多个）的单位进行回复。
/// 本技能不依赖目标或方向参数，可直接释放。
/// </summary>
public class HealAllSkill : Skill
{
    [Header("Heal All Settings")]
    [Tooltip("每个友方单位回复的生命值")]
    public int healAmount = 10;

    [Tooltip("用于标识友方单位的标签数组")]
    public string[] friendlyTags = new string[] { "Ally", "Player" };

    public GameObject healAllSkillEffect;   // 回复技能特效

    /// <summary>
    /// 实现技能效果：对所有具有 friendlyTags 中任一标签的单位进行回复
    /// </summary>
    protected override void OnSkillEffect(Vector2 direction)
    {
        HealAllAllies();
        ShowHealEffect();
    }

    /// <summary>
    /// 对场上所有符合条件的友方单位进行回复
    /// </summary>
    private void HealAllAllies()
    {
        foreach (string tag in friendlyTags)
        {
            GameObject[] allies = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject ally in allies)
            {
                Health health = ally.GetComponent<Health>();
                if (health != null)
                {
                    health.Heal(healAmount);
                }
            }
        }
    }

    /// <summary>
    /// 技能特效，将prefab的形状实例化到每个友方单位的脚下
    /// </summary>
    private void ShowHealEffect()
    {
        GameObject prefab = healAllSkillEffect;
        if (prefab == null)
        {
            Debug.LogError("Prefab 'HealAllSkillX' 未找到！");
            return;
        }
        int i = 0;
        foreach (string tag in friendlyTags)
        {
            GameObject[] allies = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject ally in allies)
            {
                i++;
                // 将实例化的效果作为当前脚本所在物体的子对象，并显示在友方单位脚下
                Collider2D col = ally.GetComponent<Collider2D>();
                if (col != null)
                {
                    // bounds.min.y 为 collider 的底部位置
                    float yOffset = col.bounds.min.y - ally.transform.position.y;
                    Vector3 offset = new Vector3(0, yOffset, 0);
                    Instantiate(prefab, ally.transform.position + offset, Quaternion.identity, transform);
                }
                else
                {
                    Instantiate(prefab, ally.transform.position + new Vector3(0, -0.5f, 0), Quaternion.identity, transform);
                }
                Debug.Log($"对 {ally.name} 使用了回复技能特效");
            }
        }
    }
}
