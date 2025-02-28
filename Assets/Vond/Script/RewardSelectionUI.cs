using System;
using UnityEngine;
using UnityEngine.UI;

public class RewardSelectionUI : MonoBehaviour
{
    public Button healthButton;
    public Button attackButton;
    public Button speedButton;

    /// <summary>
    /// 当玩家选择奖励后调用回调，参数为奖励类型
    /// </summary>
    public Action<RewardType> onRewardSelected;

    public enum RewardType
    {
        Health,
        Attack,
        Speed
    }

    private void Awake()
    {
        if (healthButton != null)
            healthButton.onClick.AddListener(() => OnRewardChosen(RewardType.Health));
        if (attackButton != null)
            attackButton.onClick.AddListener(() => OnRewardChosen(RewardType.Attack));
        if (speedButton != null)
            speedButton.onClick.AddListener(() => OnRewardChosen(RewardType.Speed));
    }

    /// <summary>
    /// 玩家点击某个奖励按钮后的处理
    /// </summary>
    /// <param name="reward">玩家选择的奖励类型</param>
    private void OnRewardChosen(RewardType reward)
    {
        // 隐藏奖励选择界面
        gameObject.SetActive(false);
        // 调用回调函数，通知外部玩家选择了哪项奖励
        onRewardSelected?.Invoke(reward);
    }
}
