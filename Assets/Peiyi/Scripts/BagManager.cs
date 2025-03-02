using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class BagManager : MonoBehaviour
{
    public List<Item> inventory = new List<Item>(); 
    public Transform inventoryGrid; 
    public GameObject inventorySlotPrefab; 
    public GameObject inventoryPanel; // Assign InventoryPanel in Inspector
    private bool isInventoryOpen = false;
    public int coinCount = 0; // 
    public TextMeshProUGUI coinText; // UI OF COIN
    public Dictionary<string, int> itemCounts = new Dictionary<string, int>();
    void Start()
    {
        InitializeInventoryUI();
        inventoryPanel.SetActive(false);
    }
    

    void InitializeInventoryUI()
    {
        for (int i = 0; i < 11; i++) 
        {
            Debug.Log("i create block"+i);
            Instantiate(inventorySlotPrefab, inventoryGrid);
        }
        for (int i = 0; i < inventoryGrid.childCount; i++)
        {
            Transform slot = inventoryGrid.GetChild(i);
            Image icon = slot.GetComponentInChildren<Image>();
            TextMeshProUGUI itemCountText = slot.GetComponentInChildren<TextMeshProUGUI>();

            

            if (itemCountText != null) 
            {
                itemCountText.text = ""; // 🔥 清除 `0`
                itemCountText.enabled = false; // 🔥 完全隱藏數字
            }
    }
    }

    
     public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);
        Debug.Log("Inventory " + (isInventoryOpen ? "Opened" : "Closed"));
    }
    public void AddCoin(int amount)
{
    coinCount += amount;
    coinText.text = "Coins: " + coinCount;
}
    public void AddItem(Item newItem)
{
    if (itemCounts.ContainsKey(newItem.itemName))
    {
        itemCounts[newItem.itemName]++;
    }
    else
    {
        if (inventory.Count < 11)
        {
            inventory.Add(newItem);
            itemCounts[newItem.itemName] = 1;
        }
    }
    UpdateInventoryUI();
}
    void Update(){
        if(Input.GetKeyDown(KeyCode.B)){
            ToggleInventory();
        }
    }
    void UpdateInventoryUI()
{
    for (int i = 0; i < inventoryGrid.childCount; i++)
    {
        Transform slot = inventoryGrid.GetChild(i);
        Image icon = slot.GetComponentInChildren<Image>();
        TextMeshProUGUI itemCountText = slot.GetComponentInChildren<TextMeshProUGUI>(); // show the num of such item

        if (i < inventory.Count)//if item kind does not exceed bag capacity
        {
            Debug.Log("i do update");
            icon.sprite = inventory[i].icon;//load the image
            icon.enabled = true;

            // update item number
            if (itemCounts.ContainsKey(inventory[i].itemName))
            {
                itemCountText.text = itemCounts[inventory[i].itemName].ToString();
                itemCountText.enabled = true;
            }
            else
            {
                itemCountText.text = "";
                itemCountText.enabled = false;
            }
        }
        else
        {
            Debug.Log("i do update, but empty slot");
            icon.sprite = null; 
            icon.enabled = true; 

            itemCountText.text = "";
            itemCountText.enabled = false; 
        }
    }
}
}
