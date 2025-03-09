using NUnit.Framework;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; } // 单例实例

    [Header("队友配置")]
    public List<GameObject> teammates = new List<GameObject>(); // 队友列表
    public List<Vector2> relativePositions = new List<Vector2>(); // 记录相对位置

    [Header("配置选项")]
    public bool inheritPreviousData = true; // 是否继承旧的实例数据

    private void Awake()
    {
        if (Instance == null)
        {
            // 第一次创建，设为 Instance 并保持数据
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (inheritPreviousData)
            {
                // 继承旧实例的数据
                InheritDataFrom(Instance);
            }

            // 销毁旧的实例
            Destroy(Instance.gameObject);

            // 设置自己为新的 Instance
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// 继承旧实例的数据
    /// </summary>
    private void InheritDataFrom(TutorialManager oldInstance)
    {
        this.teammates = new List<GameObject>(oldInstance.teammates);
        this.relativePositions = new List<Vector2>(oldInstance.relativePositions);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetTeammate()
    {
        //find gameobject with tag "player",get component "TeammateManager" and set teammates
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        TeammateManager teammateManager = player.GetComponent<TeammateManager>();
        //for each teammate in teammates, add to teammateManager
        for(int i = 0; i < teammates.Count; i++)
        {
            teammateManager.AddTeammate(teammates[i], relativePositions[i]);
            //teammates[i].GetComponent<Health_BC>().SetHealthBar(TutorialStatic.Instance.healthBar[i]);
        }
    }

    public void CallStaticFirstFight()
    {
        TutorialStatic.Instance.OpenFirstFightUI();
    }
}
