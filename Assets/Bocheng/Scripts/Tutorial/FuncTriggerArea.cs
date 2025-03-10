using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.GraphicsBuffer;

//[System.Serializable]
//public class GameObjectEvent : UnityEvent<GameObject> { }


public class FuncTriggerArea : MonoBehaviour
{
    [Header("触发模式")]
    public bool triggerOnEnter = true;  // 进入触发
    public bool triggerOnExit = false;  // 离开触发
    public bool triggerOnKeyPress = false; // 需要按键触发
    public KeyCode triggerKey = KeyCode.E; // 触发的按键
    public bool triggerOnce = true; // 是否只触发一次
    private bool hasTriggered = false; // 记录是否已经触发过

    [Header("触发事件")]
    public UnityEvent onTrigger; // 允许在 Inspector 里绑定方法
    //public GameObjectEvent onTriggerWithGameObject;

    private bool isPlayerInside = false; // 记录玩家是否在触发区域内

    private void Update()
    {
        if (triggerOnKeyPress && isPlayerInside && Input.GetKeyDown(triggerKey))
        {
            TriggerEvent();
            
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerOnEnter && other.CompareTag("Player"))
        {
            if(triggerOnKeyPress == false)
            {
                TriggerEvent();
                return;
            }
            isPlayerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (triggerOnExit && other.CompareTag("Player"))
        {
            isPlayerInside = false;
        }
    }

    private void TriggerEvent()
    {
        if (triggerOnce && hasTriggered) return; // 如果已经触发过，直接返回

        onTrigger.Invoke();
        //onTriggerWithGameObject.Invoke(gameObject);

        if (triggerOnce)
        {
            hasTriggered = true;
            this.enabled = false; // 禁用脚本
        }
    }
}
