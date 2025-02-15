using UnityEngine;
using TMPro;

public class FloatingTextHP : MonoBehaviour
{
    public float moveSpeed = 1f;     // 向上移动速度
    public float fadeDuration = 1f;  // 渐隐时间
    public float lifetime = 1f;      // 存在时间（秒）

    private TextMeshProUGUI textMesh;
    private Color textColor;

    void Awake()
    {
        // 直接获取挂在同一对象上的 TextMeshProUGUI 组件
        textMesh = GetComponent<TextMeshProUGUI>();
        if (textMesh != null)
        {
            textColor = textMesh.color;
        }
        else
        {
            Debug.LogError("FloatingTextHP: 未能找到 TextMeshProUGUI 组件，请检查预制体结构！");
        }
    }

    void Start()
    {
        Debug.Log("待摧毁：" + transform.root.name);
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
            Debug.Log($"设置浮动文字：{text}，颜色：{color}");
        }
    }
}
