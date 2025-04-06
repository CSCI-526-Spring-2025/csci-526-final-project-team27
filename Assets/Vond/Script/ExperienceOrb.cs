using UnityEngine;

public class ExperienceOrb : MonoBehaviour
{
    [Header("经验球设置")]
    public int experienceValue = 10;         // 经验球包含的经验值
    public float attractionSpeed = 5f;         // 吸附时的移动速度
    public float collectionDistance = 0.5f;    // 当与目标距离足够近时自动收集

    // 玩家吸收后通过 StartAttraction 方法传入目标 Transform
    private Transform targetPlayer;

    /// <summary>
    /// 外部调用：启动经验球的吸附效果，将目标设置为玩家的 Transform
    /// </summary>
    /// <param name="target">玩家或吸收区域的 Transform</param>
    public void StartAttraction(Transform target)
    {
        targetPlayer = target;
    }

    private void Update()
    {
        if (targetPlayer != null)
        {
            // 向目标平滑移动
            transform.position = Vector3.MoveTowards(transform.position, targetPlayer.position, attractionSpeed * Time.deltaTime);
            
            // 当距离足够近时，认为已被吸收
            if (Vector3.Distance(transform.position, targetPlayer.position) <= collectionDistance)
            {
                // 增加经验并销毁经验球
                ExperienceManager.Instance.AddExperience(experienceValue);
                Destroy(gameObject);
            }
        }
    }
}

