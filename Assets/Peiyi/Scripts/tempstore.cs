using UnityEngine;
using System.Collections.Generic;
using TMPro;
public class tempstore : MonoBehaviour
{
    public GameObject slotPrefab;
    public List<Item> ItemList;
    public Transform storePanel;
    private int slotSize = 40;
    private int slotPadding = 5;
    private int x_start=-150;
    private int y_start=90;
    private int gridWidth = 10; 
    private int gridHeight = 6; 
    private bool[,] grid;

    void Start()
    {
        RectTransform storeRect = storePanel.GetComponent<RectTransform>();
        grid = new bool[gridHeight, gridWidth]; 
        GenerateItem();
    }

    void GenerateItem()
    {
        foreach (Item item in ItemList)
        {
            Vector2Int? availablePosition = FindAvailableGridPosition(item);
            if (availablePosition.HasValue)
            {
                PlaceItemInGrid(item, availablePosition.Value);
            }
            else
            {
                Debug.LogWarning($"No space available for {item.itemName}");
            }
        }
    }

    // can grid find place to place item?
    Vector2Int? FindAvailableGridPosition(Item item)
    {
        int itemWidth = item.size.x;
        int itemHeight = item.size.y;

        for (int row = 0; row < gridHeight; row++)
        {
            for (int col = 0; col < gridWidth; col++)
            {
                if (CanPlaceItem(row, col, itemWidth, itemHeight))
                {
                    return new Vector2Int(col, row);
                }
            }
        }
        return null; // 
    }

    // ensure item completely place
    bool CanPlaceItem(int startRow, int startCol, int itemWidth, int itemHeight)
    {
        if (startCol + itemWidth > gridWidth || startRow + itemHeight > gridHeight)
        {
            return false; 
        }
        for (int row = startRow; row < startRow + itemHeight; row++)
        {
            for (int col = startCol; col < startCol + itemWidth; col++)
            {
                if (grid[row, col]) return false; 
            }
        }
        return true;
    }

    // 🔹 把 `Item` 放到 `Grid` 內
    void PlaceItemInGrid(Item item, Vector2Int position)
    {
        int itemWidth = item.size.x;
        int itemHeight = item.size.y;
        for (int row = position.y; row < position.y + itemHeight; row++)
        {
            for (int col = position.x; col < position.x + itemWidth; col++)
            {
                grid[row, col] = true;
            }
        }
        
        float x = x_start+ position.x * (slotSize + slotPadding) + 30 * position.x;//gap between items
        float y = y_start-position.y * (slotSize + slotPadding);

        CreateItem(item, new Vector2(x, y));
    }

    // 🔹 建立 `Item` 的 UI
    void CreateItem(Item itemData, Vector2 position)
    {
        GameObject itemContainer = new GameObject(itemData.itemName);
        itemContainer.transform.SetParent(storePanel);
        RectTransform containerRect = itemContainer.AddComponent<RectTransform>();

        containerRect.sizeDelta = new Vector2(
            itemData.size.x * (slotSize + slotPadding),
            itemData.size.y * (slotSize + slotPadding)
        );

        containerRect.anchoredPosition = position;
        containerRect.localScale = Vector3.one;
        Draggable draggable = itemContainer.AddComponent<Draggable>();
        draggable.itemData = itemData;
        int[,] shapeMatrix = itemData.GetShapeMatrix();

        for (int row = 0; row < itemData.size.y; row++)
        {
            for (int col = 0; col < itemData.size.x; col++)
            {
                GameObject slot = Instantiate(slotPrefab, itemContainer.transform);
                RectTransform slotRect = slot.GetComponent<RectTransform>();
                slotRect.anchoredPosition = new Vector2(
                    col * (slotSize + slotPadding),
                    -row * (slotSize + slotPadding)
                );

                UnityEngine.UI.Image slotImage = slot.GetComponent<UnityEngine.UI.Image>();
                if (slotImage != null)
                {
                    slotImage.color = itemData.itemColor;
                }

                if (shapeMatrix[row, col] == 0)
                {
                    slot.SetActive(false);
                }
            }
        }


        //ITEM INFO
        GameObject nameTextObj = new GameObject("ItemName");
        nameTextObj.transform.SetParent(itemContainer.transform);
        TextMeshProUGUI nameText = nameTextObj.AddComponent<TextMeshProUGUI>();
        nameText.text = itemData.itemName;
        nameText.fontSize = 16;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = Color.white;

        RectTransform nameTextRect = nameTextObj.GetComponent<RectTransform>();
        nameTextRect.sizeDelta = new Vector2(80, 16);
        nameTextRect.anchoredPosition = new Vector2(containerRect.sizeDelta.x / 2, -containerRect.sizeDelta.y - 5);

        // 🔹 **新增 Price UI**
        GameObject priceTextObj = new GameObject("PriceText");
        priceTextObj.transform.SetParent(itemContainer.transform);
        TextMeshProUGUI priceText = priceTextObj.AddComponent<TextMeshProUGUI>();
        priceText.text = "$" + itemData.price.ToString();
        priceText.fontSize = 16;
        priceText.alignment = TextAlignmentOptions.Center;
        priceText.color = Color.yellow;

        RectTransform priceTextRect = priceTextObj.GetComponent<RectTransform>();
        priceTextRect.sizeDelta = new Vector2(20, 16);
        priceTextRect.anchoredPosition = new Vector2(containerRect.sizeDelta.x / 2, -containerRect.sizeDelta.y - 25);

        // 🔹 **新增 Attribute UI**
        GameObject attributeTextObj = new GameObject("AttributeText");
        attributeTextObj.transform.SetParent(itemContainer.transform);
        TextMeshProUGUI attributeText = attributeTextObj.AddComponent<TextMeshProUGUI>();
        attributeText.text = itemData.attributeText;
        attributeText.fontSize = 16;
        attributeText.alignment = TextAlignmentOptions.Center;
        attributeText.color = Color.cyan;

        RectTransform attributeTextRect = attributeTextObj.GetComponent<RectTransform>();
        attributeTextRect.sizeDelta = new Vector2(80, 16);
        attributeTextRect.anchoredPosition = new Vector2(containerRect.sizeDelta.x / 2, -containerRect.sizeDelta.y - 45);
    }
}
