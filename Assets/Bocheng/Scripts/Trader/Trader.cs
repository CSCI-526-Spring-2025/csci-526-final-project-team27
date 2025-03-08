using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Trader : MonoBehaviour
{
    public Canvas worldSpaceCanvas;  // 商人的交互提示 Canvas
    public GameObject shopUI;        // 商店UI
    private bool isPlayerNear = false;

    [Header("商店物品")]
    public List<Item> item;
    private List<Item> chosenItems;

    [Header("商店物品UI")]
    public List<Image> itemsUI;
    public List<TextMeshProUGUI> itemsPriceUI;

    private bool generated = false;
    private bool isOpen = false;

    private void Start()
    {
        GenerateShop();
        worldSpaceCanvas.gameObject.SetActive(false);
        shopUI.SetActive(false);
    }

    public void PlayerEnteredRange()
    {
        isPlayerNear = true;
        worldSpaceCanvas.gameObject.SetActive(true);
    }

    public void PlayerExitedRange()
    {
        isPlayerNear = false;
        worldSpaceCanvas.gameObject.SetActive(false);
        //shopUI.SetActive(false); // 关闭商店UI
    }

    private void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E) && !isOpen)
        {
            OpenShop();
            return;
        }
        if(isOpen && Input.GetKeyDown(KeyCode.E))
        {
            CloseShop();
            return;
        }
    }

    private void GenerateShop()
    {
        if (generated) return;

        chosenItems = new List<Item>();
        foreach (Image itemUI in itemsUI)
        {
            int index = Random.Range(0, item.Count);
            chosenItems.Add(item[index]);
            itemUI.sprite = item[index].icon;
        }

        for (int i = 0; i < itemsPriceUI.Count; i++)
        {
            itemsPriceUI[i].text = chosenItems[i].price.ToString();
        }

        generated = true;
    }

    public void OpenShop()
    {
        shopUI.SetActive(true);
        CtrlCtrl.Instance.LockMove(true);
        CtrlCtrl.Instance.ToggleShootCtrler(false);
        isOpen = true;
    }

    public void CloseShop()
    {
        shopUI.SetActive(false);
        CtrlCtrl.Instance.LockMove(false);
        CtrlCtrl.Instance.ToggleShootCtrler(true);
        isOpen = false;
    }

    public void BuyItem(int index)
    {
        if (PlayerInventory.Instance != null)
        {
            bool success = PlayerInventory.Instance.PurchaseItem(chosenItems[index].price);
            if (success)
            {
                if (BagManager.Instance != null)
                {
                    BagManager.Instance.AddItem(chosenItems[index]);
                }
            }
        }
    }
}
