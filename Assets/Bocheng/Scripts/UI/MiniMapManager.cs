using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MiniMapManager : MonoBehaviour
{
    public RoomManager_BC roomManager;  // 绑定房间管理器
    public GameObject roomUIPrefab;  // 房间UI的预制体
    public RectTransform panel;  // 小地图的UI Panel

    public float scaleRatio = 2f;  // 世界尺寸 → UI 比例
    public float offset = 5f;  // 房间 UI 之间的间隔
    public float moveSpeed = 200f;  // 小地图移动速度

    private Dictionary<Vector2Int, RectTransform> roomUIElements = new Dictionary<Vector2Int, RectTransform>();

    void Start()
    {
        if (roomManager == null)
        {
            Debug.LogError("MiniMapManager: RoomManager is not assigned!");
            return;
        }

        InitializeMiniMap();
    }

    void Update()
    {
        HandleMiniMapMovement();
    }

    // 初始化小地图
    private void InitializeMiniMap()
    {
        /*
        Vector2Int startRoom = roomManager.startRoom;

        foreach (Vector2Int roomPos in roomManager.roomPositions)
        {
            CreateRoomUI(roomPos, startRoom);
        }*/
    }

    // 创建房间 UI
    private void CreateRoomUI(Vector2Int roomPos, Vector2Int startRoom)
    {
        if (roomUIElements.ContainsKey(roomPos)) return; // 防止重复创建

        Vector2 roomUIPosition = CalculateUIPosition(roomPos, startRoom);
        GameObject roomUIInstance = Instantiate(roomUIPrefab, panel);
        RectTransform roomUITransform = roomUIInstance.GetComponent<RectTransform>();

        roomUITransform.anchoredPosition = roomUIPosition;

        roomUIElements.Add(roomPos, roomUITransform);
    }

    // 计算房间 UI 位置（相对 UI 坐标）
    private Vector2 CalculateUIPosition(Vector2Int roomPos, Vector2Int startRoom)
    {
        Vector2Int offsetPos = roomPos - startRoom; // 计算相对位置
        float x = offsetPos.x * (roomManager.roomSizeX * scaleRatio + offset);
        float y = offsetPos.y * (roomManager.roomSizeY * scaleRatio + offset); 
        return new Vector2(x, y);
    }

    // 处理小地图移动
    private void HandleMiniMapMovement()
    {
        Vector3 moveDir = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) moveDir.y += 1; // 向上 = Panel 向下移动
        if (Input.GetKey(KeyCode.S)) moveDir.y -= 1; // 向下 = Panel 向上移动
        if (Input.GetKey(KeyCode.A)) moveDir.x += 1; // 向左 = Panel 向右移动
        if (Input.GetKey(KeyCode.D)) moveDir.x -= 1; // 向右 = Panel 向左移动

        //panel.anchoredPosition += moveDir * moveSpeed * Time.deltaTime;
    }
}
