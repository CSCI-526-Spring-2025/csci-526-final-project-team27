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
    private FirebaseDataUploader dataUploader;

    public static bool isLevelUp = false;

    public GameObject player;

    private void Start()
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

    public void ShowRewardSelection()
    {
        isLevelUp = true;
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
        
        // 记录buff选择
        if (dataUploader != null)
        {
            dataUploader.TrackBuffSelection(buff.buffName);
        }
        
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }
        Health playerHealth = player.GetComponent<Health>();

        if (playerHealth != null)
        {
            switch (buff.buffName)
            {   
                case "Player's Max HP +10":
                    float increaseAmount = 10f;
                    playerHealth.maxHealth += increaseAmount;
                    playerHealth.currentHealth += increaseAmount;

                    Debug.Log($"增加最大生命 {increaseAmount}，当前生命设为满血：{playerHealth.currentHealth}");

                    // 生成黄色提示
                    playerHealth.ShowFloatingText("Max HP ↑", Color.yellow, new Vector3(0.5f, 1f, 0));
                    break;
                case "Heals all allies for 25 HP":
                    playerHealth.Heal(25f);
                    GameObject[] teammatesHeal = GameObject.FindGameObjectsWithTag("Teammate");
                    foreach (GameObject teammate in teammatesHeal)
                    {
                        Health teammateHealth = teammate.GetComponent<Health>();
                        if (teammateHealth != null)
                        {
                            teammateHealth.Heal(25f);
                        }
                    }
                    break;
                case "Teammates' Speed +1":
                    float increaseSpeed = 1f;
                    // 给所有队友增加速度
                    GameObject[] teammatesSpeed = GameObject.FindGameObjectsWithTag("Teammate");
                    foreach (GameObject teammate in teammatesSpeed)
                    {
                        MeleeTeammate meleeMovement = teammate.GetComponent<MeleeTeammate>();

                        if (meleeMovement != null)
                        {
                            meleeMovement.moveSpeed += increaseSpeed;
                            continue;
                        }
                        RangedTeammate rangedMovement = teammate.GetComponent<RangedTeammate>();
                        if (rangedMovement != null)
                        {
                            rangedMovement.moveSpeed += increaseSpeed;
                            continue;
                        }
                    }
                    break;
                case "Teammates' Damage +5":
                    float increaseDamage = 5f;
                    GameObject[] teammates = GameObject.FindGameObjectsWithTag("Teammate");
                    foreach (GameObject teammate in teammates)
                    {
                        MeleeTeammate melee = teammate.GetComponent<MeleeTeammate>();
                        if (melee != null)
                        {
                            melee.damage += increaseDamage;
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
                                    teamBullet.damageAmount += increaseDamage;
                                    Debug.Log("Range teammate damage increased, new damage: " + teamBullet.damageAmount);
                                }
                            }
                            continue;
                        }
                    }
                    break;
                case "Teammates' Max HP +15":
                    GameObject[] teammatesHP = GameObject.FindGameObjectsWithTag("Teammate");
                    foreach (GameObject teammate in teammatesHP)
                    {
                        Health teammateHealth = teammate.GetComponent<Health>();
                        if (teammateHealth != null)
                        {
                            teammateHealth.maxHealth += 15;
                            teammateHealth.currentHealth += 15;
                            Debug.Log($"队友最大生命增加 {15}，当前最大生命：{teammateHealth.maxHealth}");
                        }
                    }
                    break;
                case "Player's healing power +5":
                    float increaseHeal = 5f;
                    ShootingController shootingController = player.GetComponent<ShootingController>();
                    if (shootingController != null && shootingController.bulletPrefab != null)
                    {
                        HealBallVond playerBullet = shootingController.bulletPrefab.GetComponent<HealBallVond>();
                        if (playerBullet != null)
                        {
                            playerBullet.healAmount += increaseHeal;
                            Debug.Log("Player's healing power increased, new healing amount: " + playerBullet.healAmount);
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
        isLevelUp = false;
    }

}

