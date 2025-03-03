using UnityEngine;

/// <summary>
/// 寻找玩家目标的实现，返回场景中 Tag 为 "Player" 的对象
/// </summary>
public class PlayerFinder : ITargetFinder 
{
    public Transform FindTarget(Transform self)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player ? player.transform : null;
    }
}
