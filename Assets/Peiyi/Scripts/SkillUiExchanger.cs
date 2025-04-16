using UnityEngine;
using System.Collections.Generic;
using System.Collections; 
using UnityEngine.UI;
using TMPro;


public class SkillUiExchanger : MonoBehaviour
{
    public Transform skillGridParent;
    public GameObject skillButtonPrefab;
    public GameObject glowEffectPrefab; 
    public GameObject fingerHintObject;
    public GameObject skillExchangeCanva;
    public GameObject player;
    public List<int> unlockedSkillIndeics = new List<int> { 0, 1 };
    public GameObject[] tempSlotPrefabs = new GameObject[2]; 
    public static SkillUiExchanger Instance;
    private RectTransform resetarrow;
    public RectTransform key1Slot;
    public RectTransform key2Slot;
    public GameObject arrowPrefab;
    private RectTransform resetButton;
    private GameObject changestatusCanva;
    private Coroutine fingerHintCoroutine;
    // private TextMeshProUGUI key1Text;
    // private TextMeshProUGUI key2Text;
    // private TextMeshProUGUI Hint;
    // private TextMeshProUGUI keyHint;
    public List<GameObject> allSkillPrefabs;
    public GameObject elitecanva;
    private bool isConfiguring = false;
    private List<GameObject> skillarrowlist;
    private List<GameObject> selectedSkillPrefabs = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
        skillExchangeCanva = GameObject.Find("SkillCanva");
        elitecanva = GameObject.Find("EliteCanva");
        skillarrowlist = new List<GameObject>();
        
        if (skillExchangeCanva != null)
        {
            changestatusCanva = skillExchangeCanva.transform.Find("changestatus")?.gameObject;
            // key1Text = skillExchangeCanva.transform.Find("Key1Text")?.GetComponent<TextMeshProUGUI>();
            // key2Text = skillExchangeCanva.transform.Find("Key2Text")?.GetComponent<TextMeshProUGUI>();
            // Hint = skillExchangeCanva.transform.Find("Hint")?.GetComponent<TextMeshProUGUI>();
            // keyHint = skillExchangeCanva.transform.Find("keyhint")?.GetComponent<TextMeshProUGUI>();
            resetarrow = skillExchangeCanva.transform.Find("resetfinger")?.GetComponent<RectTransform>();
            resetButton = skillExchangeCanva.transform.Find("ResetButton")?.GetComponent<RectTransform>();
            resetarrow.anchoredPosition = resetButton.anchoredPosition + new Vector2(0, -30f);
        }

