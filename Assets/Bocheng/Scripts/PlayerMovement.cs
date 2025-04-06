using UnityEngine;
public class PlayerMovement : MonoBehaviour, IDieAble
{
    public float moveSpeed = 5f;  // 移动速度

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool bCanMove = true;
    public GameObject gameOverUI;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if(bCanMove)
        {
            // 获取输入
            movement.x = Input.GetAxisRaw("Horizontal"); // A/D 或 左/右箭头
            movement.y = Input.GetAxisRaw("Vertical");   // W/S 或 上/下箭头

            movement.Normalize(); // 归一化防止对角线速度过快
        }
        else
        {
            movement = Vector2.zero;
        }
    }

    void FixedUpdate()
    {
        // 移动角色
        rb.linearVelocity = movement * moveSpeed;
    }

    public void LockMove(bool bLock)
    {
        bCanMove = !bLock;
    }

    //修改onDestroy方法,主角死亡后游戏结束
    void OnDestroy()
    {
        /*
        // 显示UI
        Debug.Log("游戏结束");
        //Time.timeScale = 0;
        if (gameOverUI != null)
            gameOverUI = Instantiate(gameOverUI);
        */
    }

    public void Die()
    {
        Debug.Log("游戏结束");
        Time.timeScale = 0;
        if (gameOverUI != null)
            gameOverUI = Instantiate(gameOverUI);
        Destroy(gameObject);
    }
}
