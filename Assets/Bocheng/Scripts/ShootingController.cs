using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ShootingController : MonoBehaviour
{
    [Header("子弹配置")]
    public GameObject cursorPrefab;   // 跟随鼠标的物体（准星）
    public GameObject bulletPrefab;   // 子弹预制体
    public Transform firePointRight;       // 子弹生成点（通常是玩家位置）
    public Transform firePointLeft;        // 子弹生成点（通常是玩家位置）
    public float bulletSpeed = 10f;   // 子弹速度
    public float bulletLifetime = 2f; // 子弹最大存在时间

    [Header("法力配置")]
    public float manaCost = 5.0f; // 每次发射消耗的魔法值
    public GameObject inventoryPanel;
    private Mana mana; // 魔法值组件引用

    private GameObject cursorInstance;
    private bool isActive = false;
    private bool isLocked = false;
    private Camera cam;

    [Header("动画配置")]
    public float cooldownTime = 0.3f; // 冷却时间
    public Animator animator; // 动画组件引用
    public SpriteRenderer spriteRenderer; // 精灵渲染器引用
    public float shootDelay = 0.1f; // 射击延迟时间
    public bool autoCooldown = true;

    public bool isOnCooldown = false; // 是否在冷却中
    private float cooldownTimer = 0f; // 冷却计时器


    void Start()
    {
        // 初始化
        cursorInstance = null;
        
        mana = GetComponent<Mana>(); // 获取魔法值组件

        ToggleActive(true);
    }



    void Update()
    {
        UpdateFilpX(); // 更新角色翻转状态

        if (isActive)
        {
            UpdateCursorPosition();

            if (Input.GetMouseButtonDown(0)) // 左键点击开火
            {
                Shoot();
            }
        }

        
        if(isOnCooldown && autoCooldown)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= cooldownTime)
            {
                isOnCooldown = false; // 冷却结束
                cooldownTimer = 0f; // 重置计时器
            }
        }
    }

    // 激活/取消功能
    public void ToggleActive(bool bActive)
    {
        if(isActive == bActive)
        {
            return;
        }

        isActive = bActive;

        if (isActive)
        {
            cursorInstance = Instantiate(cursorPrefab);
        }
        else if (cursorInstance != null)
        {
            Destroy(cursorInstance);
        }
    }

    // 更新跟随鼠标的位置
    void UpdateCursorPosition()
    {
        
        if (cursorInstance != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f; // 保持在2D平面
            cursorInstance.transform.position = mousePos;
        }
    }

    // 生成并发射子弹
    bool Shoot()
    {
        if(isLocked || isOnCooldown)
            return false;

        if(inventoryPanel != null)
        {
            if (inventoryPanel.activeSelf && EventSystem.current.IsPointerOverGameObject())
            {//when bag is open and cursor on bagpanel it won't shoot
                Debug.Log("click on the panel");
                return false;
            }
        }

        if (mana != null)
        {
            if (!(mana.UseMana(manaCost)))
            {
                Debug.Log("Not enough mana!"); // 魔法值不足
                return false; // 如果魔法值不足，直接返回
            }
        }

        if (bulletPrefab != null && firePointRight != null && firePointLeft != null)
        {
            StartCoroutine(DelayShoot(shootDelay)); // 延迟发射子弹
            Debug.Log("shoot!");

            animator.Play("PlayerShoot");
            isOnCooldown = true; // 开始冷却

            return true; // 发射成功
        }
        return false;
    }


    public void LockShoot(bool bLock)
    {
        isLocked = bLock;

    }

    void UpdateFilpX()
    {
        //判断鼠标和角色的相对位置
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f; // 保持在2D平面
        Vector3 direction = mousePos - transform.position;
        if (direction.x < 0)
        {
            spriteRenderer.flipX = true; // 翻转角色
        }
        else
        {
            spriteRenderer.flipX = false; // 正常显示角色
        }
    }

    //协程，延迟射出子弹
    private IEnumerator DelayShoot(float delay)
    {
        yield return new WaitForSeconds(delay);
        Vector3 firePoint = spriteRenderer.flipX ? firePointLeft.position : firePointRight.position;
        GameObject bullet = Instantiate(bulletPrefab, firePoint, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            Vector2 shootDirection = (cursorInstance.transform.position - firePoint).normalized;
            rb.linearVelocity = shootDirection * bulletSpeed;
        }

        // 设定子弹销毁时间
        Destroy(bullet, bulletLifetime);
    }
}
