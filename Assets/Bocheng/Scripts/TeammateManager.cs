using System.Collections.Generic;
using UnityEngine;

public class TeammateManager : MonoBehaviour
{
    public List<GameObject> teammates = new List<GameObject>(); // 队友列表
    private Dictionary<GameObject, Vector2> relativePositions = new Dictionary<GameObject, Vector2>(); // 记录相对位置
    private Vector2 defaultDirection = new Vector2(0, -1); // 默认方向（向下）

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

    // 计算新位置并移动队友
    public void MoveToNextRoom(Vector2 newPosition, Vector2 newDirection)
    {
        foreach (var entry in relativePositions)
        {
            GameObject teammate = entry.Key;
            Vector2 oldRelativePos = entry.Value;
            Vector2 newRelativePos = RotateRelativePosition(oldRelativePos, newDirection);

            // 设置队友的新位置
            teammate.transform.position = newPosition + newRelativePos;
        }

        // 更新玩家位置
        transform.position = newPosition;
    }

    // 旋转相对位置
    Vector2 RotateRelativePosition(Vector2 relativePos, Vector2 newDirection)
    {
        float dx = relativePos.x;
        float dy = relativePos.y;

        // 旋转逻辑
        if (newDirection == Vector2.up) return new Vector2(-dx, -dy); // 向上
        if (newDirection == Vector2.right) return new Vector2(-dy, dx);  // 向右
        if (newDirection == Vector2.left) return new Vector2(dy, -dx);  // 向左
        return new Vector2(dx, dy); // 向下 (默认)
    }
}
