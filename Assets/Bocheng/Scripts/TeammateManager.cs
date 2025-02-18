using System.Collections.Generic;
using UnityEngine;

public class TeammateManager : MonoBehaviour
{
    public List<GameObject> teammates = new List<GameObject>(); // 队友列表
    private Dictionary<GameObject, Vector2> relativePositions = new Dictionary<GameObject, Vector2>(); // 记录相对位置
    private Vector2 defaultDirection = new Vector2(0, -1); // 默认方向（向下）
    public GameObject gameOverUI; // 游戏结束 UI

    void Start()
    {
        RecordRelativePositions();
    }

    // 记录队友相对玩家的初始位置
    void RecordRelativePositions()
    {
        foreach (GameObject teammate in teammates)
        {
            if (teammate != null)
            {
                Vector2 relativePos = (Vector2)teammate.transform.position - (Vector2)transform.position;
                relativePositions[teammate] = relativePos;
            }
        }
    }

    // 当队友被销毁时，调用此方法更新管理器信息
    public void RemoveTeammate(GameObject teammate)
    {
        if (teammate == null) return;

        if (teammates.Contains(teammate))
        {
            teammates.Remove(teammate);
        }

        if (relativePositions.ContainsKey(teammate))
        {
            relativePositions.Remove(teammate);
        }

        if (teammates.Count == 0)
        {

            // 显示UI
            Debug.Log("游戏结束");
            gameOverUI = Instantiate(gameOverUI);

        }
    }

    // 计算新位置并移动队友
    public void MoveToNextRoom(Vector2 newPosition, Vector2 newDirection)
    {
        // 遍历时先复制一份键列表，防止在遍历过程中字典被修改
        List<GameObject> keys = new List<GameObject>(relativePositions.Keys);
        foreach (GameObject teammate in keys)
        {
            // 如果该队友已被销毁则跳过
            if (teammate == null)
            {
                relativePositions.Remove(teammate);
                continue;
            }

            Vector2 oldRelativePos = relativePositions[teammate];
            Vector2 newRelativePos = RotateRelativePosition(oldRelativePos, newDirection);

            // 设置队友的新位置
            teammate.transform.position = newPosition + newRelativePos;
        }

        // 更新玩家位置
        transform.position = newPosition;
    }

    // 根据新方向旋转相对位置
    Vector2 RotateRelativePosition(Vector2 relativePos, Vector2 newDirection)
    {
        float dx = relativePos.x;
        float dy = relativePos.y;

        if (newDirection == Vector2.up)
            return new Vector2(-dx, -dy); // 向上
        if (newDirection == Vector2.right)
            return new Vector2(-dy, dx);  // 向右
        if (newDirection == Vector2.left)
            return new Vector2(dy, -dx);  // 向左
        return new Vector2(dx, dy);       // 向下 (默认)
    }
}
