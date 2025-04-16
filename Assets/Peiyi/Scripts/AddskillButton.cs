using UnityEngine;
using UnityEngine.UI;

public class AddskillButton : MonoBehaviour
{
    public GameObject skillPrefab;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
     {
    //     // 呼叫 SkillUiExchanger 中的主流程
    //     SkillUiExchanger.Instance.OnSkillButtonClicked(skillPrefab);
    //     Debug.Log("點到技能按鈕：" + skillPrefab.name);
    }
}
