using UnityEngine;

using System.Collections;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class TutorialSkill : MonoBehaviour
{
    // 子对象引用
    private GameObject SkillOneArrow;
    
    private GameObject SkillTwoArrow;
    private GameObject SkillThreeArrow;
    private GameObject SecondSkillHint;
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

    GameObject alert1 ;
    GameObject alert2 ;
    GameObject alert3;
    private Dictionary<GameObject, Coroutine> blinkingCoroutines = new Dictionary<GameObject, Coroutine>();
    
    // 教程流程状态机
    private enum TutorialState
    {
        EnterRoomState,
        SkillOneState,
        SkillTwoState,
        
        Completed
    }
    private TutorialState currentState;

    void Start()
    {
        alert1 = GameObject.Find("PlayerCanvas/Panel/Skill1/Alert1");
        alert2 = GameObject.Find("PlayerCanvas/Panel/Skill2/Alert2");
        alert3 = GameObject.Find("PlayerCanvas/Panel/Skill3/Alert3");

        SkillOneArrow = transform.Find("1Arrow").gameObject;
        SkillTwoArrow = transform.Find("2Arrow").gameObject;
        SkillThreeArrow = transform.Find("3Arrow").gameObject;
        SecondSkillHint = transform.Find("Image_canva").gameObject;
        SecondSkillHint.SetActive(false);
        SkillcontrollerUI.Instance.HideSkillUI();
        SkillOneArrow.SetActive(false);
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
                e.gameObject.SetActive(false);
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
        
        // 禁用玩家射击和技能
        player = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<ShootingController>().enabled = false;
        player.GetComponent<ShootingController>().ToggleActive(false);
        player.GetComponent<SkillController>().enabled = false;
        
        // 获取 UI Text 组件（假设 Canvas 下有 Text 子对象）
        tutorialText = transform.Find("Canvas/Text").GetComponent<TextMeshProUGUI>();

        currentState = TutorialState.EnterRoomState;
        tutorialText.text = "Use your skills to help your teammates\n[Space] to continue";
        //冷却后禁用移动
        StartCoroutine(ProcessCoolDown(0.5f));  
        player.GetComponent<PlayerMovement>().enabled = true;
        player.GetComponent<ShootingController>().enabled = true;
    }
    
    // 显示"认识你的队友"提示
    void LearnSkillOne()
    {
        tutorialText.text = "Press 1 to heal all teammates";
    }
    
    // 显示"认识你的敌人"提示
    void LearnSkillTwo()
    {
        tutorialText.text = "Press 2 and left-click to freeze the enemy.";
        SecondSkillHint.SetActive(true);
    }

    // // 显示"战斗"提示
    // void LearnSkillThree()
    // {
    //     tutorialText.text = "Press 3 and left-click to freeze the enemy.";
    // }
    

    
    

    void Update()
    {
        // 檢查目前所有還活著的敵人
        GameObject[] remainingEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        // 敵人死光了才觸發下一波
        if (remainingEnemies.Length == 0 && currentState != TutorialState.EnterRoomState)
        {
            if (waveIndex == 1)
            {
                foreach (Transform e in TwoAttackGroup.transform)
                    e.gameObject.SetActive(true);
                waveIndex = 2;
            }
            else if (waveIndex == 2)
            {
                
                waveIndex = 3; // 避免重複觸發
            }
        }

        // 教學流程對應按鍵
        switch (currentState)
        {
            case TutorialState.EnterRoomState:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    // 激活第一波怪物
                    foreach (Transform e in firstAttackGroup.transform)
                    {
                        e.gameObject.SetActive(true);
                    }
                    // SkillOneArrow.SetActive(true);
                    currentState = TutorialState.SkillOneState;
                    StartCoroutine(ProcessCoolDown(0.5f));
                    LearnSkillOne();
                    SkillcontrollerUI.Instance.ShowSkillUI();
                    // 启用技能
                    player.GetComponent<ShootingController>().enabled = true;
                    player.GetComponent<ShootingController>().ToggleActive(true);
                    player.GetComponent<SkillController>().enabled = true;
                    StartBlinking(alert1);
                }
                break;

            case TutorialState.SkillOneState:
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    StopBlinking(alert1);
                    Destroy(SkillOneArrow);
                    // SkillTwoArrow.SetActive(true);
                    currentState = TutorialState.SkillTwoState;
                    StartCoroutine(ProcessCoolDown(0.5f));
                    LearnSkillTwo();
                    StartBlinking(alert2);
                }
                break;

            

            case TutorialState.SkillTwoState:
                bool isCalled = false;
                if (Input.GetKeyDown(KeyCode.Alpha2) )
                {
                    tutorialText.text = "Left-click to set a trap zone";
                    StartCoroutine(ClearTutorialTextAfterDelay(3));
                    currentState = TutorialState.Completed;
                    StartCoroutine(ProcessCoolDown(0.5f));
                    SecondSkillHint.SetActive(false);
                }
                // if (Input.GetKeyDown(KeyCode.Alpha2) && !isCalled)
                // {
                //     StopBlinking(alert2);
                //     isCalled = true;
                //     Destroy(SkillThreeArrow);
                //     tutorialText.text = "Left-click to set a trap zone";
                // }
                // if (player.GetComponent<SkillController>().IsSkillOnCooldown(1) && isCalled){
                //     tutorialText.text = "Clear the room";
                //     currentState = TutorialState.Completed;
                //     StartCoroutine(ProcessCoolDown(0.5f));
                //     break;
                // }
                break;

            case TutorialState.Completed:
                // 检查是否所有敌人都被击败
                if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
                {
                    //tutorialText.text = "";
                    Debug.Log("enter completed");
                    UnlockRoom();
                }
                
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

    /// <summary>
    /// 开始闪烁指定的目标物体
    /// 要求该目标物体上挂有 Image 组件
    /// </summary>
    /// <param name="target">用户选择的目标物体</param>
    public void StartBlinking(GameObject target)
    {
        if (target == null)
        {
            Debug.LogWarning("传入的目标物体为 null");
            return;
        }

        // 如果该物体已经在闪烁，则不重复启动
        if (blinkingCoroutines.ContainsKey(target))
        {
            return;
        }

        Image img = target.GetComponent<Image>();
        if (img == null)
        {
            Debug.LogWarning("目标物体没有 Image 组件！");
            return;
        }

        // 开启闪烁协程，并存储到字典中
        Coroutine coroutine = StartCoroutine(BlinkCoroutine(img));
        blinkingCoroutines.Add(target, coroutine);
    }

    /// <summary>
    /// 停止指定目标物体的闪烁效果，并将其透明度重置为不透明
    /// </summary>
    /// <param name="target">用户选择的目标物体</param>
    public void StopBlinking(GameObject target)
    {
        if (target == null)
        {
            Debug.LogWarning("传入的目标物体为 null");
            return;
        }

        // 如果该物体正在闪烁，则停止协程并从字典中移除
        if (blinkingCoroutines.ContainsKey(target))
        {
            StopCoroutine(blinkingCoroutines[target]);
            blinkingCoroutines.Remove(target);

            // 将目标物体的透明度重置为 1（完全不透明）
            Image img = target.GetComponent<Image>();
            if (img != null)
            {
                Color c = img.color;
                c.a = 1f;
                img.color = c;
            }
        }
    }

    /// <summary>
    /// 闪烁协程：每帧根据 Mathf.PingPong 计算透明度值，并应用到目标 Image 上
    /// </summary>
    /// <param name="img">目标物体的 Image 组件</param>
    /// <returns></returns>
    private IEnumerator BlinkCoroutine(Image img)
    {
        while (true)
        {
            // 这里 Time.time * 2f 可调整闪烁速度
            float alpha = Mathf.PingPong(Time.time * 2f, 1f);
            Color c = img.color;
            c.a = alpha;
            img.color = c;
            yield return null;
        }
    }
    
}
