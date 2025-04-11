using UnityEngine;
public class PlayerMovement : MonoBehaviour, IDieAble
{
    public float moveSpeed = 5f;  // 移动速度

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool bCanMove = true;
    public GameObject gameOverUI;
    public static bool isEnd = false;
    
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        isEnd = false;

        if(animator == null || spriteRenderer == null)
        {
            //get child gameobject nameed "Sprite" then get its animator
            GameObject spriteObject = transform.Find("Sprite").gameObject;
            if (spriteObject != null)
            {
                animator = spriteObject.GetComponent<Animator>();
                spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
                if (animator == null || spriteRenderer == null)
                {
                    Debug.LogError("Animator or SpriteRenderer not found in child object");
                }
            }
        }

        ShootingController shootingController = GetComponent<ShootingController>();
        if(shootingController != null)
        {
            shootingController.animator = animator; // 将Animator传递给射击控制器
            shootingController.spriteRenderer = spriteRenderer; // 将SpriteRenderer传递给射击控制器
        }
    }

    void Update()
    {
        if (bCanMove)
        {
            // 获取输入
            movement.x = Input.GetAxisRaw("Horizontal"); // A/D 或 左/右箭头
            movement.y = Input.GetAxisRaw("Vertical");   // W/S 或 上/下箭头

            /*
            if(movement.x !=0)
            {
                spriteRenderer.flipX = movement.x < 0; // 根据水平输入翻转角色
            }*/

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
        animator.SetBool("Moving", movement != Vector2.zero);
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
        isEnd = true;
        Debug.Log("isEnd is true");
        if (gameOverUI != null)
            gameOverUI = Instantiate(gameOverUI);
        Destroy(gameObject);
    }
}
