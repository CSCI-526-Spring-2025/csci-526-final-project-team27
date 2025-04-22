using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class skillslotdrop : MonoBehaviour, IDropHandler
{
    public int slotIndex; // 0 = Key1, 1 = Key2

    public void OnDrop(PointerEventData eventData)
    {
        if (!SkillUiExchanger.Instance.IsConfiguring())
        {
            Debug.Log("拖曳不允許，請先按下 Reset 開始配置技能！");
            return;
        }

        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        Draggableskill dragHandler = dropped.GetComponent<Draggableskill>();

        if (dragHandler != null)
        {
            GameObject skillPrefab = dragHandler.skillPrefab;
            //Skill skill = skillPrefab.GetComponent<Skill>();

            //SkillController controller = SkillUiExchanger.Instance.player.GetComponent<SkillController>();
            
            SkillUiExchanger.Instance.NotifySkillDropped(slotIndex, skillPrefab);
            
            dropped.transform.SetParent(transform, false);  // 第二參數 false 很重要！
            RectTransform rt = dropped.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(82.5f, -82.5f);
            //SkillUiExchanger.Instance.NotifySkillDropped(slotIndex, skillPrefab);


            //Debug.Log($"技能槽 {slotIndex + 1} 更新為：{skill.skillName}");
        }
    }
}
