using UnityEngine;

public class FinalBoss : MonoBehaviour
{
    public Canvas canvas;
    public GameObject textWin;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 确保canvas和textWin被正确引用
        if (canvas == null)
        {
            canvas = GetComponentInChildren<Canvas>();
        }
        if (textWin == null && canvas != null)
        {
            textWin = canvas.transform.Find("TextWin")?.gameObject;
        }
        
        // 初始时隐藏胜利文本
        if (textWin != null)
        {
            textWin.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 检查场景中是否还有带有Enemy标签的对象
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        // 如果没有敌人且胜利文本存在，则显示胜利文本
        if (enemies.Length == 0 && textWin != null)
        {
            textWin.SetActive(true);
        }
    }
}
