using UnityEngine;

public class DamageBallTeam : MonoBehaviour
{
    public float damageAmount = 10f; // 造成的伤害值
    private FirebaseDataUploader dataUploader;
    
    void Start()
    {
        // 获取FirebaseDataUploader的引用
        GameObject roomManager = GameObject.Find("RoomManager");
        if (roomManager != null)
        {
            dataUploader = roomManager.GetComponent<FirebaseDataUploader>();
        }
        else
        {
            Debug.LogError("找不到RoomManager对象！");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 检查是否碰到了玩家
        if (other.CompareTag("Enemy"))
        {
            // 尝试获取玩家的 Health_BC 组件
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damageAmount);
                
                // 记录伤害
                if (dataUploader != null)
                {
                    dataUploader.TrackTeammateDamage("RangedTeammate_2", damageAmount);
                }
            }
            // 造成伤害后销毁子弹
            Destroy(gameObject);
        }
        else
        {
            // 可选：碰到其他目标时处理（如环境、墙体），视需求而定
            if (other.CompareTag("Wall") || other.CompareTag("Door"))
            {
               Destroy(gameObject);
            }
        }
    }
}
