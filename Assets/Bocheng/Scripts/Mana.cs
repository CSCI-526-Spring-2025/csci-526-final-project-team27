using UnityEngine;
using UnityEngine.UI;

public class Mana : MonoBehaviour
{
    public float maxMana = 100f; // 最大魔法值
    public float currentMana; // 当前魔法值
    public float manaRegenRate = 5f; // 魔法值恢复速度
    public bool KeepRegen = false; // 是否保持恢复魔法值
    public float manaRegenInterval = 1f; // 魔法值恢复间隔
    public Image manaFill; // 魔法值条前景的 Image 组件

    private float LastUseTime = -10.0f; // 上次使用魔法值的时间

    private void Awake()
    {
        currentMana = maxMana; // 初始化当前魔法值为最大值
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(KeepRegen) // 如果保持恢复魔法值
        {
            Regen(manaRegenRate * Time.deltaTime); // 恢复魔法值
        }
        else
        {
            if(LastUseTime + manaRegenInterval < Time.time) // 如果距离上次使用魔法值的时间超过恢复间隔
            {
                Regen(manaRegenRate * Time.deltaTime); // 恢复魔法值
            }
        }
        UpdateManaFill(); // 更新魔法值条的填充量
    }

    public bool UseMana(float amount)
    {
        if(currentMana >= amount)
        {
            currentMana -= amount; // 消耗魔法值
            LastUseTime = Time.time; // 更新上次使用魔法值的时间
            return true; // 成功消耗魔法值
        }
        else
        {
            Debug.Log("Not enough mana!"); // 魔法值不足
            return false; // 消耗失败
        }
    }

    public void Regen(float amount)
    {
        currentMana += amount; // 恢复魔法值
        if(currentMana > maxMana) // 如果当前魔法值超过最大值
        {
            currentMana = maxMana; // 设置为最大值
        }
    }

    public void UpdateManaFill()
    {
        if(manaFill != null) // 如果魔法值条前景的 Image 组件不为空
        {
            manaFill.fillAmount = currentMana / maxMana; // 更新魔法值条的填充量
        }
    }
}
