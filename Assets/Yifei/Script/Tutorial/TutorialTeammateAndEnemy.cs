using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class RoomTutorial : MonoBehaviour
{
    // 子对象引用
    private GameObject meetTeammateArrow;
  
    private GameObject meetEnemyArrow;

    private GameObject doorArrow;

    private GameObject[] teammates;

    private GameObject[] doors;

    private GameObject player;

    private GameObject enemy;
    
    // UI文本组件（假设在 Canvas/Text 路径下）
    private TextMeshProUGUI tutorialText;
    
    // 教程流程状态机
    private enum TutorialState
    {
        MeetTeammates,
        MeetEnemy,
        Fight,
        HealTeammate,
        Completed
    }
    private TutorialState currentState;

    void Start()
    {
        meetTeammateArrow = transform.Find("MeetTeammate").gameObject;
        meetEnemyArrow = transform.Find("MeetEnemy").gameObject;
        doorArrow = transform.Find("DoorArrowIndicator").gameObject;
        doorArrow.SetActive(false);

        doors = new GameObject[2];
        doors[0] = transform.Find("Door_Left").gameObject;
        doors[1] = transform.Find("Door_Right").gameObject;
        foreach (GameObject door in doors)
        {
            door.SetActive(false);
        }
        //避免索敌
        enemy = transform.Find("Enemy").gameObject;
        enemy.tag = "Untagged";
        //避免索敌
        teammates = new GameObject[3];
        teammates[0] = transform.Find("MeleeTeammate_1").gameObject;
        teammates[1] = transform.Find("MeleeTeammate_3").gameObject;
        teammates[2] = transform.Find("RangeTeammate_2").gameObject;
        foreach (GameObject teammate in teammates)
        {
            teammate.tag = "Untagged";
        }
        // 禁用玩家射击
        player = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<ShootingController>().enabled = true;
        player.GetComponent<ShootingController>().ToggleActive(true);
        // 禁用技能面板
        player.GetComponent<SkillController>().enabled = false;
        
        // 获取 UI Text 组件（假设 Canvas 下有 Text 子对象）
        tutorialText = transform.Find("Canvas/Text").GetComponent<TextMeshProUGUI>();

        // 初始状态：显示三个队友，隐藏敌人
        meetTeammateArrow.SetActive(true);
        meetEnemyArrow.SetActive(false);
        
        currentState = TutorialState.MeetTeammates;
        //冷却后禁用移动
        StartCoroutine(ProcessCoolDown(0.5f));  
        player.GetComponent<PlayerMovement>().enabled = true;
        ShowMeetTeammates();
    }
    
    // 显示“认识你的队友”提示
    void ShowMeetTeammates()
    {
        tutorialText.text = "Meet your teammates\n[Space] to continue";
    }
    
    // 显示“认识你的敌人”提示
    void ShowMeetEnemy()
    {
        tutorialText.text = "Meet your enemy\n[Space] to continue";
    }

    // 显示“战斗”提示
    void ShowFight()
    {
        tutorialText.text = "Teammates attack the enemy";
    }
    
    // 显示“治疗你的队友”提示
    void ShowHealTeammate()
    {
        tutorialText.text = "Shoot teammates to heal your teammate";
    }
    
    void Update()
    {
        switch (currentState)
        {
            case TutorialState.MeetTeammates:
                // 按空格跳过后，隐藏队友并进入下一个流程
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    // 隐藏队友箭头
                    Destroy(meetTeammateArrow);
                    // 切换到认识敌人流程，显示敌人
                    currentState = TutorialState.MeetEnemy;
                    
                    StartCoroutine(ProcessCoolDown(0.5f));
                    meetEnemyArrow.SetActive(true);
                    ShowMeetEnemy();
                }
                break;
                
            case TutorialState.MeetEnemy:
                // 按空格跳过后，隐藏敌人箭头，开始模拟战斗
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Destroy(meetEnemyArrow);
                    currentState = TutorialState.Fight;
                    StartCoroutine(ProcessCoolDown(0.5f));
                    ShowFight();
                    enemy.tag = "Enemy";
                    foreach (GameObject teammate in teammates)
                    {
                        teammate.tag = "Teammate";
                    }
                }
                break;
                
            case TutorialState.Fight:
                
                if (enemy == null){
                    currentState = TutorialState.HealTeammate;
                    StartCoroutine(ProcessCoolDown(0.5f));
                    ShowHealTeammate();
                    player.GetComponent<ShootingController>().enabled = true;
                    player.GetComponent<PlayerMovement>().enabled = true;
                }
                break;
                
                
            case TutorialState.HealTeammate:
                
                bool allTeammatesFullHealth = true;
                foreach (GameObject teammate in teammates)
                {
                    Health_BC health = teammate.GetComponent<Health_BC>();
                    if (health != null && health.currentHealth < health.maxHealth)
                    {
                        allTeammatesFullHealth = false;
                        break;
                    }
                }

                if (allTeammatesFullHealth)
                {
                    currentState = TutorialState.Completed;
                    StartCoroutine(ProcessCoolDown(0.5f));
                }
                break;
                
            case TutorialState.Completed:
                UnlockRoom();
                break;
        }
    }
    
    
    // 解锁房间（这里通过调试信息和清空提示文本表示）
    private void UnlockRoom()
    {
        doorArrow.SetActive(true);
        doors[1].SetActive(true);
        tutorialText.text = "";
        
        // 如有需要，这里可添加进一步解锁房间的逻辑（例如显示门、播放动画等）
    }

    // 流程转换冷却
    private IEnumerator ProcessCoolDown(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }
}