        UpdateUnlockSkillUI();
    }

    private void Start()
    {
        skillExchangeCanva?.SetActive(false);
        changestatusCanva.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
        changestatusCanva?.SetActive(false);
        if (fingerHintObject != null)
            fingerHintObject.SetActive(false);
        //resetarrow.GetComponent<RectTransform>().anchoredPosition = resetButton.GetComponent<RectTransform>().anchoredPosition+new Vector2(0,15);
        elitecanva?.SetActive(false);
        // keyHint?.gameObject.SetActive(false);
    }

    public void UnlockSkill(int skillIndex)
    {
        if (!unlockedSkillIndeics.Contains(skillIndex))
        {
            unlockedSkillIndeics.Add(skillIndex);
            Debug.Log($"unlock {skillIndex}: {allSkillPrefabs[skillIndex].name}");

            // ✅ 讓剛解鎖的按鈕高亮
            StartCoroutine(DelayedHighlight(skillIndex));
        }
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleSkillUI();
        }
    }
    


    public void ToggleSkillUI()
    {
        bool isActive = skillExchangeCanva.activeSelf;
        skillExchangeCanva.SetActive(!isActive);
        
        if (!isActive)
        {
            isConfiguring = false;
            selectedSkillPrefabs.Clear();
            ClearSkillSlot(key1Slot);
            ClearSkillSlot(key2Slot);
            tempSlotPrefabs[0] = null;
            tempSlotPrefabs[1] = null;
            UpdateUnlockSkillUI();
            changestatusCanva?.SetActive(false);
            //UpdateCurrentSkillDisplay();
            // keyHint.gameObject.SetActive(false);
            
        }
        else
        {
            isConfiguring = false;
            resetarrow.gameObject.SetActive(true);
            fingerHintObject.SetActive(false);
            
        }
    }

    public void OnResetButtonClicked()
    {
        isConfiguring = true;
        selectedSkillPrefabs.Clear();
        UpdateUnlockSkillUI();
        resetarrow.gameObject.SetActive(false);
        changestatusCanva?.SetActive(false);
        ClearSkillSlot(key1Slot);
        ClearSkillSlot(key2Slot);
        if (fingerHintCoroutine != null)
            StopCoroutine(fingerHintCoroutine);
        fingerHintCoroutine = StartCoroutine(StartFingerHintAnimation());
        
        tempSlotPrefabs[0] = null;
        tempSlotPrefabs[1] = null;
        
    }

    private IEnumerator DelayedHighlight(int index)
{
    yield return new WaitForSeconds(2.5f);  // 等 UI fully active
    UpdateUnlockSkillUI(index);             // 高亮該技能
}
    public void OpenSkillUI()
    {
        skillExchangeCanva.SetActive(true);

        // 每次開的時候都清空
        ClearSkillSlot(key1Slot);
        ClearSkillSlot(key2Slot);
        tempSlotPrefabs[0] = null;
        tempSlotPrefabs[1] = null;

        isConfiguring = false;
        selectedSkillPrefabs.Clear();

        resetarrow?.gameObject.SetActive(true);
        fingerHintObject?.SetActive(false);

        UpdateUnlockSkillUI(); // 重新生成按鈕
    }


    

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        changestatusCanva.SetActive(false);
        selectedSkillPrefabs.Clear();
        resetarrow.gameObject.SetActive(true);
        // Hint.gameObject.SetActive(true);
        // keyHint.gameObject.SetActive(false);
        //foreach (GameObject arrow in skillarrowlist) arrow.SetActive(false);
        isConfiguring = false;
    }

    private IEnumerator StartFingerHintAnimation()
{
    yield return new WaitForEndOfFrame(); // 等一幀，確保技能都生出來

    if (SkillUiExchanger.Instance.skillGridParent.childCount == 0)
        yield break;

    RectTransform fromBtn = SkillUiExchanger.Instance.skillGridParent.GetChild(0).GetComponent<RectTransform>();
    RectTransform fingerImg = fingerHintObject.GetComponent<RectTransform>();

    // 第一次：指向 Key1Slot
    Vector2 fromPos = WorldToAnchored(fromBtn);
    Vector2 toPos1 = WorldToAnchored(key1Slot);

    fingerImg.anchoredPosition = fromPos;
    fingerImg.gameObject.SetActive(true);

    float duration = 1.3f;
    float t = 0;
    while (t < duration)
    {
        fingerImg.anchoredPosition = Vector2.Lerp(fromPos, toPos1, t / duration);
        t += Time.deltaTime;
        yield return null;
    }

    // 第二次：指向 Key2Slot
    float duration2 = 1.5f;
    Vector2 toPos2 = WorldToAnchored(key2Slot);
    t = 0;
    while (t < duration2)
    {
        fingerImg.anchoredPosition = Vector2.Lerp(fromPos, toPos2, t / duration2);
        t += Time.deltaTime;
        yield return null;
    }

    fingerImg.gameObject.SetActive(false);
    fingerHintCoroutine = null;
}

private void ClearSkillSlot(RectTransform slot)
{
    foreach (Transform child in slot)
    {
        Destroy(child.gameObject);
    }
}
private Vector2 WorldToAnchored(RectTransform target)
{
    Vector2 localPoint;
    RectTransformUtility.ScreenPointToLocalPointInRectangle(
        fingerHintObject.transform.parent as RectTransform,
        target.position, null, out localPoint);
    return localPoint;
}

    public void UpdateUnlockSkillUI(int? highlightIndex = null)
{
    foreach (Transform child in skillGridParent)
    {
        
            Destroy(child.gameObject);
    
    }
    //skillarrowlist.Clear();

    for (int i = 0; i < allSkillPrefabs.Count; i++)
    {
        if (!unlockedSkillIndeics.Contains(i))
            continue;

        GameObject prefab = allSkillPrefabs[i];
        GameObject btnObj = Instantiate(skillButtonPrefab, skillGridParent);
        AddskillButton uiButton = btnObj.GetComponent<AddskillButton>();
        uiButton.skillPrefab = prefab;

        Skill skill = prefab.GetComponent<Skill>();

        if (skill != null && btnObj.GetComponent<Image>() != null)
        {
            Sprite icon = prefab.GetComponent<Image>()?.sprite;
            if (icon != null)
                btnObj.GetComponent<Image>().sprite = icon;
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = skill.skillName;
                btnText.fontSize = 25f;
            }
        }
        if (highlightIndex.HasValue && i == highlightIndex.Value)
        {
            // 加 Glow
            GameObject glow = Instantiate(glowEffectPrefab, btnObj.transform);
            glow.SetActive(true); 
            glow.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            newskillglow flicker = glow.GetComponent<newskillglow>();
            if (flicker != null) flicker.StartFlicker();
            glow.transform.SetAsFirstSibling();
        }
        // 🔽 加上拖曳功能
        if (btnObj.GetComponent<CanvasGroup>() == null)
            btnObj.AddComponent<CanvasGroup>();

        Draggableskill dragScript = btnObj.AddComponent<Draggableskill>();
        dragScript.skillPrefab = prefab;

       
    }
}
public bool IsConfiguring()
{
    return isConfiguring;
}
public void ShowChangeStatusText(string message, float duration = 1.3f)
{
    changestatusCanva.SetActive(true);
    TextMeshProUGUI text = changestatusCanva.GetComponentInChildren<TextMeshProUGUI>();
    if (text != null)
        text.text = message;

    StartCoroutine(HideChangeStatusAfterDelay(duration));
}

