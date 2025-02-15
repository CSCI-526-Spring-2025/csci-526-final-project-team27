using UnityEngine;
using TMPro;

public class FloatingTextHP : MonoBehaviour
{
    public float moveSpeed = 1f;     // �����ƶ��ٶ�
    public float fadeDuration = 1f;  // ����ʱ��
    public float lifetime = 1f;      // ����ʱ�䣨�룩

    private TextMeshProUGUI textMesh;
    private Color textColor;

    void Awake()
    {
        // ֱ�ӻ�ȡ����ͬһ�����ϵ� TextMeshProUGUI ���
        textMesh = GetComponent<TextMeshProUGUI>();
        if (textMesh != null)
        {
            textColor = textMesh.color;
        }
        else
        {
            Debug.LogError("FloatingTextHP: δ���ҵ� TextMeshProUGUI ���������Ԥ����ṹ��");
        }
    }

    void Start()
    {
        Debug.Log("���ݻ٣�" + transform.root.name);
        Destroy(transform.root.gameObject, lifetime);
    }

    void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        textColor.a -= Time.deltaTime / fadeDuration;
        if (textMesh != null)
        {
            textMesh.color = textColor;
        }
    }

    public void SetText(string text, Color color)
    {
        if (textMesh != null)
        {
            textMesh.text = text;
            textMesh.color = color;
            textColor = color;
            Debug.Log($"���ø������֣�{text}����ɫ��{color}");
        }
    }
}
