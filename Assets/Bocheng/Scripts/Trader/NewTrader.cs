using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NewTrader : MonoBehaviour
{
     public Canvas worldSpaceCanvas;  // 商人的交互提示 Canvas
     public GameObject shopUI;        // 商店UI
     private bool isPlayerNear = false;

     [Header("商店物品")]
     public tempstore tempstore;
     public List<Item> item;
     private Item chosenItems;

     [Header("商店物品UI")]
     public List<Image> itemsUI;
     public List<TextMeshProUGUI> itemsPriceUI;
     
     private GameObject BagUI;
     private bool generated = false;
     private bool isOpen = false;

     private void Start()
     {
         GenerateShop();
         worldSpaceCanvas.gameObject.SetActive(false);
         shopUI.SetActive(false);
         BagUI = CtrlCtrl.Instance.GetBagPanel();
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
        if(generated) return;
        //random choose 1 item set to tempstore
        tempstore.ItemList = new List<Item>();
        int chosenIndex = Random.Range(0, item.Count);
        tempstore.ItemList.Add(item[chosenIndex]);
        chosenItems = item[chosenIndex];
        generated = true;
     }

     public void OpenShop()
     {
         shopUI.SetActive(true);
         CtrlCtrl.Instance.LockMove(true);
         CtrlCtrl.Instance.ToggleShootCtrler(false);
         isOpen = true;
        if (BagUI != null)
        {
            BagUI.SetActive(true);
        }
    }

     public void CloseShop()
     {
         shopUI.SetActive(false);
         CtrlCtrl.Instance.LockMove(false);
         CtrlCtrl.Instance.ToggleShootCtrler(true);
         isOpen = false;
        if(BagUI != null)
        {
            BagUI.SetActive(false);
        }

    }

     public void BuyItem(int index)
     {

     }
}