private IEnumerator HideChangeStatusAfterDelay(float delay)
{
    yield return new WaitForSeconds(delay);
    changestatusCanva.SetActive(false);
    resetarrow?.gameObject.SetActive(true);
}
public void NotifySkillDropped(int slotIndex, GameObject prefab)
{
    tempSlotPrefabs[slotIndex] = prefab;

    if (tempSlotPrefabs[0] != null && tempSlotPrefabs[1] != null)
    {
        if (tempSlotPrefabs[0] != tempSlotPrefabs[1])
        {
            SkillController controller = player.GetComponent<SkillController>();

            for (int i = 0; i < 2; i++)
            {
                GameObject skillPrefab = tempSlotPrefabs[i];
                Skill skill = skillPrefab.GetComponent<Skill>();
                controller.ReplaceSkill(i, skillPrefab, skill.releaseType);
            }

            ShowChangeStatusText("Skills updated!", 2f);
            resetarrow.gameObject.SetActive(true);
        }
        else
        {
            ShowChangeStatusText("Update failed! Cannot use the same skill twice.", 2f);
            ClearSkillSlot(key2Slot);
            
        }

        
    }
}

   
// public void OnSkillButtonClicked(GameObject skillPrefab)
    // {
    //     if (!isConfiguring || selectedSkillPrefabs.Count >= 2) return;

    //     selectedSkillPrefabs.Add(skillPrefab);

    //     if (selectedSkillPrefabs.Count == 2)
    //     {
    //         CheckValidSkillSelection();
    //     }
    // }
// public void UpdateCurrentSkillDisplay()
//     {
//         SkillController controller = player.GetComponent<SkillController>();
//         if (controller != null)
//         {
//             for (int i = 0; i < 2; i++)
//             {
//                 GameObject prefab = controller.skillSlots[i].skillPrefab;
//                 if (prefab != null)
//                 {
//                     Skill skill = prefab.GetComponent<Skill>();
//                     string name = (skill != null) ? skill.skillName : "Unnamed";
//                     if (i == 0) key1Text.text = "Key1: " + name;
//                     else key2Text.text = "Key2: " + name;
//                 }
//                 else
//                 {
//                     if (i == 0) key1Text.text = "Key1: None";
//                     else key2Text.text = "Key2: None";
//                 }
//             }
//         }
//     }

// private void ApplySkillSelection()
//     {
//         SkillController controller = player.GetComponent<SkillController>();

//         for (int i = 0; i < 2; i++)
//         {
//             GameObject skillPrefab = selectedSkillPrefabs[i];
//             Skill skill = skillPrefab.GetComponent<Skill>();
//             controller.ReplaceSkill(i, skillPrefab, skill.releaseType);
//         }

//         Debug.Log("update successfully");
//         //UpdateCurrentSkillDisplay();
//         changestatusCanva.SetActive(true);
//         resetarrow.gameObject.SetActive(true);
//         // Hint.gameObject.SetActive(true);
//         // keyHint.gameObject.SetActive(false);
//         //foreach (GameObject arrow in skillarrowlist) arrow.SetActive(false);

//         StartCoroutine(ResetAfterDelay());
//     }

    


}
