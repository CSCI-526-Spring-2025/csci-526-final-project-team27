using UnityEngine;
using System;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    public int currentCoins = 0; // 当前金币数量
    [SerializeField] private TMPro.TextMeshProUGUI coinText; // 金币数量文本
    

    private void Awake()
    {
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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount;
        Debug.Log("Coins: " + currentCoins);
        
        coinText.text = "Coins: " + currentCoins;
    }
}
