using UnityEngine;

/// <summary>
/// ����������ֵ�������������2D��ɫ�����
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;  // �������ֵ
    private int currentHealth;                     // ��ǰ����ֵ

    [Header("Floating Text Settings")]
    public GameObject FloatingHPCanvas;            // �� Canvas �ĸ�������Ԥ����

    void Start()
    {
        // ��Ϸ��ʼʱ��ʼ������ֵ
        currentHealth = maxHealth;
        Debug.Log($"{gameObject.name} ������ֵ�ѳ�ʼ��Ϊ {currentHealth}");
    }

    void Update()
    {
        // ��ʱ���Դ��룺�� Q ����Ѫ 10 �㣬�� E ����Ѫ 10 ��
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TakeDamage(10);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            Heal(10);
        }
    }

    /// <summary>
    /// �ܵ��˺������ٵ�ǰ����ֵ��������Ƿ�����
    /// </summary>
    /// <param name="damage">�˺���ֵ</param>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} �ܵ��� {damage} ���˺���ʣ������ֵ��{currentHealth}");

        // ���ɺ�ɫ�˺���Ч��ƫ����ʹ������ڽ�ɫ���Ͻ�
        ShowFloatingText("-" + damage, Color.red, new Vector3(0.5f, 1f, 0));

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// �ָ�����ֵ
    /// </summary>
    /// <param name="amount">�ָ���ֵ</param>
    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        Debug.Log($"{gameObject.name} ������ {amount} ������ֵ����ǰ����ֵ��{currentHealth}");

        // ������ɫ������Ч��ƫ����ʹ������ڽ�ɫ���Ͻ�
        ShowFloatingText("+" + amount, Color.green, new Vector3(0.5f, 1f, 0));
    }

    /// <summary>
    /// ���ɸ���������Ч
    /// </summary>
    /// <param name="text">��ʾ������</param>
    /// <param name="color">������ɫ</param>
    /// <param name="offset">����ڽ�ɫλ�õ�ƫ����</param>
    void ShowFloatingText(string text, Color color, Vector3 offset)
    {
        if (FloatingHPCanvas != null)
        {
            Vector3 spawnPos = transform.position + offset;
            GameObject textObj = Instantiate(FloatingHPCanvas, spawnPos, Quaternion.identity);
            Debug.Log($"���ɸ���������Ч��{text}����ɫ��{color}��λ�ã�{spawnPos}");
            // ���� FloatingHP �������Ԥ�����е��Ӷ����ϣ������� GetComponentInChildren<>
            FloatingTextHP ft = textObj.GetComponentInChildren<FloatingTextHP>();
            if (ft != null)
            {
                ft.SetText(text, color);
            }
            else
            {
                Debug.LogWarning("δ����ʵ������ FloatingHPCanvas ���ҵ� FloatingHP �����");
            }
        }
    }

    /// <summary>
    /// ��ɫ�����߼�
    /// </summary>
    private void Die()
    {
        Debug.Log($"{gameObject.name} �Ѿ�������");
        // �˴�����Ӹ�������ʱ���߼������粥�Ŷ��������ɵ�����ȣ�
        Destroy(gameObject);
    }
}
