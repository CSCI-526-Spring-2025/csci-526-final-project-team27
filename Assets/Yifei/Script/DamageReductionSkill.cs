using UnityEngine;
using System.Collections;

public class DamageReductionSkill : Skill
{
    public GameObject damageReductionSkillX;
    GameObject[] allies;

    [Header("Reduction rate")]
    [Tooltip("减伤率")]
    public float reductionRate = 0.5f;

    [Tooltip("用于标识友方单位的标签数组")]
    public string[] friendlyTags = new string[] { "Teammate", "Player" };

    /// <summary>
    /// 实现技能效果：对所有具有 friendlyTags 中任一标签的单位减伤
    /// </summary>
    protected override void OnSkillEffect(Vector2 direction)
    {
        DamageDeductionAllAllies();
        //ShowHealEffect();
    }

    protected override void OnSkillEffect(GameObject target)
    {
        DamageDeductionAllAllies();
        //ShowHealEffect();
    }


    /// <summary>
    /// 对场上所有符合条件的友方单位减伤
    /// </summary>
    private void DamageDeductionAllAllies()
    {
        foreach (string tag in friendlyTags)
        {
            allies = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject ally in allies)
            {
                Health_BC health = ally.GetComponent<Health_BC>();
                if (health != null)
                {
                    health.defenseBuff = reductionRate;
                }
            }
        }
    }
    /// <summary>
    /// 取消减伤效果
    /// </summary>
    private void CancelDamageDeduction()
    {
        foreach (string tag in friendlyTags)
        {
            allies = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject ally in allies)
            {
                Health_BC health = ally.GetComponent<Health_BC>();
                if (health != null)
                {
                    health.defenseBuff = 1.0f;
                }
            }
        }
    }


    /// <summary>
    /// 技能特效，将prefab的形状实例化到每个友方单位的脚下
    /// </summary>
    private void ShowHealEffect()
    {
        GameObject prefab = damageReductionSkillX;
        if (prefab == null)
        {
            Debug.LogError("Prefab 'DamageReductionSkillX' 未找到！");
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
                Debug.Log($"对 {ally.name} 使用了减伤特效");
            }
        }
    }
    //重写Skill routine,销毁技能特效
    protected override IEnumerator SkillRoutine()
    {
        yield return new WaitForSeconds(skillDuration);
        CancelDamageDeduction();
        while (transform.childCount > 0)
        {
            Destroy(transform.GetChild(0).gameObject);
            yield return null;
        }
    }
}