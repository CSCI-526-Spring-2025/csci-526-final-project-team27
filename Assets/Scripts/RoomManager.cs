using UnityEngine;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
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

    private bool[,] map; // 房间网格
    private Vector2Int startRoom, endRoom; // 起点和终点
    private List<Vector2Int> roomPositions = new List<Vector2Int>(); // 已生成房间

    void Start()
    {
        GenerateMap();
        SpawnRooms();
    }

    void GenerateMap()
    {
        map = new bool[gridWidth, gridHeight];

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
    }
}
