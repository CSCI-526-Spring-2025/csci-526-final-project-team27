using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DamageReductionSkill : Skill
{
    public GameObject damageReductionSkillX;
    GameObject[] allies;
    
    // 存储每个队友对应的特效对象
    private Dictionary<GameObject, GameObject> allyEffects = new Dictionary<GameObject, GameObject>();

    [Header("Reduction rate")]
    [Tooltip("减伤比率")]
    public float reductionRate = 0.5f;

    [Tooltip("用于标识友方单位的标签列表")]
    public string[] friendlyTags = new string[] { "Teammate", "Player" };

    /// <summary>
    /// 实施减伤效果，对所有具有 friendlyTags 中任一标签的单位生效
    /// </summary>
    protected override void OnSkillEffect(Vector2 direction)
    {
        DamageDeductionAllAllies();
        ShowHealEffect();
    }

    protected override void OnSkillEffect(GameObject target)
    {
        DamageDeductionAllAllies();
        ShowHealEffect();
    }

    /// <summary>
    /// 对场景中所有拥有相关友方单位标签的单位应用减伤效果
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
                    Debug.Log($"对 {ally.name} 使用了减伤技能，减伤率：{health.defenseBuff}");
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
    /// 处理特效：预制体环状实例化于每个友方单位的脚下
    /// </summary>
    private void ShowHealEffect()
    {
        GameObject prefab = damageReductionSkillX;
        if (prefab == null)
        {
            Debug.LogError("Prefab 'DamageReductionSkillX' 未找到！");
            return;
        }
        
        // 清空之前的特效字典
        allyEffects.Clear();
        
        foreach (string tag in friendlyTags)
        {
            GameObject[] allies = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject ally in allies)
            {
                // 为每个友方单位创建特效
                Collider2D col = ally.GetComponent<Collider2D>();
                Vector3 offset = new Vector3(0, -0.5f, 0);
                
                if (col != null)
                {
                    // 使用collider底部位置
                    float yOffset = col.bounds.min.y - ally.transform.position.y;
                    offset = new Vector3(0, yOffset, 0);
                }
                
                GameObject effect = Instantiate(prefab, ally.transform.position + offset, Quaternion.identity, transform);
                
                // 存储特效引用，以便后续更新位置
                allyEffects[ally] = effect;
                
                Debug.Log($"对 {ally.name} 使用了减伤特效");
            }
        }
    }
    
    void Update()
    {
        // 更新所有特效的位置，让它们跟随队友
        List<GameObject> destroyList = new List<GameObject>();
        
        foreach (var pair in allyEffects)
        {
            GameObject ally = pair.Key;
            GameObject effect = pair.Value;
            
            if (ally == null || effect == null)
            {
                // 如果队友或特效已被销毁，加入待删除列表
                destroyList.Add(ally);
                continue;
            }
            
            // 更新特效位置
            Collider2D col = ally.GetComponent<Collider2D>();
            Vector3 offset = new Vector3(0, -0.5f, 0);
            
            if (col != null)
            {
                float yOffset = col.bounds.min.y - ally.transform.position.y;
                offset = new Vector3(0, yOffset, 0);
            }
            
            effect.transform.position = ally.transform.position + offset;
        }
        
        // 清理无效引用
        foreach (GameObject key in destroyList)
        {
            if (allyEffects.ContainsKey(key) && allyEffects[key] != null)
            {
                Destroy(allyEffects[key]);
            }
            allyEffects.Remove(key);
        }
    }
    
    //重写Skill routine，结束时取消效果
    protected override IEnumerator SkillRoutine()
    {
        yield return new WaitForSeconds(skillDuration);
        CancelDamageDeduction();
        
        // 销毁所有特效
        foreach (var pair in allyEffects)
        {
            if (pair.Value != null)
            {
                Destroy(pair.Value);
            }
        }
        allyEffects.Clear();
    }
}