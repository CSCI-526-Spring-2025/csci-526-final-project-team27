using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Rigidbody2D rb;
    private Vector2 movement;
    public GameObject bulletPrefab; // 预制子弹
    public Transform firePoint; // 子弹发射点
    public float bulletSpeed = 10f;

    void Update()
    {
        // 移动输入
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // 射击输入
        if (Input.GetMouseButtonDown(0)) // 鼠标左键射击
        {
            Shoot();
        }

    }

    void FixedUpdate()
    {
        // 移动角色
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

void Shoot()
{
    if (bulletPrefab == null || firePoint == null)
    {
        Debug.LogError("BulletPrefab 或 FirePoint 没有赋值！");
        return;
    }

    // 计算鼠标点击的位置
    Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    mousePos.z = 0f; // 2D 游戏不需要 z 轴

    // 计算朝向
    Vector2 direction = (mousePos - firePoint.position).normalized;

    // 生成子弹
    GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
    
    // 让子弹朝向鼠标方向
    Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
    if (rb != null)
    {
        rb.linearVelocity = direction * bulletSpeed; // 让子弹沿着鼠标方向移动
    }

    // 旋转子弹，使其朝向运动方向（可选）
    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
}

}
