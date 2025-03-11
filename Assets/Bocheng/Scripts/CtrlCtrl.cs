using UnityEngine;

public class CtrlCtrl : MonoBehaviour
{
    public static CtrlCtrl Instance { get; private set; } // 单例实例
    
    private PlayerMovement playerMovement;
    private ShootingController shootingController;

    private void Awake()
    {
        // 确保单例唯一性
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // 确保场景切换时不会销毁
        }
        else
        {
            Destroy(gameObject); // 如果已经存在实例，销毁当前对象
            return;
        }
    }

    void Start()
    {
        GetPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GetPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
            shootingController = player.GetComponent<ShootingController>();
        }
        else
        {
            Debug.LogWarning("No GameObject with tag 'Player' found in scene.");
        }
    }

    public void LockMove(bool bLock)
    {
        if (playerMovement != null)
        {
            playerMovement.LockMove(bLock);
        }
    }

    public void LockShoot(bool bLock)
    {
        if (shootingController != null)
        {
            shootingController.LockShoot(bLock);
        }
    }

    public void ToggleShootCtrler(bool bToggle)
    {
        if (shootingController != null)
        {
            shootingController.ToggleActive(bToggle);
        }
    }
}
