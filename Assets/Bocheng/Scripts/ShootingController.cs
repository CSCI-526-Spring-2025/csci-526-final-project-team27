using UnityEngine;
using UnityEngine.EventSystems;

public class ShootingController : MonoBehaviour
{
    public GameObject cursorPrefab;   // 跟随鼠标的物体（准星）
    public GameObject bulletPrefab;   // 子弹预制体
    public Transform firePoint;       // 子弹生成点（通常是玩家位置）
    public float bulletSpeed = 10f;   // 子弹速度
    public float bulletLifetime = 2f; // 子弹最大存在时间
    public GameObject inventoryPanel; 
    private GameObject cursorInstance;
    private bool isActive = false;
    private bool isLocked = false;

    void Start()
    {
        // 初始化
        cursorInstance = null;
        ToggleActive(true);
    }


    void Update()
    {
        if (isActive)
        {
            UpdateCursorPosition();

            if (Input.GetMouseButtonDown(0)) // 左键点击开火
            {
                Shoot();
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
    void Shoot()
    {
        if(isLocked)
            return;
        if(inventoryPanel.activeSelf &&EventSystem.current.IsPointerOverGameObject()){//when bag is open and cursor on bagpanel it won't shoot
            Debug.Log("click on the panel");
            return ;
        }
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                Vector2 shootDirection = (cursorInstance.transform.position - firePoint.position).normalized;
                rb.linearVelocity = shootDirection * bulletSpeed;
            }

            // 设定子弹销毁时间
            Destroy(bullet, bulletLifetime);
            Debug.Log("shoot!");
        }
    }

    public void LockShoot(bool bLock)
    {
        isLocked = bLock;
    }
}
