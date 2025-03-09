using System.Collections.Generic;
using UnityEngine;

public class TutorialStatic : MonoBehaviour
{
    public static TutorialStatic Instance { get; private set; } // 单例实例

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
}
