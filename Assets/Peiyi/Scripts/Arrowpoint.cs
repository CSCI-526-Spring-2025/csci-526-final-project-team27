using UnityEngine;

public class ArrowPoint : MonoBehaviour
{
    private RectTransform resetarrow;         // 要指的箭頭
    private RectTransform resetButton;  // 目標按鈕（Reset Button）
    // private RectTransform arrow1;         // 要指的箭頭
    // private RectTransform skill1Button;
    // private RectTransform arrow2;         // 要指的箭頭
    // private RectTransform skill2Button;
    // private RectTransform arrow3;         // 要指的箭頭
    // private RectTransform skill3Button;

    private Canvas skillexchangecanvas;               // 指向這個 UI 所屬的 Canvas
    void Start()
    {
        GameObject skillCanvas = GameObject.Find("SkillCanva");

        if (skillCanvas != null)
        {
            resetarrow = skillCanvas.transform.Find("resetarrow")?.GetComponent<RectTransform>();
            resetButton = skillCanvas.transform.Find("ResetButton")?.GetComponent<RectTransform>();

            if (resetarrow != null && resetButton != null)
            {
                // 讓箭頭的位置對準按鈕
                resetarrow.position = resetButton.position;
                Debug.Log("✅ Arrow 指向 ResetButton 完成");
            }
            else
            {
                Debug.LogError("❌ 找不到 resetarrow 或 ResetButton");
            }

            
        }
        else
        {
            Debug.LogError("❌ 找不到 SkillCanva");
        }
    }
    
}
