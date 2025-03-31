using UnityEngine;
using System.Collections;

public class TutorialStart : MonoBehaviour
{
    private GameObject door;
    private GameObject arrow;
    private GameObject keys;
    private GameObject player;
    private GameObject skillPanel;

    private bool unlocked = false;

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
        
        // 禁用玩家射击
        player = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<ShootingController>().enabled = false;
        // 禁用技能面板
        player.GetComponent<SkillController>().enabled = false;
    }

    void Update()
    {
        // 如果已经解锁，则不再检测按键
        if (unlocked)
            return;

        // 按下W、A、S、D任意一个就触发解锁
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
        {
            unlocked = true;
            Debug.Log("按下任意一个 WASD 键，解锁门和箭头");
            StartCoroutine(ShowDoorAndArrow());
        }
    }
    
    // 显示门和箭头
    private IEnumerator ShowDoorAndArrow()
    {
        
        yield return new WaitForSeconds(2f);
        // 显示门和箭头
        if (door != null)
            door.SetActive(true);
            
        if (arrow != null)
            arrow.SetActive(true);

        if (keys != null)
            keys.SetActive(false);
    }
}
