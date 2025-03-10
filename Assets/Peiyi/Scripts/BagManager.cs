using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class BagManager : MonoBehaviour
{
    public static BagManager Instance { get; private set; } // 单例实例

    public List<Item> inventory = new List<Item>(); 
    //public Transform inventoryGrid; 
    //public GameObject inventorySlotPrefab; 
    public GameObject inventoryPanel; // Assign InventoryPanel in Inspector
    private bool isInventoryOpen = false;
    public int coinCount = 0; // 
    public TextMeshProUGUI coinText; // UI OF COIN
    public Dictionary<string, int> itemCounts = new Dictionary<string, int>();
    public bool isTutorial = false;


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
    }


    void Start()
    {
       
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
    
}
