using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Draggableskill : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject skillPrefab;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform originalParent;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!SkillUiExchanger.Instance.IsConfiguring()) return; // ❌ 未按 Reset 不可拖曳

        originalParent = transform.parent;
        transform.SetParent(transform.root); // 避免被 UI 擋住
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!SkillUiExchanger.Instance.IsConfiguring()) return; // ❌ 未按 Reset 不可拖曳

        rectTransform.anchoredPosition += eventData.delta / transform.root.localScale.x;
     
       
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!SkillUiExchanger.Instance.IsConfiguring()) return; // ❌ 未按 Reset 不可拖曳

        if (transform.parent == transform.root)  // 被拉出來但沒被 OnDrop 接住
        {
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}

