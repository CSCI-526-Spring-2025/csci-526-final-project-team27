using UnityEngine;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    public int currentCoins = 0; // 当前金币数量
    [SerializeField] private TMPro.TextMeshProUGUI coinText; // 金币数量文本

    public static PlayerInventory Instance { get; private set; } // 单例实例

    // 数据收集
    private FirebaseDataUploader dataUploader;


    private void Awake()
    {
        // 确保单例唯一性
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 确保场景切换时不会销毁
        }
        else
        {
            Destroy(gameObject); // 如果已经存在实例，销毁当前对象
            return;
        }

        GameObject coinTextObj = GameObject.Find("CoinText");
        if (coinTextObj != null)
        {
            coinText = coinTextObj.GetComponent<TextMeshProUGUI>();
            Debug.Log("CoinText found.");
        }
        else
        {
            Debug.LogWarning("No GameObject named 'CoinText' found in scene.");
        }
    }
    void Start()
    {
        dataUploader = FindFirstObjectByType<FirebaseDataUploader>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount;
        Debug.Log("Coins: " + currentCoins);

        // 硬币数据上传
        if (dataUploader != null)
        {
            dataUploader.UpdateData("CoinCollected", dataUploader.GetData("CoinCollected") + amount);
        }


        coinText.text = "Coins: " + currentCoins;
    }

    public bool PurchaseItem(int cost)
    {
        if (currentCoins >= cost)
        {
            currentCoins -= cost;
            Debug.Log("Coins: " + currentCoins);
            coinText.text = "Coins: " + currentCoins;
            return true;
        }
        else
        {
            Debug.Log("Not enough coins to purchase item.");
            return false;
        }
    }
}
