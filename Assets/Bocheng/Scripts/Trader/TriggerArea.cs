using UnityEngine;
using UnityEngine.Events;

public class TriggerArea : MonoBehaviour
{
    public UnityEvent OnEnterRange; // 进入范围事件
    public UnityEvent OnExitRange;  // 离开范围事件
    public string tagFilter = "Player"; // 过滤标签

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(tagFilter)) // 确保是玩家进入
        {
            OnEnterRange?.Invoke();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(tagFilter)) // 确保是玩家离开
        {
            OnExitRange?.Invoke();
        }
    }
}
