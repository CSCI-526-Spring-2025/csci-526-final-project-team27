using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class RealBag : MonoBehaviour, IDropHandler
{
    public static RealBag Instance { get; private set; }

    public GameObject slotPrefab;
    public Transform bagPanel;
    private int rows = 5;
    private int columns = 4;
    private bool[,] grid; // track which grid is occupied
    private int slotSize = 40;
    private int slotPadding = 5;
    private List<Transform> slotGridList = new List<Transform>(); 

    void Awake()
    {
        Instance = this;
        grid = new bool[rows, columns]; 
    }

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        Debug.Log("Creating grid slots");

        for (int i = 0; i < rows * columns; i++)
        {
            GameObject slot = Instantiate(slotPrefab, bagPanel);
            slotGridList.Add(slot.transform); 
        }

        Debug.Log("Finished creating grid slots");
    }


public void OnDrop(PointerEventData eventData)
{
    GameObject droppedItem = eventData.pointerDrag;

    if (droppedItem == null)
    {
        Debug.LogError("OnDrop: No item was dragged.");
        return;
    }

    Draggable draggable = droppedItem.GetComponent<Draggable>();

    if (draggable == null || draggable.itemData == null)
    {
        Debug.LogError("OnDrop: Invalid draggable item.");
        return;
    }

    // 🔥 取得滑鼠位置，並轉換成 `Grid` 格子索引
    Vector2 dropPosition = Input.mousePosition;
    Vector2Int gridPosition = GetGridPositionFromMouse(dropPosition);

    // 🔹 **檢查該位置是否可以放置 `Item`**
    if (!CanPlaceItem(gridPosition.y, gridPosition.x, draggable.itemData.size.x, draggable.itemData.size.y))
    {
        Debug.LogWarning($"OnDrop: Cannot place {draggable.itemData.itemName} at {gridPosition}");
        draggable.ResetPosition(); // 🔥 直接回到 `tempstore`
        return;
    }

    // 🔹 **如果可以放，就放進 `GridSlot`**
    PlaceItemInGrid(draggable, gridPosition);
}

Vector2Int GetGridPositionFromMouse(Vector2 mousePosition)
{
    Vector2 localPoint;
    RectTransformUtility.ScreenPointToLocalPointInRectangle(
        bagPanel as RectTransform, mousePosition, null, out localPoint);

    // 🔥 position calibration
    float gridX = (localPoint.x + (columns * (slotSize + slotPadding)) / 2) / (slotSize + slotPadding);
    float gridY = ((rows * (slotSize + slotPadding)) / 2 - localPoint.y) / (slotSize + slotPadding);

    int col = Mathf.Clamp(Mathf.FloorToInt(gridX), 0, columns - 1);
    int row = Mathf.Clamp(Mathf.FloorToInt(gridY), 0, rows - 1);

    return new Vector2Int(col, row);
}

   

    // check if enough to place such item
    bool CanPlaceItem(int startRow, int startCol, int itemWidth, int itemHeight)
    {
        for (int row = startRow; row < startRow + itemHeight; row++)
        {
            for (int col = startCol; col < startCol + itemWidth; col++)
            {
                if (row >= rows || col >= columns || grid[row, col])
                {
                    return false; 
                }
            }
        }
        return true;
    }

    void PlaceItemInGrid(Draggable draggable, Vector2Int position)
{
    Item itemData = draggable.itemData;
    int itemWidth = itemData.size.x;
    int itemHeight = itemData.size.y;

    // 🔥 標記 `Grid` 內所有被 `Item` 佔用的格子
    for (int row = position.y; row < position.y + itemHeight; row++)
    {
        for (int col = position.x; col < position.x + itemWidth; col++)
        {
            grid[row, col] = true;

            // 🔥 讓 `GridSlot` 變色，顯示被佔用
            Transform gridSlot = GetGridTransformAt(new Vector2Int(col, row));
            if (gridSlot != null)
            {
                Image slotImage = gridSlot.GetComponent<Image>();
                if (slotImage != null)
                {
                    slotImage.color = draggable.itemData.itemColor; // ✅ 設定 `Item` 顏色
                }
            }
        }
    }

    // 🔥 **隱藏 `Item` 本體，而不是刪除它**
    draggable.GetComponent<CanvasGroup>().alpha = 0; // ✅ 讓 `Item` 變透明
    draggable.GetComponent<CanvasGroup>().interactable = false; // ✅ 禁用互動
    draggable.GetComponent<CanvasGroup>().blocksRaycasts = false; // ✅ 不會影響滑鼠事件
}



    // 🔹 取得 `Item` 應該對應的 `gridPrefab`
    Transform GetGridTransformAt(Vector2Int position)
    {
        int index = position.y * columns + position.x;
        if (index >= 0 && index < slotGridList.Count)
        {
            return slotGridList[index];
        }
        return null;
    }
}
