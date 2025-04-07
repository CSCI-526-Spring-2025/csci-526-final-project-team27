using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Buff
{
    public string buffName;
    public GameObject buffPrefab;
}

public class LevelUpRewardSystem : MonoBehaviour
{
    [Header("Buff 池：在 Inspector 中添加或删除可用的 Buff")]
    public List<Buff> availableBuffs = new List<Buff>();

    [Header("奖励 UI 设置")]
    public GameObject rewardPanel;                        // 奖励面板
    public List<Transform> buffSlots = new List<Transform>(); // 3个空位Slot，需在 Inspector 中指定

    private List<Buff> currentChoices = new List<Buff>();

    public GameObject player;

    public void ShowRewardSelection()
    {
        Time.timeScale = 0f;
        if (CtrlCtrl.Instance != null)
        {
            CtrlCtrl.Instance.ToggleShootCtrler(false);
        }
        Debug.Log("函数已开始");
        // 清空旧的 slot 内容
        foreach (Transform slot in buffSlots)
        {
            foreach (Transform child in slot)
            {
                Destroy(child.gameObject);
            }
        }
        currentChoices.Clear();

        // 抽取 buff
        List<Buff> buffPoolCopy = new List<Buff>(availableBuffs);
        int choicesCount = Mathf.Min(buffSlots.Count, buffPoolCopy.Count);

        for (int i = 0; i < choicesCount; i++)
        {
            int index = Random.Range(0, buffPoolCopy.Count);
            Buff selectedBuff = buffPoolCopy[index];
            buffPoolCopy.RemoveAt(index);
            currentChoices.Add(selectedBuff);

            // 实例化到对应 slot 下
            GameObject buffButton = Instantiate(selectedBuff.buffPrefab, buffSlots[i]);

            // 添加点击事件
            Button button = buffButton.GetComponentInChildren<Button>();
            if (button != null)
            {
                Buff tempBuff = selectedBuff;
                button.onClick.AddListener(() => { OnBuffSelected(tempBuff); });
            }
        }

        rewardPanel.SetActive(true);
    }

    public void OnBuffSelected(Buff buff)
    {
        Debug.Log("玩家选择了 Buff: " + buff.buffName);
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }
        Health playerHealth = player.GetComponent<Health>();
        PlayerMovement playerspeed = player.GetComponent<PlayerMovement>();

        if (playerHealth != null)
        {
            switch (buff.buffName)
            {
                case "Heal HP":
                    playerHealth.Heal(20f);
                    break;

                case "MaxHP++":
                    float increaseAmount = 20f;
                    playerHealth.maxHealth += increaseAmount;

                    Debug.Log($"增加最大生命 {increaseAmount}，当前生命设为满血：{playerHealth.currentHealth}");

                    // 生成黄色提示
                    playerHealth.ShowFloatingText("Max HP ↑", Color.yellow, new Vector3(0.5f, 1f, 0));
                    break;
                case "Speed++":
                    float increaseSpeed = 1f;
                    playerspeed.moveSpeed += increaseSpeed;

                    Debug.Log($"增加最大速度 {increaseSpeed}，当前速度：{playerspeed.moveSpeed}");
                    break;
                case "Damage++":
                    GameObject[] teammates = GameObject.FindGameObjectsWithTag("Teammate");
                    foreach (GameObject teammate in teammates)
                    {
                        MeleeTeammate melee = teammate.GetComponent<MeleeTeammate>();
                        if (melee != null)
                        {
                            melee.damage += 1f;
                            continue;
                        }
                        // 如果是远程队友
                        RangedTeammate ranged = teammate.GetComponent<RangedTeammate>();
                        if (ranged != null)
                        {
                            if (ranged.bulletPrefab != null)
                            {
                                DamageBallTeam teamBullet = ranged.bulletPrefab.GetComponent<DamageBallTeam>();
                                if (teamBullet != null)
                                {
                                    teamBullet.damageAmount += 5;
                                    Debug.Log("Range teammate damage increased, new damage: " + teamBullet.damageAmount);
                                }
                            }
                            continue;
                        }
                    }
                    break;
                default:
                    Debug.LogWarning("未知 Buff: " + buff.buffName);
                    break;
            }
        }

        rewardPanel.SetActive(false);
        CtrlCtrl.Instance.ToggleShootCtrler(true);
        Time.timeScale = 1f;
    }

}

