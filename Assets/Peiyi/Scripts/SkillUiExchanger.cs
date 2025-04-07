using UnityEngine;
using System.Collections.Generic;
using System.Collections; 
using UnityEngine.UI;
using TMPro;

public class SkillUiExchanger : MonoBehaviour
{
    public Transform skillGridParent;
    public GameObject skillButtonPrefab;
    public GameObject skillExchangeCanva;
    public GameObject player;
    public List<int> unlockedSkillIndeics = new List<int> { 0, 1 };
    public static SkillUiExchanger Instance;
    private RectTransform resetarrow;
    
    public GameObject arrowPrefab;
    private RectTransform resetButton;
    private GameObject changestatusCanva;
    private TextMeshProUGUI key1Text;
    private TextMeshProUGUI key2Text;
    private TextMeshProUGUI Hint;
    private TextMeshProUGUI keyHint;
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
            key1Text = skillExchangeCanva.transform.Find("Key1Text")?.GetComponent<TextMeshProUGUI>();
            key2Text = skillExchangeCanva.transform.Find("Key2Text")?.GetComponent<TextMeshProUGUI>();
            Hint = skillExchangeCanva.transform.Find("Hint")?.GetComponent<TextMeshProUGUI>();
            keyHint = skillExchangeCanva.transform.Find("keyhint")?.GetComponent<TextMeshProUGUI>();
            resetarrow = skillExchangeCanva.transform.Find("resetarrow")?.GetComponent<RectTransform>();
            resetButton = skillExchangeCanva.transform.Find("ResetButton")?.GetComponent<RectTransform>();
            resetarrow.anchoredPosition = resetButton.anchoredPosition + new Vector2(0, -30f);
        }

        UpdateUnlockSkillUI();
    }

    private void Start()
    {
        skillExchangeCanva?.SetActive(false);
        //changestatusCanva.GetComponent<RectTransform>().anchoredPosition = new Vector2(100f, 0f);
        changestatusCanva?.SetActive(false);

        elitecanva?.SetActive(false);
        keyHint?.gameObject.SetActive(false);
    }

    public void UnlockSkill(int skillIndex)
    {
        if (!unlockedSkillIndeics.Contains(skillIndex))
        {
            unlockedSkillIndeics.Add(skillIndex);
            Debug.Log($"unlock {skillIndex}: {allSkillPrefabs[skillIndex].name}");
            UpdateUnlockSkillUI();
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
            UpdateCurrentSkillDisplay();
            keyHint.gameObject.SetActive(false);
        }
        else
        {
            isConfiguring = false;
            
        }
    }

    public void OnResetButtonClicked()
    {
        isConfiguring = true;
        selectedSkillPrefabs.Clear();
        resetarrow.gameObject.SetActive(false);
        Hint.gameObject.SetActive(false);
        keyHint.gameObject.SetActive(true);
        foreach (GameObject arrow in skillarrowlist)
        {
            arrow.SetActive(true);
        }
    }

    public void OnSkillButtonClicked(GameObject skillPrefab)
    {
        if (!isConfiguring || selectedSkillPrefabs.Count >= 2) return;

        selectedSkillPrefabs.Add(skillPrefab);

        if (selectedSkillPrefabs.Count == 2)
        {
            CheckValidSkillSelection();
        }
    }

    private void CheckValidSkillSelection()
    {
        if (selectedSkillPrefabs[0] == selectedSkillPrefabs[1])
        {
            Debug.LogWarning("update failed");
            changestatusCanva.SetActive(true);
            changestatusCanva.GetComponentInChildren<TextMeshProUGUI>().text = "Update failed!, no same skill";
            StartCoroutine(ResetAfterDelay());
        }
        else
        {
            ApplySkillSelection();
        }
    }

    private void ApplySkillSelection()
    {
        SkillController controller = player.GetComponent<SkillController>();

        for (int i = 0; i < 2; i++)
        {
            GameObject skillPrefab = selectedSkillPrefabs[i];
            Skill skill = skillPrefab.GetComponent<Skill>();
            controller.ReplaceSkill(i, skillPrefab, skill.releaseType);
        }

        Debug.Log("update successfully");
        UpdateCurrentSkillDisplay();
        changestatusCanva.SetActive(true);
        changestatusCanva.GetComponentInChildren<TextMeshProUGUI>().text = "Skill updated";

        resetarrow.gameObject.SetActive(true);
        Hint.gameObject.SetActive(true);
        keyHint.gameObject.SetActive(false);
        foreach (GameObject arrow in skillarrowlist) arrow.SetActive(false);

        StartCoroutine(ResetAfterDelay());
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        changestatusCanva.SetActive(false);
        selectedSkillPrefabs.Clear();
        resetarrow.gameObject.SetActive(true);
        Hint.gameObject.SetActive(true);
        keyHint.gameObject.SetActive(false);
        foreach (GameObject arrow in skillarrowlist) arrow.SetActive(false);
        isConfiguring = false;
    }

    public void UpdateCurrentSkillDisplay()
    {
        SkillController controller = player.GetComponent<SkillController>();
        if (controller != null)
        {
            for (int i = 0; i < 2; i++)
            {
                GameObject prefab = controller.skillSlots[i].skillPrefab;
                if (prefab != null)
                {
                    Skill skill = prefab.GetComponent<Skill>();
                    string name = (skill != null) ? skill.skillName : "Unnamed";
                    if (i == 0) key1Text.text = "Key1: " + name;
                    else key2Text.text = "Key2: " + name;
                }
                else
                {
                    if (i == 0) key1Text.text = "Key1: None";
                    else key2Text.text = "Key2: None";
                }
            }
        }
    }

    public void UpdateUnlockSkillUI()
    {
        foreach (Transform child in skillGridParent)
        {
            Destroy(child.gameObject);
        }
        skillarrowlist.Clear();

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

            GameObject arrowObj = Instantiate(arrowPrefab);
            arrowObj.transform.SetParent(btnObj.transform, false);
            arrowObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 50f);
            arrowObj.SetActive(false);
            skillarrowlist.Add(arrowObj);
        }
    }
}
