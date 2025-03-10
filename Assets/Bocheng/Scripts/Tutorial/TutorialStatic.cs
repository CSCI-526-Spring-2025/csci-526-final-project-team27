using System.Collections.Generic;
using UnityEngine;

public class TutorialStatic : MonoBehaviour
{
    public static TutorialStatic Instance { get; private set; } // 单例实例
    public GameObject FirstFightUI; // 第一次战斗UI

    private void Awake()
    {
        // 确保单例唯一性
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // 如果已经存在实例，销毁当前对象
            return;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenFirstFightUI()
    {
        FirstFightUI.SetActive(true);
        CtrlCtrl.Instance.LockMove(true);
        CtrlCtrl.Instance.ToggleShootCtrler(false);
    }

    public void MoveToFirstFight()
    {
        CtrlCtrl.Instance.LockMove(false);
        CtrlCtrl.Instance.ToggleShootCtrler(true);
        Debug.Log("Move to first fight");
        FirstFightUI.SetActive(false);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        RoomManager_BC.Instance.MoveTo(player, new Vector2Int(1,0));
    }
}
