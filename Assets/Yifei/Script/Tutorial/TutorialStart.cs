using UnityEngine;
using System.Collections;
public class TutorialStart : MonoBehaviour
{
    private GameObject door;
    private GameObject arrow;
    private GameObject keys;

    private bool pressedW = false;
    private bool pressedA = false;
    private bool pressedS = false;
    private bool pressedD = false;
    private bool allKeysPressed = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 获取子对象
        door = transform.Find("Door_Right").gameObject;
        arrow = transform.Find("ArrowIndicator").gameObject;
        keys = transform.Find("Canvas").gameObject;
        // 初始时隐藏门和箭头
        if (door != null)
            door.SetActive(false);
        
        if (arrow != null)
            arrow.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // 如果已经按下所有按键，不需要再检测
        if (allKeysPressed)
            return;
            
        // 检测W键
        if (Input.GetKeyDown(KeyCode.W) && !pressedW)
        {
            pressedW = true;
            Debug.Log("按下W键");
        }
        
        // 检测A键
        if (Input.GetKeyDown(KeyCode.A) && !pressedA)
        {
            pressedA = true;
            Debug.Log("按下A键");
        }
        
        // 检测S键
        if (Input.GetKeyDown(KeyCode.S) && !pressedS)
        {
            pressedS = true;
            Debug.Log("按下S键");
        }
        
        // 检测D键
        if (Input.GetKeyDown(KeyCode.D) && !pressedD)
        {
            pressedD = true;
            Debug.Log("按下D键");
        }
        
        // 检查是否所有按键都已按下
        CheckAllKeysPressed();
    }
    
    private void CheckAllKeysPressed()
    {
        if (pressedW && pressedA && pressedS && pressedD && !allKeysPressed)
        {
            allKeysPressed = true;
            StartCoroutine(ShowDoorAndArrow());
            Debug.Log("所有按键已按下，显示门和箭头");
        }
    }
    
    // 显示门和箭头
    private IEnumerator ShowDoorAndArrow()
    {
        // 等待1秒
        yield return new WaitForSeconds(1f);
        // 显示门和箭头
        if (door != null)
            door.SetActive(true);
            
        if (arrow != null)
            arrow.SetActive(true);

        if (keys != null)
            keys.SetActive(false);
    }
}
