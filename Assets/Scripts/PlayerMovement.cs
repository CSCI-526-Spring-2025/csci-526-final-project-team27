using UnityEngine;
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;  // 移动速度

    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 获取输入
        movement.x = Input.GetAxisRaw("Horizontal"); // A/D 或 左/右箭头
        movement.y = Input.GetAxisRaw("Vertical");   // W/S 或 上/下箭头

        movement.Normalize(); // 归一化防止对角线速度过快
    }

    void FixedUpdate()
    {
        // 移动角色
        rb.linearVelocity = movement * moveSpeed;
    }
}
