using UnityEngine;
using System.Collections;

public class DamageReductionSkill : Skill
{
    public GameObject damageReductionSkillX;
    GameObject[] allies;

    [Header("Reduction rate")]
    [Tooltip("������")]
    public float reductionRate = 0.5f;

    [Tooltip("���ڱ�ʶ�ѷ���λ�ı�ǩ����")]
    public string[] friendlyTags = new string[] { "Teammate", "Player" };

    /// <summary>
    /// ʵ�ּ���Ч���������о��� friendlyTags ����һ��ǩ�ĵ�λ����
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
    /// �Գ������з����������ѷ���λ����
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
    /// ȡ������Ч��
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
    /// ������Ч����prefab����״ʵ������ÿ���ѷ���λ�Ľ���
    /// </summary>
    private void ShowHealEffect()
    {
        GameObject prefab = damageReductionSkillX;
        if (prefab == null)
        {
            Debug.LogError("Prefab 'DamageReductionSkillX' δ�ҵ���");
            return;
        }
        int i = 0;
        foreach (string tag in friendlyTags)
        {
            GameObject[] allies = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject ally in allies)
            {
                i++;
                // ��ʵ������Ч����Ϊ��ǰ�ű�����������Ӷ��󣬲���ʾ���ѷ���λ����
                Collider2D col = ally.GetComponent<Collider2D>();
                if (col != null)
                {
                    // bounds.min.y Ϊ collider �ĵײ�λ��
                    float yOffset = col.bounds.min.y - ally.transform.position.y;
                    Vector3 offset = new Vector3(0, yOffset, 0);
                    Instantiate(prefab, ally.transform.position + offset, Quaternion.identity, transform);
                }
                else
                {
                    Instantiate(prefab, ally.transform.position + new Vector3(0, -0.5f, 0), Quaternion.identity, transform);
                }
                Debug.Log($"�� {ally.name} ʹ���˼�����Ч");
            }
        }
    }
    //��дSkill routine,���ټ�����Ч
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