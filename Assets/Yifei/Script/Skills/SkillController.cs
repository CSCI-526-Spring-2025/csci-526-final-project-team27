using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 技能槽位数据结构：每个槽位包括技能预制体与其释放模式
/// </summary>
[System.Serializable]
public class SkillSlot
{
    public GameObject skillPrefab;     // 技能预制体（要求预制体上有 Skill 脚本）
    public SkillReleaseType releaseType; // 释放模式
}

/// <summary>
/// 技能释放控制器（挂载到主控角色）：
/// - 支持三个技能槽，可通过数字键（1~3）绑定；
/// - 不同技能槽支持直接释放、点地释放、点击单位释放三种模式；
/// - 点地和点击单位模式使用一个跟随鼠标的准星（cursorPrefab）
/// - 提供接口 ReplaceSkill 替换技能
/// </summary>
public class SkillController : MonoBehaviour
{
    [Header("General Settings")]
    public Transform skillFirePoint;         // 技能发射/释放点（通常在玩家身上）
    public Camera mainCamera;                // 主摄像机

    [Header("Cursor Settings")]
    public GameObject cursorPrefab;          // 用于目标/方向选择的准星预制体

    [Header("Skill Slots")]
    [Tooltip("三个技能槽位，可分别指定技能预制体和释放模式")]
    public SkillSlot[] skillSlots = new SkillSlot[3];
    public UnityEngine.UI.Image[] hpFills;

    // 内部变量
    private GameObject cursorInstance;
    private int currentSkillSlotIndex = -1;      // 当前正在等待目标/方向选择的技能槽索引
    private bool isTargetingMode = false;         // 是否处于目标/方向选择状态

    private GameObject[] skillInstances = new GameObject[3];        // 实例化的技能对象

    private ShootingController shootingController;

