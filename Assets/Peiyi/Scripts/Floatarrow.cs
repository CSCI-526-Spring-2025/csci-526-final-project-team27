using UnityEngine;

public class Floatarrow : MonoBehaviour
{
    public float floatSpeed = 3f; // 上下擺動速度
    public float floatHeight = 5f; // 擺動高度

    private Vector2 originalPos;
    private RectTransform rect;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        originalPos = rect.anchoredPosition;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        rect.anchoredPosition = originalPos + new Vector2(0, offset);
        //Debug.Log(rect.anchoredPosition); // ✅ 現在不會報錯了
    }
}

