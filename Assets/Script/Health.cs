using UnityEngine;

/// <summary>
/// ����������ֵ�������������2D��ɫ�����
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;  // �������ֵ
    private int currentHealth;                     // ��ǰ����ֵ

    void Start()
    {
        // ��Ϸ��ʼʱ����ʼ����ǰ����ֵΪ�������ֵ
        currentHealth = maxHealth;
    }

    /// <summary>
    /// �ܵ��˺������ٵ�ǰ����ֵ��������Ƿ�����
    /// </summary>
    /// <param name="damage">��ɵ��˺�ֵ</param>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} �ܵ��� {damage} ���˺���ʣ������ֵ��{currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// ��ɫ�����߼���������������Ӳ��Ŷ�������Ч��
    /// </summary>
    private void Die()
    {
        Debug.Log($"{gameObject.name} �Ѿ�������");
        // �˴�������Ӹ�������ʱ�Ĵ����߼������粥���������������ɵ������
        Destroy(gameObject);
    }

    /// <summary>
    /// ��ѡ���ָ�����ֵ�ķ���
    /// </summary>
    /// <param name="amount">�ָ�������ֵ��ֵ</param>
    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        Debug.Log($"{gameObject.name} ������ {amount} ������ֵ����ǰ����ֵ��{currentHealth}");
    }
}