    private void Awake()
    {
        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i].skillPrefab != null)
            {
                GameObject skillInstance = Instantiate(skillSlots[i].skillPrefab, skillFirePoint.position, Quaternion.identity, transform);
                skillInstances[i] = skillInstance;
                Debug.Log("实例化" + skillInstances[i].name);
            }
        }
        shootingController = GetComponentInParent<ShootingController>();
    }

    void Update()
    {
        CoolDownImagesTick();

        // 监听数字键1~3对应技能槽的输入
        if (!isTargetingMode)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                ProcessSkillKey(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                ProcessSkillKey(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                ProcessSkillKey(2);
        }

        // 如果当前处于目标/方向选择状态，则更新准星位置
        if (isTargetingMode)
        {
            UpdateCursorPosition();

            // 当玩家点击左键时，确认释放
            if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
            {
                Vector2 targetPos = GetMouseWorldPosition();
                // 根据当前技能槽的释放模式决定：若为 Ground 则传入位置，若为 Target 则尝试检测点击到的对象
                SkillReleaseType type = skillSlots[currentSkillSlotIndex].releaseType;
                if (type == SkillReleaseType.Ground)
                {
                    UseSkill(currentSkillSlotIndex, targetPos);
                }
                else if (type == SkillReleaseType.Target)
                {
                    // 尝试射线检测点击的对象（比如敌人）
                    GameObject targetObj = GetTargetUnderMouse();
                    if (targetObj != null)
                    {
                        UseSkill(currentSkillSlotIndex, targetObj);
                    }
                    else
                    {
                        // 如果没有点击到单位，可以选择不释放或提示
                        Debug.Log("未点击到有效目标，取消技能释放。");
                        if (shootingController != null)
                        {
                            shootingController.LockShoot(false);
                        }
                    }
                }
                EndTargetingMode();
            }
            // 点击其他技能时切换到新技能
            else if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3))
            {
                EndTargetingMode();
                if (shootingController != null)
                {
                    shootingController.LockShoot(false);
                }
                if (Input.GetKeyDown(KeyCode.Alpha1))
                    ProcessSkillKey(0);
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                    ProcessSkillKey(1);
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                    ProcessSkillKey(2);
            }
        }
    }

    /// <summary>
    /// 查询指定技能槽是否在冷却
    /// </summary>
    /// <param name="slotIndex">技能槽索引（0～2）</param>
    /// <returns>如果在冷却返回 true，否则返回 false</returns>
    public bool IsSkillOnCooldown(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= skillSlots.Length)
            return false; // 无效索引，返回 false

        Skill skill = skillInstances[slotIndex].GetComponent<Skill>();
        if (skill != null)
        {
            return skill.IsOnCooldown(); // 调用 IsOnCooldown 方法
        }
        return false; // 如果没有技能，返回 false
    }

    /// <summary>
    /// 处理技能键输入，根据技能槽的释放模式选择处理方式
    /// </summary>
    /// <param name="slotIndex">技能槽索引（0～2）</param>
    private void ProcessSkillKey(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= skillSlots.Length)
            return;
        SkillSlot slot = skillSlots[slotIndex];
        if (slot.skillPrefab == null)
            return; // 该槽位未配置技能

        // 根据释放模式分流
        if (slot.releaseType == SkillReleaseType.Direct)
        {
            // 直接释放：使用当前鼠标方向作为释放方向
            Vector2 aimDir = (GetMouseWorldPosition() - (Vector2)skillFirePoint.position).normalized;
            UseSkill(slotIndex, aimDir);
        }
        else if (slot.releaseType == SkillReleaseType.Ground ||
                 slot.releaseType == SkillReleaseType.Target)
        {
            // 进入目标/方向选择模式
            currentSkillSlotIndex = slotIndex;
            isTargetingMode = true;
            CreateCursor();
            if (shootingController != null)
            {
                shootingController.LockShoot(true);
            }
        }
    }

    // 直接解锁射击似乎无法在技能释放前保持射击锁定
    private IEnumerator UnlockShootAfterFrame()
    {
        yield return null; // 等待一帧
        if (shootingController != null)
        {
            shootingController.LockShoot(false);
        }
    }

    /// <summary>
    /// 根据方向释放技能（用于直接释放模式）
    /// </summary>
    /// <param name="slotIndex">技能槽索引</param>
    /// <param name="direction">释放方向</param>
    private void UseSkill(int slotIndex, Vector2 direction)
    {
        SkillSlot slot = skillSlots[slotIndex];
        //GameObject skillInstance = Instantiate(slot.skillPrefab, skillFirePoint.position, Quaternion.identity);
        GameObject skillInstance = skillInstances[slotIndex];
        // 使技能朝向释放方向
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        skillInstance.transform.rotation = Quaternion.Euler(0, 0, angle);
        // 调用 Skill 脚本的 Initialize 方法（假设技能预制体上有 Skill 组件）
        Skill skillScript = skillInstance.GetComponent<Skill>();
        if (skillScript != null)
        {
            skillScript.TryInitialize(direction);
        }
        // 等待一帧后解锁射击
        StartCoroutine(UnlockShootAfterFrame());
    }

    /// <summary>
    /// 根据目标对象释放技能（用于点击单位释放模式）
    /// </summary>
    /// <param name="slotIndex">技能槽索引</param>
    /// <param name="target">目标 GameObject</param>
    private void UseSkill(int slotIndex, GameObject target)
    {
        SkillSlot slot = skillSlots[slotIndex];

        //GameObject skillInstance = Instantiate(slot.skillPrefab, skillFirePoint.position, Quaternion.identity);

        GameObject skillInstance = skillInstances[slotIndex];
        // 使技能朝向目标（这里根据目标位置计算方向）
        Vector2 dir = ((Vector2)target.transform.position - (Vector2)skillFirePoint.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        skillInstance.transform.rotation = Quaternion.Euler(0, 0, angle);
        // 调用 Skill 脚本的 Initialize 方法（假设 Skill 组件支持重载 Initialize(GameObject target)）
        Skill skillScript = skillInstance.GetComponent<Skill>();
        if (skillScript != null)
        {
            skillScript.TryInitialize(target);
        }
        StartCoroutine(UnlockShootAfterFrame());
    }

    /// <summary>
    /// 创建准星，用于目标/方向选择模式
    /// </summary>
    private void CreateCursor()
    {
        if (cursorInstance == null && cursorPrefab != null)
        {
            cursorInstance = Instantiate(cursorPrefab);
        }
    }

    /// <summary>
    /// 更新准星位置，使其跟随鼠标
    /// </summary>
    private void UpdateCursorPosition()
    {
        if (cursorInstance != null)
        {
            Vector2 pos = GetMouseWorldPosition();
            cursorInstance.transform.position = pos;
        }
    }

    /// <summary>
    /// 结束目标/方向选择模式，销毁准星
    /// </summary>
    private void EndTargetingMode()
    {
        isTargetingMode = false;
        currentSkillSlotIndex = -1;
        if (cursorInstance != null)
        {
            Destroy(cursorInstance);
            cursorInstance = null;
        }
    }

    /// <summary>
    /// 获取鼠标在世界坐标中的位置（z设为0）
    /// </summary>
    /// <returns>鼠标世界坐标</returns>
    private Vector2 GetMouseWorldPosition()
    {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        return mousePos;
    }

    /// <summary>
    /// 射线检测鼠标位置下是否有目标单位，返回第一个检测到的有效对象
    /// </summary>
    private GameObject GetTargetUnderMouse()
    {
        Vector2 mousePos = GetMouseWorldPosition();
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        if (hit.collider != null)
        {
            // 这里可根据具体需求判断目标标签，比如 "Enemy"
            if (hit.collider.CompareTag("Enemy"))
                return hit.collider.gameObject;
        }
        return null;
    }

    /// <summary>
    /// 替换指定技能槽位的技能（可在游戏过程中调用）
    /// </summary>
    /// <param name="slotIndex">技能槽索引（0～2）</param>
    /// <param name="newSkillPrefab">新的技能预制体</param>
    /// <param name="newReleaseType">新的释放模式</param>
    public void ReplaceSkill(int slotIndex, GameObject newSkillPrefab, SkillReleaseType newReleaseType)
    {
        if (slotIndex < 0 || slotIndex >= skillSlots.Length)
            return;
        skillSlots[slotIndex].skillPrefab = newSkillPrefab;
        skillSlots[slotIndex].releaseType = newReleaseType;

    }

    /// <summary>
    /// 判断当前鼠标是否悬停在 UI 元素上（防止误触技能释放）
    /// </summary>
    private bool IsPointerOverUIObject()
    {
        //return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        return false;
    }

    void CoolDownImagesTick()
    {
        // for each skill slot, update the cooldown image
        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (hpFills[i] != null)
            {
                GameObject skillObject = skillInstances[i];
                Skill skill = skillObject.GetComponent<Skill>();

                string key = skill.GetType().Name;
                if (Skill.lastUsedTimeBySkill.ContainsKey(key))
                {
                    hpFills[i].fillAmount = Mathf.Clamp01(1 - (Time.time - Skill.lastUsedTimeBySkill[key]) / skill.cooldownTime);
                }
            }
        }
    }
}