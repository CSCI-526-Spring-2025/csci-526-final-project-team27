using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class BagManager : MonoBehaviour
{
    public static BagManager Instance { get; private set; } // 单例实例

    public List<Item> inventory = new List<Item>(); 
    public GameObject store;
    public GameObject realbag;
    public GameObject inventoryPanel; // Assign InventoryPanel in Inspector
    private bool isInventoryOpen = false;
    
    public GameObject coin;
    public Dictionary<string, int> itemCounts = new Dictionary<string, int>();
    public bool isTutorial = false;
    private RectTransform realRect;
    private RectTransform storeRect;
    private RectTransform panelRect;
    private RectTransform coinRect;
    private void Awake()
    {
        /*
        // 确保单例唯一性
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject); 
            return;
        }*/
        if (Instance != null && Instance != this)
        {
            // 特殊判断 Tutorial 的情况
            if (Instance.isTutorial)
            {
                Destroy(Instance.gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (store == null) store = GameObject.Find("tempstore");
        if (realbag == null) realbag = GameObject.Find("RealBag");
        if (coin == null) coin = GameObject.Find("CoinText");
        realRect = realbag.GetComponent<RectTransform>();
        storeRect = store.GetComponent<RectTransform>();
        panelRect = inventoryPanel.GetComponent<RectTransform>();
        coinRect = coin.GetComponent<RectTransform>();
    }


    void Start()
    {   
        
        storeRect.anchoredPosition = new Vector2(-100, storeRect.anchoredPosition.y);
        panelRect.anchoredPosition = new Vector2(0, panelRect.anchoredPosition.y);
        panelRect.sizeDelta = new Vector2(230, panelRect.sizeDelta.y);
        realRect.anchoredPosition = new Vector2(0, realRect.anchoredPosition.y);
        coinRect.anchoredPosition = new Vector2(-70, coinRect.anchoredPosition.y);
        store.SetActive(false);
        realbag.SetActive(true);
        
        inventoryPanel.SetActive(false);
    }
    
    void Update(){
        if(Input.GetKeyDown(KeyCode.B)){
            ToggleInventory();
        }
    }

   
    

    
     public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);
        Debug.Log("Inventory " + (isInventoryOpen ? "Opened" : "Closed"));
    }
    public void UpdateBagDisplay(string prefabName){
        Debug.Log("bagmanager handle room change");
        
        
        
        if (prefabName == "RoomPrefab_Shop")//enter shop
        {
            store.SetActive(true);
            realbag.SetActive(true);
            panelRect.sizeDelta = new Vector2(600, panelRect.sizeDelta.y);
            storeRect.sizeDelta = new Vector2(380, storeRect.sizeDelta.y);
            realRect.anchoredPosition = new Vector2(190, realRect.anchoredPosition.y);
            coinRect.anchoredPosition = new Vector2(120, coinRect.anchoredPosition.y);
            
        }
        else
        {
            store.SetActive(false);
            realbag.SetActive(true);
            panelRect.sizeDelta = new Vector2(230, panelRect.sizeDelta.y);
            realRect.anchoredPosition = new Vector2(0, realRect.anchoredPosition.y);
            coinRect.anchoredPosition = new Vector2(-70, coinRect.anchoredPosition.y);
        }
    }
    
}
