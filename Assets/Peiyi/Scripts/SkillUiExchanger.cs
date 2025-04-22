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
    private RectTransform fromBtn;
    public List<int> unlockedSkillIndeics = new List<int> { 0, 1 };
    public GameObject[] tempSlotPrefabs = new GameObject[2]; 
    public static SkillUiExchanger Instance;
    private RectTransform resetarrow;
    public RectTransform key1Slot;
    public RectTransform key2Slot;
    public GameObject arrowPrefab;
    private RectTransform resetButton;
    private  bool shouldCancelHint=false;
    private GameObject changestatusCanva;
    private Coroutine fingerHintCoroutine;
    // private TextMeshProUGUI key1Text;
    // private TextMeshProUGUI key2Text;
    // private TextMeshProUGUI Hint;
    // private TextMeshProUGUI keyHint;
    private Coroutine hideStatusCoroutine; 
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
        //changestatusCanva.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
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
        
        ClearSkillSlot(key1Slot);
        ClearSkillSlot(key2Slot);
        if (hideStatusCoroutine != null)
        {
            StopCoroutine(hideStatusCoroutine);
            hideStatusCoroutine = null;
        }
        changestatusCanva?.SetActive(false);
        if (fingerHintCoroutine != null)
            StopCoroutine(fingerHintCoroutine);
       
        if (fromBtn !=null)
        {
            fingerHintCoroutine = StartCoroutine(PlayBothHintsSequentially(fromBtn));
        }
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


   

    

    private IEnumerator StartFingerHintAnimation(RectTransform fromBtn,Vector2 toPos,float duration)
{
    yield return new WaitForEndOfFrame(); // 等一幀，確保技能都生出來

    if (SkillUiExchanger.Instance.skillGridParent.childCount == 0)
        yield break;

    
    RectTransform fingerImg = fingerHintObject.GetComponent<RectTransform>();
    Vector2 fromPos=WorldToAnchored(fromBtn);
    fingerImg.anchoredPosition = fromPos;
    fingerImg.gameObject.SetActive(true);

    
    float t = 0;
    while (t < duration)
    {
        fingerImg.anchoredPosition = Vector2.Lerp(fromPos, toPos, t / duration);
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
    if (target == null || target.Equals(null))
    {
        Debug.LogWarning("WorldToAnchored: target is null or destroyed.");
        return Vector2.zero;
    }
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
    if (skillGridParent.childCount > 0)
    {
        fromBtn = skillGridParent.GetComponent<RectTransform>();
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

    hideStatusCoroutine = StartCoroutine(HideChangeStatusAfterDelay(duration));
}

private IEnumerator HideChangeStatusAfterDelay(float delay)
{
    yield return new WaitForSeconds(delay);
    changestatusCanva.SetActive(false);
    if (isConfiguring == false)
        resetarrow?.gameObject.SetActive(true);
    
    hideStatusCoroutine = null;
}
public void NotifySkillDropped(int slotIndex, GameObject prefab)
{
    shouldCancelHint=true;
    tempSlotPrefabs[slotIndex] = prefab;

    bool slot0Filled = tempSlotPrefabs[0] != null;
    bool slot1Filled = tempSlotPrefabs[1] != null;

    if (slot0Filled && slot1Filled)
    {
        if (hideStatusCoroutine != null)
            StopCoroutine(hideStatusCoroutine);
        if (fingerHintCoroutine != null)
            StopCoroutine(fingerHintCoroutine);

        if (tempSlotPrefabs[0] != tempSlotPrefabs[1])
        {
            SkillController controller = player.GetComponent<SkillController>();
            for (int i = 0; i < 2; i++)
            {
                GameObject skillPrefab = tempSlotPrefabs[i];
                Skill skill = skillPrefab.GetComponent<Skill>();
                controller.ReplaceSkill(i, skillPrefab, skill.releaseType);
            }

            ShowChangeStatusText("Skills updated!", 1.3f);
            isConfiguring = false;
        }
        else
        {
            ShowChangeStatusText("Update failed! Cannot use the same skill twice.", 2f);
            ClearSkillSlot(key1Slot);
            ClearSkillSlot(key2Slot);
            tempSlotPrefabs[0] = null;
            tempSlotPrefabs[1] = null;
        }
    }
    else
    {
        ShowChangeStatusText("Drag two skills to update!", 3f);

        if (fingerHintCoroutine != null)
            StopCoroutine(fingerHintCoroutine);

        if (fromBtn != null)
        {
            if (slot0Filled && !slot1Filled)
            {
                fingerHintCoroutine = StartCoroutine(StartFingerHintAnimation(fromBtn, WorldToAnchored(key2Slot), 1.5f));
            }
            else if (slot1Filled && !slot0Filled)
            {
                fingerHintCoroutine = StartCoroutine(StartFingerHintAnimation(fromBtn, WorldToAnchored(key1Slot), 1.5f));
            }
            else if (!slot0Filled && !slot1Filled)
            {
                StartCoroutine(PlayBothHintsSequentially(fromBtn));
            }
        }
    }
}

private IEnumerator PlayBothHintsSequentially(RectTransform fromBtn)
{
    shouldCancelHint=false;
    if (fingerHintCoroutine != null)
    {
        StopCoroutine(fingerHintCoroutine);
        fingerHintCoroutine = null;
    }

    fingerHintCoroutine = StartCoroutine(StartFingerHintAnimation(fromBtn, WorldToAnchored(key1Slot), 1f));
    yield return fingerHintCoroutine;
    if (shouldCancelHint) yield break;
    yield return new WaitForSeconds(0.3f);

    fingerHintCoroutine = StartCoroutine(StartFingerHintAnimation(fromBtn, WorldToAnchored(key2Slot), 1.2f));
    yield return fingerHintCoroutine;

    fingerHintCoroutine = null;
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
