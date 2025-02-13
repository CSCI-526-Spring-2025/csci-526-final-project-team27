using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image hpFill; // 血条前景的 Image 组件

    void Start()
    {
        hpFill = GetComponent<Image>();
    }

    // 更新血量条显示
    public void SetHealth(float currentHealth, float maxHealth)
    {
        hpFill.fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
        Debug.Log($"当前血量：{currentHealth}，最大血量：{maxHealth}");
    }
}
