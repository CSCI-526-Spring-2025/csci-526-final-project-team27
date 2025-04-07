 using UnityEngine;

using System.Collections;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
    private bool allowFirstWaveRespawn = false;

    private GameObject[] doors;

    private GameObject player;
    private SkillController playerSkillController;

    // UI文本组件
    private TextMeshProUGUI tutorialText;

    GameObject alert1;
    GameObject alert2;
    GameObject alert3;
    private Dictionary<GameObject, Coroutine> blinkingCoroutines = new Dictionary<GameObject, Coroutine>();
    
    // 教程流程状态机
    private enum TutorialState
    {
        EnterRoomState,       // 初始状态：介绍教程
        SkillOneState,        // 学习技能1
        SkillTwoState,        // 学习技能2
        SkillTwoUsedState,    // 已按下2键，等待玩家左键释放
        ClearRoomState        // 清理房间状态
    }
    private TutorialState currentState;

    // 用于跟踪技能2是否已被释放
    private bool isSkill2Released = false;
    
    // 保存敌人的初始信息
    [System.Serializable]
    public class EnemyInfo
    {
        public GameObject enemyPrefab;
        public Vector3 position;
        public Quaternion rotation;
    }
    
    // 保存第一波敌人的预制体和位置信息
    private List<EnemyInfo> firstWaveEnemies = new List<EnemyInfo>();
    
    // 敌人刷新计时器
    private float enemyRespawnTimer = 0f;
    private float respawnInterval = 5f; // 每5秒检查一次是否需要刷新敌人
    
    // 房间已解锁标志
    private bool isRoomUnlocked = false;
    
    // 在Start之前复制原始敌人预制体
    private void Awake()
    {
        // 初始化敌人组
        firstAttackGroup = transform.Find("First_attack").gameObject;
        
        if (firstAttackGroup != null)
        {
            // 为每个敌人创建副本
            foreach (Transform enemy in firstAttackGroup.transform)
            {
                if (enemy != null && enemy.gameObject != null)
                {
                    // 创建敌人信息对象保存预制体和位置
                    EnemyInfo info = new EnemyInfo
                    {
                        // 对敌人游戏对象进行深复制
                        enemyPrefab = Instantiate(enemy.gameObject),
                        position = enemy.position,
                        rotation = enemy.rotation
                    };
                    
                    // 暂时禁用并隐藏预制体
                    info.enemyPrefab.SetActive(false);
                    info.enemyPrefab.transform.SetParent(null);
                    info.enemyPrefab.hideFlags = HideFlags.HideInHierarchy;
                    
                    // 添加到列表
                    firstWaveEnemies.Add(info);
                    
                    // 确保场景中的原始敌人是禁用状态
                    enemy.gameObject.SetActive(false);
                }
            }
            
            Debug.Log($"已保存 {firstWaveEnemies.Count} 个敌人的预制体和位置信息");
        }
        else
        {
            Debug.LogError("找不到First_attack敌人组!");
        }
    }

    void Start()
    {
        // 初始化UI元素引用
        alert1 = GameObject.Find("PlayerCanvas/Panel/Skill1/Alert1");
        alert2 = GameObject.Find("PlayerCanvas/Panel/Skill2/Alert2");
        alert3 = GameObject.Find("PlayerCanvas/Panel/Skill3/Alert3");

        // 初始化箭头指示器
        SkillOneArrow = transform.Find("1Arrow").gameObject;
        SkillTwoArrow = transform.Find("2Arrow").gameObject;
        SkillThreeArrow = transform.Find("3Arrow").gameObject;
        SecondSkillHint = transform.Find("Image_canva").gameObject;
        
        // 初始状态设置
        SecondSkillHint.SetActive(false);
        SkillcontrollerUI.Instance.HideSkillUI();
        SkillOneArrow.SetActive(false);
        SkillTwoArrow.SetActive(false);
        SkillThreeArrow.SetActive(false);
        
        // 初始化门和指示器
        doorArrow = transform.Find("DoorArrowIndicator").gameObject;
        doorArrow.SetActive(false);

        doors = new GameObject[2];
        doors[0] = transform.Find("Door_Left").gameObject;
        doors[1] = transform.Find("Door_Right").gameObject;
        foreach (GameObject door in doors)
        {
            door.SetActive(false);
        }
        
        TwoAttackGroup = transform.Find("Second_attack").gameObject;
        foreach (Transform e in TwoAttackGroup.transform)
        {
            e.gameObject.SetActive(false);
        }
        
        // 禁用第三波敌人（不需要）
        ThirdAttackGroup = transform.Find("Third").gameObject;
        if (ThirdAttackGroup != null)
        {
            ThirdAttackGroup.SetActive(false);
        }
        
        // 初始化玩家引用
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // 禁用玩家射击和技能
            ShootingController shootingController = player.GetComponent<ShootingController>();
            if (shootingController != null)
            {
                shootingController.enabled = false;
                shootingController.ToggleActive(false);
            }
            
            playerSkillController = player.GetComponent<SkillController>();
            if (playerSkillController != null)
            {
                playerSkillController.enabled = false;
            }
            
            // 启用玩家移动
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.enabled = true;
            }
        }
        else
        {
            Debug.LogError("找不到Player标签的游戏对象!");
        }
        
        // 获取UI文本组件
        Transform canvasTransform = transform.Find("Canvas/Text");
        if (canvasTransform != null)
        {
            tutorialText = canvasTransform.GetComponent<TextMeshProUGUI>();
            tutorialText.text = "Use your skills to help your teammates\n[Space] to continue";
        }
        else
        {
            Debug.LogError("找不到教程文本UI组件!");
        }

        // 设置初始状态
        currentState = TutorialState.EnterRoomState;
    }
    
    // 显示"认识你的队友"提示
    void LearnSkillOne()
    {
        tutorialText.text = "Press 1 to heal all teammates";
    }
    
    // 显示"认识你的敌人"提示
    void LearnSkillTwo()
    {
        tutorialText.text = "Press 2 and left-click to freeze the enemy";
        SecondSkillHint.SetActive(true);
    }

    // 显示释放2技能的提示
    void LearnSkillTwoRelease()
    {
        tutorialText.text = "Left-click to set a trap zone";
    }

    // 显示清理房间的提示
    void ShowClearRoomText()
    {
        tutorialText.text = "Clear the room and the door will open";
    }

    void Update()
    {
        // 在教程未完成前，处理敌人刷新逻辑
        if (allowFirstWaveRespawn && currentState >= TutorialState.SkillOneState && currentState < TutorialState.ClearRoomState)
        {
            HandleEnemyRespawn();
        }

        // 检查是否成功释放技能2
        if (currentState == TutorialState.SkillTwoUsedState && !isSkill2Released)
        {
            // 通过检查技能2是否进入冷却来判断技能是否已经释放
            if (playerSkillController != null && playerSkillController.IsSkillOnCooldown(1))
            {
                isSkill2Released = true;
                StopBlinking(alert2);
                SecondSkillHint.SetActive(false);
                
                Debug.Log("技能2释放成功，进入清理房间阶段");
                
                // 激活第二波敌人
                ActivateSecondWaveEnemies();
                
                // 显示清理房间提示
                ShowClearRoomText();
                
                // 转换到清理房间状态
                currentState = TutorialState.ClearRoomState;
            }
            else
            {
                // 持续显示左键释放提示
                LearnSkillTwoRelease();
            }
        }

        // 如果处于清理房间状态且房间未解锁，检查是否所有敌人已消灭
        if (currentState == TutorialState.ClearRoomState && !isRoomUnlocked)
        {
            GameObject[] remainingEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            if (remainingEnemies.Length == 0)
            {
                UnlockRoom();
                isRoomUnlocked = true;
            }
        }

        // 按键处理逻辑
        HandleKeyInput();
    }
    
    // 处理按键输入
    private void HandleKeyInput()
    {
        switch (currentState)
        {
            case TutorialState.EnterRoomState:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    // 激活第一波怪物
                    //ActivateFirstWaveEnemies();
                    SetTeammatesToMissingHealth();
                    
                    // 转换状态
                    currentState = TutorialState.SkillOneState;
                    
                    // 更新UI
                    LearnSkillOne();
                    SkillcontrollerUI.Instance.ShowSkillUI();
                    
                    // 启用技能
                    if (player != null)
                    {
                        ShootingController shootingController = player.GetComponent<ShootingController>();
                        if (shootingController != null)
                        {
                            shootingController.enabled = true;
                            shootingController.ToggleActive(true);
                        }
                        
                        if (playerSkillController != null)
                        {
                            playerSkillController.enabled = true;
                        }
                    }
                    
                    // 开始闪烁技能1提示
                    StartBlinking(alert1);
                }
                break;

            case TutorialState.SkillOneState:
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    
                    allowFirstWaveRespawn = true;
                    StopBlinking(alert1);
                    if (SkillOneArrow != null)
                    {
                        Destroy(SkillOneArrow);
                    }
                    
                    // 转换状态
                    currentState = TutorialState.SkillTwoState;
                    
                    // 更新UI
                    LearnSkillTwo();
                    ActivateFirstWaveEnemies();
                    // 开始闪烁技能2提示
                    StartBlinking(alert2);
                }
                break;

            case TutorialState.SkillTwoState:
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    // 玩家按下了2键，但尚未左键释放技能
                    LearnSkillTwoRelease();
                    currentState = TutorialState.SkillTwoUsedState;
                }
                break;
        }
    }

    // 激活第一波敌人
    private void ActivateFirstWaveEnemies()
    {
        Debug.Log("开始激活第一波敌人...");
        
        // 首先生成新的敌人实例
        SpawnNewWaveOfEnemies();
    }
    
    // 激活第二波敌人
    private void ActivateSecondWaveEnemies()
    {
        if (TwoAttackGroup != null)
        {
            int count = 0;
            foreach (Transform enemy in TwoAttackGroup.transform)
            {
                if (enemy != null)
                {
                    enemy.gameObject.SetActive(true);
                    count++;
                    Debug.Log("激活第二波敌人: " + enemy.name);
                }
            }
            Debug.Log($"共激活了 {count} 个第二波敌人");
        }
        else
        {
            Debug.LogError("第二波敌人组不存在!");
        }
    }

    // 处理敌人刷新逻辑
    private void HandleEnemyRespawn()
    {
        // 只有在已经开始学习技能1但尚未进入清理房间状态时才刷新敌人
        if (currentState >= TutorialState.SkillOneState && currentState < TutorialState.ClearRoomState)
        {
            enemyRespawnTimer += Time.deltaTime;
            
            // 到达刷新时间间隔时检查敌人数量
            if (enemyRespawnTimer >= respawnInterval)
            {
                enemyRespawnTimer = 0f;
                
                // 检查当前场景中的敌人数量
                GameObject[] currentEnemies = GameObject.FindGameObjectsWithTag("Enemy");
                Debug.Log("当前敌人数量: " + currentEnemies.Length);
                
                // 如果敌人数量为0，则生成新的敌人
                if (currentEnemies.Length == 0)
                {
                    Debug.Log("开始生成新一波敌人...");
                    SpawnNewWaveOfEnemies();
                }
            }
        }
    }
    
    // 生成新一波敌人
    private void SpawnNewWaveOfEnemies()
    {
        if (firstAttackGroup == null || firstWaveEnemies.Count == 0)
        {
            Debug.LogWarning("没有可用的敌人信息来生成新敌人!");
            return;
        }
        
        int spawnedCount = 0;
        
        // 基于保存的敌人信息生成新敌人
        foreach (EnemyInfo info in firstWaveEnemies)
        {
            if (info.enemyPrefab != null)
            {
                // 复制敌人预制体并设置位置和旋转
                GameObject newEnemy = Instantiate(info.enemyPrefab, info.position, info.rotation);
                
                // 确保新敌人激活并属于第一波敌人组
                newEnemy.SetActive(true);
                newEnemy.transform.SetParent(firstAttackGroup.transform);
                
                // 把新敌人的隐藏标志清除
                newEnemy.hideFlags = HideFlags.None;
                
                spawnedCount++;
                Debug.Log($"成功生成敌人: {newEnemy.name}");
            }
        }
        
        Debug.Log($"此次共生成了 {spawnedCount} 个敌人");
    }

    // 设置队友的血量为缺失状态
    void SetTeammatesToMissingHealth()
    {
        //debug print
        Debug.Log("SetTeammatesToMissingHealth");
        GameObject[] allTeammates = GameObject.FindGameObjectsWithTag("Teammate");
        foreach (GameObject mate in allTeammates)
        {
            Health health = mate.GetComponent<Health_BC>();
            if (health != null)
            {
                //Debug.Log("SetTeammatesToMissingHealth: " + mate.name);
                health.TakeDamage(20);
                //Debug.Log("SetTeammatesToMissingHealth: " + health.currentHealth);
            }
        }
    }
    
    // 在场景关闭时清理资源
    private void OnDestroy()
    {
        // 清理保存的敌人预制体
        foreach (EnemyInfo info in firstWaveEnemies)
        {
            if (info.enemyPrefab != null)
            {
                Destroy(info.enemyPrefab);
            }
        }
        firstWaveEnemies.Clear();
    }
    
    // 解锁房间
    private void UnlockRoom()
    {
        if (doorArrow != null)
        {
            doorArrow.SetActive(true);
        }
        
        if (doors.Length > 1 && doors[1] != null)
        {
            doors[1].SetActive(true);
        }
        
        if (tutorialText != null)
        {
            tutorialText.text = "";
        }
        
        Debug.Log("房间已解锁!");
    }

    // 流程转换冷却
    private IEnumerator ProcessCoolDown(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    /// <summary>
    /// 开始闪烁指定的目标物体
    /// 要求该目标物体上挂有 Image 组件
    /// </summary>
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
