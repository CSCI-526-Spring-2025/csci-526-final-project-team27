using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialStatic : MonoBehaviour
{
    public static TutorialStatic Instance { get; private set; } // 单例实例
    public GameObject FirstFightUI; // 第一次战斗UI
    public GameObject LeaveFFUI;    // 离开第一次战斗场景的UI
    public string NextSceneName;    // 下一个场景的名称


    private GameObject TriggerA;
    private int EnemyClearCount = 0;

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

    public void OpenFirstFightUI(GameObject Trigger)
    {
        TriggerA = Trigger;
        FirstFightUI.SetActive(true);
        CtrlCtrl.Instance.LockMove(true);
        CtrlCtrl.Instance.ToggleShootCtrler(false);
    }

    public void MoveToFirstFight()
    {
        CtrlCtrl.Instance.LockMove(false);
        CtrlCtrl.Instance.ToggleShootCtrler(true);

        FirstFightUI.SetActive(false);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        RoomManager_BC.Instance.MoveTo(player, new Vector2Int(1,0));
        TriggerA.GetComponent<BCsDoor>().bActive = true;
    }

    public void LeaveFirstFightUI()
    {
        CtrlCtrl.Instance.LockMove(true);
        CtrlCtrl.Instance.ToggleShootCtrler(false);
        LeaveFFUI.SetActive(true);
    }

    public void LeaveFirstFight()
    {
        CtrlCtrl.Instance.LockMove(false);
        CtrlCtrl.Instance.ToggleShootCtrler(true);
        LeaveFFUI.SetActive(false);
    }

    public void EnemyClear()
    {
        EnemyClearCount++;
        if(EnemyClearCount == 1)
        {
            LeaveFirstFightUI();
        }
    }

    public void OpenFirstLevel()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Destroy(gameObject);

        Debug.Log("OpenNextScene");
        //SceneManager.LoadScene(NextSceneName, LoadSceneMode.Single);
        //StartCoroutine(LoadNewScene());
        AsyncOperation op = SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
        //find player and set teammates,destroy this gameobject
    }

    IEnumerator LoadNewScene()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
        yield return op;
        
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(NextSceneName));
        Time.timeScale = 1f;

        Debug.Log("Scene Loaded: " + SceneManager.GetActiveScene().name);
    }
}
