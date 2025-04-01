using UnityEngine;

using System.Collections;
using UnityEngine.UI;
using TMPro;

public class TutorialSkill : MonoBehaviour
{
    // 子对象引用
    private GameObject SkillOneArrow;
    
    private GameObject SkillTwoArrow;
    private GameObject SkillThreeArrow;

    private GameObject doorArrow;
    private GameObject firstAttackGroup;
    private GameObject TwoAttackGroup;
    private GameObject ThirdAttackGroup;
    private GameObject[] teammates;

    private GameObject[] doors;

    private GameObject player;

    private GameObject enemy;
    private int waveIndex = 1;
    // UI文本组件（假设在 Canvas/Text 路径下）
    private TextMeshProUGUI tutorialText;
    
    // 教程流程状态机
    private enum TutorialState
    {
        SkillOneState,
        SkillTwoState,
        SkillThreeState,
        Completed
    }
    private TutorialState currentState;

    void Start()
    {
        
        SkillOneArrow = transform.Find("1Arrow").gameObject;
        SkillTwoArrow = transform.Find("2Arrow").gameObject;
        SkillThreeArrow = transform.Find("3Arrow").gameObject;
        
        SkillcontrollerUI.Instance.ShowSkillUI();
        SkillOneArrow.SetActive(true);
        SkillTwoArrow.SetActive(false);
        SkillThreeArrow.SetActive(false);
        doorArrow = transform.Find("DoorArrowIndicator").gameObject;
        doorArrow.SetActive(false);

        doors = new GameObject[2];
        doors[0] = transform.Find("Door_Left").gameObject;
        doors[1] = transform.Find("Door_Right").gameObject;
        foreach (GameObject door in doors)
        {
            door.SetActive(false);
        }
        firstAttackGroup = transform.Find("First_attack").gameObject;
        foreach (Transform e in firstAttackGroup.transform)
        {
                e.gameObject.SetActive(true);
                
        }
        ThirdAttackGroup = transform.Find("Third").gameObject;
        foreach (Transform e in ThirdAttackGroup.transform)
        {
                e.gameObject.SetActive(false);
                
        }
        TwoAttackGroup = transform.Find("Second_attack").gameObject;
        foreach (Transform e in TwoAttackGroup.transform)
        {
                e.gameObject.SetActive(false);
                
        }
        
        
        // 禁用玩家射击
        player = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<ShootingController>().enabled = true;
        player.GetComponent<ShootingController>().ToggleActive(true);
        // 禁用技能面板
        player.GetComponent<SkillController>().enabled = true;
        
        // 获取 UI Text 组件（假设 Canvas 下有 Text 子对象）
        tutorialText = transform.Find("Canvas/Text").GetComponent<TextMeshProUGUI>();

        
        currentState = TutorialState.SkillOneState;
        //冷却后禁用移动
        StartCoroutine(ProcessCoolDown(0.5f));  
        player.GetComponent<PlayerMovement>().enabled = true;
        player.GetComponent<ShootingController>().enabled = true;
        LearnSkillOne();
    }
    
    // 显示“认识你的队友”提示
    void LearnSkillOne()
    {
        tutorialText.text = "Press 1 to heal all teammate simultanously";
    }
    
    // 显示“认识你的敌人”提示
    void LearnSkillTwo()
    {
        tutorialText.text = "Press 2 to Defense UPPPPP";
    }

    // 显示“战斗”提示
    void LearnSkillThree()
    {
        tutorialText.text = "Press 3 and Right-click to freeze the enemy.";
    }
    

    
    

    void Update()
    {
        // 檢查目前所有還活著的敵人
        GameObject[] remainingEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        // 敵人死光了才觸發下一波
        if (remainingEnemies.Length == 0)
        {
            if (waveIndex == 1)
            {
                foreach (Transform e in TwoAttackGroup.transform)
                    e.gameObject.SetActive(true);
                waveIndex = 2;
            }
            else if (waveIndex == 2)
            {
                foreach (Transform e in ThirdAttackGroup.transform)
                    e.gameObject.SetActive(true);
                waveIndex = 3;
            }
            else if (waveIndex == 3)
            {
                UnlockRoom();
                //tutorialText.text = "Start fighting!";
                waveIndex = 4; // 避免重複觸發
            }
        }

        // 教學流程對應按鍵
        switch (currentState)
        {
            case TutorialState.SkillOneState:
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    Destroy(SkillOneArrow);
                    SkillTwoArrow.SetActive(true);
                    currentState = TutorialState.SkillTwoState;
                    StartCoroutine(ProcessCoolDown(0.5f));
                    LearnSkillTwo();
                }
                break;

            case TutorialState.SkillTwoState:
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    Destroy(SkillTwoArrow);
                    SkillThreeArrow.SetActive(true);
                    currentState = TutorialState.SkillThreeState;
                    StartCoroutine(ProcessCoolDown(0.5f));
                    LearnSkillThree();
                }
                break;

            case TutorialState.SkillThreeState:
                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    Destroy(SkillThreeArrow);
                    tutorialText.text = "Right-click to set a trap zone.";
                    currentState = TutorialState.Completed;
                    StartCoroutine(ClearTutorialTextAfterDelay(5f));
                    StartCoroutine(ProcessCoolDown(0.5f));
                }
                break;

            case TutorialState.Completed:
                
                
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
    private IEnumerator ClearTutorialTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        tutorialText.text = "";
    }
    
}
