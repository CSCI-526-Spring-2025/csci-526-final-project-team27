using UnityEngine;

/// <summary>
/// 基本的生命值管理组件，用于2D角色或对象
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] public float maxHealth = 100;  // 最大生命值
    protected float currentHealth;                     // 当前生命值

    [Header("Floating Text Settings")]
    public GameObject FloatingHPCanvas;            // 含 Canvas 的浮动文字预制体 



    void Awake()
    {
        // 游戏开始时初始化生命值
        currentHealth = maxHealth;
        Debug.Log($"{gameObject.name} 的生命值已初始化为 {currentHealth}");
    }

    void Update()
    {
        //// 临时测试代码：按 Q 键掉血 10 点，按 E 键加血 10 点
        //if (Input.GetKeyDown(KeyCode.Q))
        //{
        //    TakeDamage(10);
        //}
        //if (Input.GetKeyDown(KeyCode.E))
        //{
        //    Heal(10);
        //}
    }

    /// <summary>
    /// 受到伤害，减少当前生命值，并检测是否死亡
    /// </summary>
    /// <param name="damage">伤害数值</param>
    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} 受到了 {damage} 点伤害，剩余生命值：{currentHealth}");

        // 生成红色伤害特效，偏移量使其出现在角色右上角
        ShowFloatingText("-" + damage, Color.red, new Vector3(0.5f, 1f, 0));

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 恢复生命值
    /// </summary>
    /// <param name="amount">恢复数值</param>
    public virtual void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        Debug.Log($"{gameObject.name} 治愈了 {amount} 点生命值，当前生命值：{currentHealth}");

        // 生成绿色治疗特效，偏移量使其出现在角色右上角
        ShowFloatingText("+" + amount, Color.green, new Vector3(0.5f, 1f, 0));
    }

    /// <summary>
    /// 生成浮动文字特效
    /// </summary>
    /// <param name="text">显示的文字</param>
    /// <param name="color">文字颜色</param>
    /// <param name="offset">相对于角色位置的偏移量</param>
    public void ShowFloatingText(string text, Color color, Vector3 offset)
    {
        if (FloatingHPCanvas != null)
        {
            Vector3 spawnPos = transform.position + offset;
            GameObject textObj = Instantiate(FloatingHPCanvas, spawnPos, Quaternion.identity);
            //Debug.Log($"生成浮动文字特效：{text}，颜色：{color}，位置：{spawnPos}");
            // 由于 FloatingHP 组件挂在预制体中的子对象上，所以用 GetComponentInChildren<>
            FloatingTextHP ft = textObj.GetComponentInChildren<FloatingTextHP>();
            if (ft != null)
            {
                ft.SetText(text, color);
            }
            else
            {
                Debug.LogWarning("未能在实例化的 FloatingHPCanvas 中找到 FloatingHP 组件！");
            }
        }
    }

    /// <summary>
    /// 角色死亡逻辑
    /// </summary>
    virtual public void Die()
    {
        Debug.Log($"{gameObject.name} 已经死亡！");
        // 此处可添加更多死亡时的逻辑（例如播放动画、生成掉落物等）
        Destroy(gameObject);
    }
}
