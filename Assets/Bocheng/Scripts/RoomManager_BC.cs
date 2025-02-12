using UnityEngine;
using System.Collections.Generic;

public class RoomManager_BC : MonoBehaviour
{
    [Header("地图尺寸")]
    public int gridWidth = 10;  // 地图网格宽度
    public int gridHeight = 10; // 地图网格高度
    public int minPathLength = 5; // 最短路径长度

    [Header("房间预制体")]
    public GameObject roomPrefab; // 普通房间
    public GameObject startRoomPrefab; // 起点房间
    public GameObject endRoomPrefab; // 终点房间

    [Header("房间大小")]
    public float roomSizeX = 2f; // 房间宽度
    public float roomSizeY = 2f; // 房间高度
    public float offset = 2f; // 额外的间隔距离

    [Header("玩家相机")]
    public CameraFollow cameraFollow; // **手动指定相机跟随组件**

    public static RoomManager_BC Instance;

    private bool[,] map; // 房间网格
    private Vector2Int startRoom, endRoom; // 起点和终点
    private List<Vector2Int> roomPositions = new List<Vector2Int>(); // 已生成房间

    private Vector2Int CurrentRoom;
    private bool[,] CreatedRooms;
    private Dictionary<Vector2Int, GameObject> roomInstances = new Dictionary<Vector2Int, GameObject>(); // 存储已创建的房间

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }


    void Start()
    {
        GenerateMap();
        //SpawnRooms();
        InitRoom();
        CurrentRoom = startRoom;
        Debug.Log(startRoom);
    }

    void GenerateMap()
    {
        map = new bool[gridWidth, gridHeight];
        CreatedRooms = new bool[gridWidth, gridHeight];

        // 选择起点
        startRoom = new Vector2Int(Random.Range(0, gridWidth), Random.Range(0, gridHeight));
        map[startRoom.x, startRoom.y] = true;
        roomPositions.Add(startRoom);

        // 选择终点，并确保距离够远
        do
        {
            endRoom = new Vector2Int(Random.Range(0, gridWidth), Random.Range(0, gridHeight));
        } while (Vector2Int.Distance(startRoom, endRoom) < minPathLength);

        map[endRoom.x, endRoom.y] = true; // 预留位置

        // **第一步**: 先创建 `startRoom → endRoom` 的主路径
        CreatePath(startRoom, endRoom);

        // **第二步**: 额外生成房间（不考虑 `endRoom`）
        ExpandRooms(0.3f);

        // **确保 `endRoom` 仍然存在**
        roomPositions.Add(endRoom);

        //debug startroom and end room
        Debug.Log(startRoom);
        Debug.Log(endRoom);
    }

    void CreatePath(Vector2Int start, Vector2Int end)
    {
        Vector2Int current = start;

        while (current != end)
        {
            List<Vector2Int> possibleMoves = new List<Vector2Int>();

            if (current.x > end.x) possibleMoves.Add(Vector2Int.left);
            if (current.x < end.x) possibleMoves.Add(Vector2Int.right);
            if (current.y > end.y) possibleMoves.Add(Vector2Int.down);
            if (current.y < end.y) possibleMoves.Add(Vector2Int.up);

            if (possibleMoves.Count > 0)
            {
                Vector2Int move = possibleMoves[Random.Range(0, possibleMoves.Count)];
                current += move;

                if (!map[current.x, current.y]) // 避免重复添加
                {
                    map[current.x, current.y] = true;
                    roomPositions.Add(current);
                }
            }
        }
    }

    void ExpandRooms(float expansionChance)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        // 从 `startRoom` 开始拓展（**不包含 `endRoom`**）
        foreach (Vector2Int pos in roomPositions)
        {
            if (pos != endRoom)
                queue.Enqueue(pos);
        }

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            List<Vector2Int> neighbors = GetValidNeighbors(current);

            foreach (Vector2Int neighbor in neighbors)
            {
                if (neighbor == endRoom) continue; // 跳过终点

                if (!map[neighbor.x, neighbor.y] && Random.value < expansionChance)
                {
                    map[neighbor.x, neighbor.y] = true;
                    roomPositions.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
    }

    List<Vector2Int> GetValidNeighbors(Vector2Int room)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        if (room.x > 0) neighbors.Add(new Vector2Int(room.x - 1, room.y));
        if (room.x < gridWidth - 1) neighbors.Add(new Vector2Int(room.x + 1, room.y));
        if (room.y > 0) neighbors.Add(new Vector2Int(room.x, room.y - 1));
        if (room.y < gridHeight - 1) neighbors.Add(new Vector2Int(room.x, room.y + 1));

        return neighbors;
    }

    /*
    void SpawnRooms()
    {
        foreach (Vector2Int pos in roomPositions)
        {
            // 计算世界坐标，房间紧密排列
            Vector3 worldPos = new Vector3(pos.x * roomSizeX, pos.y * roomSizeY, 0);
            GameObject prefab = roomPrefab;

            if (pos == startRoom) prefab = startRoomPrefab;
            if (pos == endRoom) prefab = endRoomPrefab;

            GameObject spawnedRoom = Instantiate(prefab, worldPos, Quaternion.identity);
            spawnedRoom.transform.localScale = new Vector3(roomSizeX, roomSizeY, 1);
        }
    }*/
    void InitRoom()
    {
        // 创建起点房间
        Vector3 startWorldPos = new Vector3(0, 0, 0);
        GameObject startRoomInstance = Instantiate(startRoomPrefab, startWorldPos, Quaternion.identity);
        //startRoomInstance.transform.localScale = new Vector3(roomSizeX, roomSizeY, 1);
        roomInstances[startRoom] = startRoomInstance;
        //debug startroominstance

        CreatedRooms[startRoom.x, startRoom.y] = true;
        DeleteDoor(startRoomInstance, startRoom);

        
        // 计算终点房间的世界坐标
        Vector3 endWorldPos = GetWorldPosition(endRoom);
        GameObject endRoomInstance = Instantiate(endRoomPrefab, endWorldPos, Quaternion.identity);
        //endRoomInstance.transform.localScale = new Vector3(roomSizeX, roomSizeY, 1);
        roomInstances[endRoom] = endRoomInstance;
        CreatedRooms[endRoom.x, endRoom.y] = true;
        DeleteDoor(endRoomInstance, endRoom);
        
    }

    Vector3 GetWorldPosition(Vector2Int room)
    {
        // 计算房间相对 `startRoom` 的偏移
        Vector2Int offsetRoom = room - startRoom;
        return new Vector3(offsetRoom.x * (roomSizeX + offset), offsetRoom.y * (roomSizeY + offset), 0);
    }

    

    public void ChangeRoom(Vector2Int newRoom)
    {
        Vector3 worldPos = GetWorldPosition(newRoom);

        if (CreatedRooms[newRoom.x, newRoom.y])
        {
            // 房间已创建，执行 "do something" 逻辑
            Debug.Log($"Room at {newRoom} already exists. Do something.");

            if (cameraFollow != null)
            {
                cameraFollow.UpdateRoomBounds(worldPos, new Vector2(roomSizeX, roomSizeY));
            }

            return;
        }

        // 房间未创建，则生成
        //Vector3 worldPos = GetWorldPosition(newRoom);
        GameObject newRoomInstance = Instantiate(roomPrefab, worldPos, Quaternion.identity);
        //newRoomInstance.transform.localScale = new Vector3(roomSizeX, roomSizeY, 1);
        Debug.Log($"Created room at {newRoom}.");

        // 记录房间已创建
        CreatedRooms[newRoom.x, newRoom.y] = true;
        roomInstances[newRoom] = newRoomInstance;
        DeleteDoor(newRoomInstance, newRoom);

        if (cameraFollow != null)
        {
            cameraFollow.UpdateRoomBounds(worldPos, new Vector2(roomSizeX, roomSizeY));
        }
    }

    void DeleteDoor(GameObject room, Vector2Int roomPos)
    {
        if (room == null) return;

        if (roomPos.x == 0 || !map[roomPos.x - 1, roomPos.y])
        {
            Transform leftDoor = room.transform.Find("Door_Left");
            if (leftDoor) Destroy(leftDoor.gameObject);
        }

        if (roomPos.x == gridWidth - 1 || !map[roomPos.x + 1, roomPos.y])
        {
            Transform rightDoor = room.transform.Find("Door_Right");
            if (rightDoor) Destroy(rightDoor.gameObject);
        }

        if (roomPos.y == 0 || !map[roomPos.x, roomPos.y - 1])
        {
            Transform bottomDoor = room.transform.Find("Door_Bottom");
            if (bottomDoor) Destroy(bottomDoor.gameObject);
        }

        if (roomPos.y == gridHeight - 1 || !map[roomPos.x, roomPos.y + 1])
        {
            Transform topDoor = room.transform.Find("Door_Top");
            if (topDoor) Destroy(topDoor.gameObject);
        }
    }

    public void MoveTo(GameObject obj, Vector2Int direction)
    {
        if (obj == null) return;

        // 计算新房间的坐标
        Vector2Int newRoomPos = CurrentRoom + direction;

        Debug.Log($"Moving to {newRoomPos}.");

        // 确保新房间在地图范围内
        if (newRoomPos.x < 0 || newRoomPos.x >= gridWidth || newRoomPos.y < 0 || newRoomPos.y >= gridHeight)
        {
            Debug.Log("Invalid move: Out of bounds.");
            return;
        }

        // 确保目标房间存在
        if (!map[newRoomPos.x, newRoomPos.y])
        {
            Debug.Log("Invalid move: No room in that direction.");
            return;
        }

        // 创建或获取房间
        ChangeRoom(newRoomPos); // 创建新房间

        // 获取新房间
        GameObject newRoom = roomInstances[newRoomPos];

        if (newRoom == null)
        {
            Debug.LogError($"Failed to retrieve the new room at {newRoomPos}.");
            return;
        }

        // 计算进入新房间的入口点
        string entryPointName = GetEntryPointName(direction);
        Transform entryPoint = newRoom.transform.Find(entryPointName);

        if (entryPoint == null)
        {
            Debug.LogError($"Entry point {entryPointName} not found in new room!");
            return;
        }

        // 移动物体到新房间的入口点
        obj.transform.position = entryPoint.position;

        // 更新当前房间
        CurrentRoom = newRoomPos;

        Debug.Log($"Moved to {newRoomPos}, entered through {entryPointName}.");
    }

    // 获取进入新房间的入口点名称
    private string GetEntryPointName(Vector2Int direction)
    {
        if (direction == Vector2Int.up) return "InPoint_Bottom";    // 从下方进入
        if (direction == Vector2Int.down) return "InPoint_Top";     // 从上方进入
        if (direction == Vector2Int.left) return "InPoint_Right";   // 从右侧进入
        if (direction == Vector2Int.right) return "InPoint_Left";   // 从左侧进入
        return "";
    }
}
