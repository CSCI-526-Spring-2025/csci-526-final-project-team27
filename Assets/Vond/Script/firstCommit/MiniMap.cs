// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// public class MiniMap : MonoBehaviour
// {
//     public GameObject roomIconPrefab; // 小地图房间图标
//     private Dictionary<Vector2Int, GameObject> roomIcons = new Dictionary<Vector2Int, GameObject>();

//     public Color exploredColor = Color.white; // 已探索房间颜色
//     public Color currentColor = Color.yellow; // 当前房间颜色

//     private Vector2Int currentRoom;

//     public void UpdateMap(Vector2Int newRoomPosition)
//     {
//         currentRoom = newRoomPosition;

//         // 生成新房间图标
//         if (!roomIcons.ContainsKey(newRoomPosition))
//         {
//             GameObject newIcon = Instantiate(roomIconPrefab, transform);
//             newIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(newRoomPosition.x * 20, newRoomPosition.y * 20);
//             roomIcons.Add(newRoomPosition, newIcon);
//         }

//         // 更新所有房间颜色
//         foreach (var room in roomIcons)
//         {
//             room.Value.GetComponent<Image>().color = (room.Key == currentRoom) ? currentColor : exploredColor;
//         }
//     }
// }

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMap : MonoBehaviour
{
    public GameObject roomIconPrefab; // 小地图房间图标
    private Dictionary<Vector2Int, GameObject> roomIcons = new Dictionary<Vector2Int, GameObject>();

    public Color exploredColor = Color.white; // 已探索房间颜色
    public Color currentColor = Color.yellow; // 当前房间颜色

    private Vector2Int currentRoom; // 记录当前房间的位置
    private Vector2Int minPosition = Vector2Int.zero; // 记录最左下角位置
    private Vector2Int maxPosition = Vector2Int.zero; // 记录最右上角位置

    public void UpdateMap(Vector2Int newRoomPosition)
    {
        // 计算小地图的房间偏移
        Vector2Int offset = newRoomPosition - currentRoom;
        currentRoom = newRoomPosition;

        // 更新最小和最大坐标
        minPosition = Vector2Int.Min(minPosition, currentRoom);
        maxPosition = Vector2Int.Max(maxPosition, currentRoom);

        // 生成新房间图标
        if (!roomIcons.ContainsKey(newRoomPosition))
        {
            GameObject newIcon = Instantiate(roomIconPrefab, transform);
            roomIcons.Add(newRoomPosition, newIcon);
        }

        // 重新调整所有房间的UI位置
        foreach (var room in roomIcons)
        {
            Vector2Int relativePosition = room.Key - minPosition; // 计算相对位置
            room.Value.GetComponent<RectTransform>().anchoredPosition = new Vector2(relativePosition.x * 20, relativePosition.y * 20);
        }

        // 更新所有房间颜色
        foreach (var room in roomIcons)
        {
            room.Value.GetComponent<Image>().color = (room.Key == currentRoom) ? currentColor : exploredColor;
        }
    }
}
