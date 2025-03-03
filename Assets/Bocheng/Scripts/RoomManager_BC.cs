using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

public class RoomManager_BC : MonoBehaviour
{
    [Header("地图尺寸")]
    public int gridWidth = 10;  // 地图网格宽度
    public int gridHeight = 10; // 地图网格高度
    public int minPathLength = 5; // 最短路径长度

    [Header("房间预制体")]
    public List<GameObject> normalRoomPrefabs;// 普通房间
    public List<GameObject> eliteRoomPrefabs;
    public List<GameObject> shopRoomPrefabs;
    public GameObject startRoomPrefab; // 起点房间
    public GameObject endRoomPrefab; // 终点房间

    [Header("房间大小")]
    public float roomSizeX = 2f; // 房间宽度
    public float roomSizeY = 2f; // 房间高度
    public float offset = 2f; // 额外的间隔距离

    [Header("房间生成比例")]
    [Range(0f, 1f)] public float eliteRoomRatio = 0.3f; // 精英房间比例


    [Header("玩家相机")]
    public CameraFollow cameraFollow; // **手动指定相机跟随组件**

    [Header("小地图")]
    public GameObject roomUIPrefab;
    public GameObject eliteRoomUIPrefab;
    public GameObject shopRoomUIPrefab;
    public GameObject startRoomUIPrefab;
    public GameObject endRoomUIPrefab;
    public RectTransform panel;  // 小地图的UI Panel
    public GameObject playerUIPrefab;
    private RectTransform playerUI;

    [Header("Player")]
    public PlayerMovement playerMovement;

    public float scaleRatio = 2f;  // 世界尺寸 → UI 比例
    public float UIoffset = 5f;  // 房间 UI 之间的间隔
    public float UImoveSpeed = 200f;  // 小地图移动速度

    [Header("Reward Selection UI")]
    public GameObject rewardSelectionUIPrefab; // 奖励选择界面预制体
    private RewardSelectionUI rewardSelectionUI;

    //[Header("Win UI")]
    //public GameObject winUIPrefab; // 获胜时显示的UI预制体

    public static RoomManager_BC Instance;

    private Room[,] map; // 房间网格
    private Vector2Int startRoom, endRoom; // 起点和终点
    private List<Vector2Int> roomPositions = new List<Vector2Int>(); // 已生成房间

    private Vector2Int CurrentRoom;
    private bool[,] CreatedRooms;
    private Dictionary<Vector2Int, GameObject> roomInstances = new Dictionary<Vector2Int, GameObject>(); // 存储已创建的房间
    private Dictionary<Vector2Int, RectTransform> roomUIElements = new Dictionary<Vector2Int, RectTransform>();

    private bool MinimapOpen = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }


    void Start()
    {
        GenerateMap();
        //SpawnRooms();
        InitRoom();
        InitializeMiniMap();
        RemoveMaskFromRoomUI(startRoom);
        RemoveMaskFromRoomUI(endRoom);


        CurrentRoom = startRoom;
        panel.transform.parent.gameObject.SetActive(false);

    }

    void Update()
    {
        HandleMinimapInput();
    }

    void GenerateMap()
    {
        map = new Room[gridWidth, gridHeight];
        CreatedRooms = new bool[gridWidth, gridHeight];

        // 选择起点
        startRoom = GenerateStartRoom();
        map[startRoom.x, startRoom.y] = new Room(startRoom, Room.RoomType.Start);
        roomPositions.Add(startRoom);

        // 选择终点，并确保距离够远
        /*
        do
        {
            endRoom = new Vector2Int(Random.Range(0, gridWidth), Random.Range(0, gridHeight));
        } while (Vector2Int.Distance(startRoom, endRoom) < minPathLength);
        */

        List<Vector2Int> validEndRooms = FindValidEndRooms(startRoom, minPathLength);
        if (validEndRooms.Count > 0)
        {
            endRoom = validEndRooms[Random.Range(0, validEndRooms.Count)];
            map[endRoom.x, endRoom.y] = new Room(endRoom, Room.RoomType.End);
        }
        else
        {
            Debug.LogError("无法找到合适的终点房间！");
            return;
        }

        map[endRoom.x, endRoom.y] = new Room(endRoom, Room.RoomType.End); // 预留位置

        // **第一步**: 先创建 `startRoom → endRoom` 的主路径
        CreatePath(startRoom, endRoom);

        // **第二步**: 额外生成房间（不考虑 `endRoom`）
        ExpandRooms(0.3f);

        // **确保 `endRoom` 仍然存在**
        roomPositions.Add(endRoom);

        // **第三步**: 修改房间类型
        ModifyRoomTypes();

        //debug startroom and end room
        Debug.Log($"开始房间: {startRoom}");
        Debug.Log($"结束房间: {endRoom}");
    }

    Vector2Int GenerateStartRoom()
    {
        int x = (Random.value < 0.5f) ? 0 : Random.Range(0, gridWidth);  // 50% 生成在 x=0
        int y = (x == 0) ? Random.Range(0, gridHeight) : 0;  // 如果 x=0，则 y 随机；否则 y=0
        return new Vector2Int(x, y);
    }


    List<Vector2Int> FindValidEndRooms(Vector2Int start, int minDistance)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> distances = new Dictionary<Vector2Int, int>();
        List<Vector2Int> candidates = new List<Vector2Int>();

        queue.Enqueue(start);
        distances[start] = 0;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            int currentDistance = distances[current];

            // **找到符合条件的房间**
            if (currentDistance >= minDistance)
            {
                candidates.Add(current);
            }

            foreach (Vector2Int neighbor in GetValidNeighbors(current))
            {
                if (!distances.ContainsKey(neighbor) && map[neighbor.x, neighbor.y] == null) // 只考虑未使用的房间格子
                {
                    queue.Enqueue(neighbor);
                    distances[neighbor] = currentDistance + 1;
                }
            }
        }

        return candidates;
    }


    void ModifyRoomTypes()
    {
        // Step 1: DFS 选择商人房间
        List<Vector2Int> path = DFSPathToEnd(startRoom, endRoom);
        if (path.Count > 1) // 确保路径有效
        {
            int shopRoomIndex = Random.Range(path.Count / 2, path.Count); // 1/2路径后随机选一个
            Vector2Int shopRoomPos = path[shopRoomIndex];

            if (map[shopRoomPos.x, shopRoomPos.y].Type == Room.RoomType.Normal) // 只能改普通房间
            {
                map[shopRoomPos.x, shopRoomPos.y] = new Room(shopRoomPos, Room.RoomType.Shop);
                Debug.Log($"商人房间: {shopRoomPos}");
            }
        }

        // Step 2: BFS 选择精英房间
        List<Vector2Int> eligibleEliteRooms = BFSRoomsFarFromStart(startRoom, 1);

        // 至少要有一个精英房间
        int eliteCount = Mathf.Max(1, Mathf.RoundToInt(eligibleEliteRooms.Count * eliteRoomRatio));
        HashSet<Vector2Int> chosenEliteRooms = new HashSet<Vector2Int>();

        while (chosenEliteRooms.Count < eliteCount && eligibleEliteRooms.Count > 0)
        {
            int index = Random.Range(0, eligibleEliteRooms.Count);
            Vector2Int elitePos = eligibleEliteRooms[index];

            if (map[elitePos.x, elitePos.y].Type == Room.RoomType.Normal) // 只能改普通房间
            {
                map[elitePos.x, elitePos.y] = new Room(elitePos, Room.RoomType.Elite);
                chosenEliteRooms.Add(elitePos);
                Debug.Log($"精英房间: {elitePos}");
            }
            eligibleEliteRooms.RemoveAt(index);
        }
    }


    List<Vector2Int> DFSPathToEnd(Vector2Int start, Vector2Int end)
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> parentMap = new Dictionary<Vector2Int, Vector2Int>();

        stack.Push(start);
        visited.Add(start);

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Pop();

            if (current == end)
            {
                break; // 找到终点
            }

            foreach (Vector2Int neighbor in GetValidNeighbors(current))
            {
                if (!visited.Contains(neighbor) && map[neighbor.x, neighbor.y] != null)
                {
                    stack.Push(neighbor);
                    visited.Add(neighbor);
                    parentMap[neighbor] = current;
                }
            }
        }

        // 回溯路径
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int trace = end;

        while (parentMap.ContainsKey(trace))
        {
            path.Add(trace);
            trace = parentMap[trace];
        }
        path.Add(start);
        path.Reverse();

        return path;
    }

    List<Vector2Int> BFSRoomsFarFromStart(Vector2Int start, int farLimit)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        List<Vector2Int> result = new List<Vector2Int>();

        queue.Enqueue(start);
        visited.Add(start);
        int depth = 0;

        while (queue.Count > 0)
        {
            int size = queue.Count;
            depth++;

            for (int i = 0; i < size; i++)
            {
                Vector2Int current = queue.Dequeue();

                foreach (Vector2Int neighbor in GetValidNeighbors(current))
                {
                    if (!visited.Contains(neighbor) && map[neighbor.x, neighbor.y] != null)
                    {
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);

                        // 只考虑 `距离起点 > 1` 的普通房间
                        if (depth > farLimit && map[neighbor.x, neighbor.y].Type == Room.RoomType.Normal)
                        {
                            result.Add(neighbor);
                        }
                    }
                }
            }
        }

        return result;
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

                if (map[current.x, current.y] == null) // 避免重复添加
                {
                    map[current.x, current.y] = new Room(current, Room.RoomType.Normal);
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

                if (map[neighbor.x, neighbor.y] == null && Random.value < expansionChance)
                {
                    map[neighbor.x, neighbor.y] = new Room(neighbor, Room.RoomType.Normal);
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
        //// 检查是否进入终点房间
        //if(newRoom == endRoom)
        //{
        //    WinGame();
        //    return;
        //}

        // 启用玩家控制
        EnablePlayerControllers();

        Vector3 worldPos = GetWorldPosition(newRoom);
        // **更新玩家 UI**
        UpdatePlayerUI(newRoom);
        CurrentRoom = newRoom;

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
        GameObject roomPrefab;

        if (map[newRoom.x, newRoom.y].Type == Room.RoomType.Elite)
        {
            roomPrefab = eliteRoomPrefabs[Random.Range(0, eliteRoomPrefabs.Count)];
        }
        else if (map[newRoom.x, newRoom.y].Type == Room.RoomType.Shop)
        {
            roomPrefab = shopRoomPrefabs[Random.Range(0, shopRoomPrefabs.Count)];
        }
        else // map[newRoom.x, newRoom.y].Type == Room.RoomType.Normal
        {
            roomPrefab = normalRoomPrefabs[Random.Range(0, normalRoomPrefabs.Count)];
        }

        GameObject newRoomInstance = Instantiate(roomPrefab, worldPos, Quaternion.identity);
        //newRoomInstance.transform.localScale = new Vector3(roomSizeX, roomSizeY, 1);
        Debug.Log($"Created room at {newRoom}.");

        RemoveMaskFromRoomUI(newRoom); // 移除 Mask

        // 记录房间已创建
        CreatedRooms[newRoom.x, newRoom.y] = true;
        roomInstances[newRoom] = newRoomInstance;
        DeleteDoor(newRoomInstance, newRoom);

        if (cameraFollow != null)
        {
            cameraFollow.UpdateRoomBounds(worldPos, new Vector2(roomSizeX, roomSizeY));
        }

        //EnemySpawner enemySpawner = newRoomInstance.GetComponent<EnemySpawner>();
         SimpleSpawner enemySpawner = newRoomInstance.GetComponent<SimpleSpawner>();
        if (enemySpawner != null)
        {
            DoorControl(false);
            //enemySpawner.StartSpawn();
            // enemySpawner.RoomClearEvent += DoorControl;
            enemySpawner.RoomClearEvent += OnRoomClear;

        }
    }

    void DeleteDoor(GameObject room, Vector2Int roomPos)
    {
        if (room == null) return;

        if (roomPos.x == 0 || map[roomPos.x - 1, roomPos.y] == null)
        {
            Transform leftDoor = room.transform.Find("Door_Left");
            if (leftDoor) Destroy(leftDoor.gameObject);
        }

        if (roomPos.x == gridWidth - 1 || map[roomPos.x + 1, roomPos.y] == null)
        {
            Transform rightDoor = room.transform.Find("Door_Right");
            if (rightDoor) Destroy(rightDoor.gameObject);
        }

        if (roomPos.y == 0 || map[roomPos.x, roomPos.y - 1] == null)
        {
            Transform bottomDoor = room.transform.Find("Door_Bottom");
            if (bottomDoor) Destroy(bottomDoor.gameObject);
        }

        if (roomPos.y == gridHeight - 1 || map[roomPos.x, roomPos.y + 1] == null)
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
        if (map[newRoomPos.x, newRoomPos.y] == null)
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
        //obj.transform.position = entryPoint.position;
        TeammateManager teammateManager = obj.GetComponent<TeammateManager>();
        if (teammateManager != null)
        {
            teammateManager.MoveToNextRoom(entryPoint.position, direction);
        }

        // 更新当前房间
        //CurrentRoom = newRoomPos;

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

    // 计算房间 UI 位置（相对 UI 坐标）
    private Vector2 CalculateUIPosition(Vector2Int roomPos, Vector2Int startRoom)
    {
        Vector2Int offsetPos = roomPos - startRoom; // 计算相对位置
        float x = offsetPos.x * (roomSizeX * scaleRatio + UIoffset);
        float y = offsetPos.y * (roomSizeY * scaleRatio + UIoffset); 
        return new Vector2(x, y);
    }

    private void CreateRoomUI(Vector2Int roomPos, Vector2Int startRoom)
    {
        if (roomUIElements.ContainsKey(roomPos)) return; // 防止重复创建

        Vector2 roomUIPosition = CalculateUIPosition(roomPos, startRoom);
        GameObject roomUIInstance;
        if (roomPos == startRoom)
        {
            roomUIInstance = Instantiate(startRoomUIPrefab, panel);
        }
        else if(roomPos == endRoom)
        {
            roomUIInstance = Instantiate(endRoomUIPrefab, panel);
        }
        else
        {
            if (map[roomPos.x, roomPos.y].Type == Room.RoomType.Elite)
            {
                roomUIInstance = Instantiate(eliteRoomUIPrefab, panel);
            }
            else if (map[roomPos.x, roomPos.y].Type == Room.RoomType.Shop)
            {
                roomUIInstance = Instantiate(shopRoomUIPrefab, panel);
            }
            else
            {
                roomUIInstance = Instantiate(roomUIPrefab, panel); 
            }
        }

        RectTransform roomUITransform = roomUIInstance.GetComponent<RectTransform>();
        roomUITransform.gameObject.SetActive(false); // 默认隐藏

        roomUITransform.anchoredPosition = roomUIPosition;
        roomUITransform.sizeDelta = new Vector2(roomSizeX * scaleRatio, roomSizeY * scaleRatio);
        roomUIElements.Add(roomPos, roomUITransform);
    }

    private void InitializeMiniMap()
    {
        foreach (Vector2Int roomPos in roomPositions)
        {
            CreateRoomUI(roomPos, startRoom);
        }
        // **创建玩家 UI**
        CreatePlayerUI();
    }

    private void HandleMinimapInput()
    {
        // 打开/关闭小地图
        if (Input.GetKeyDown(KeyCode.M))
        {
            if(!MinimapOpen)
            {
                panel.transform.parent.gameObject.SetActive(true);
                MinimapOpen = true;
                AdjustRoomUIPosition();
                if (playerMovement != null)
                    playerMovement.LockMove(true);
            }
            else
            {
                panel.transform.parent.gameObject.SetActive(false);
                MinimapOpen = false;
                AdjustRoomUIPosition();
                if (playerMovement != null)
                    playerMovement.LockMove(false);
            }
        }

        if(MinimapOpen)
        {
            Vector2 moveDir = Vector2.zero;

            if (Input.GetKey(KeyCode.W)) moveDir.y -= 1; // 向上 = Panel 向下移动
            if (Input.GetKey(KeyCode.S)) moveDir.y += 1; // 向下 = Panel 向上移动
            if (Input.GetKey(KeyCode.A)) moveDir.x += 1; // 向左 = Panel 向右移动
            if (Input.GetKey(KeyCode.D)) moveDir.x -= 1; // 向右 = Panel 向左移动

            panel.anchoredPosition += moveDir * UImoveSpeed * Time.deltaTime;
        }
    }

    void CreatePlayerUI()
    {
        if (playerUI != null) return; // 避免重复创建

        // **找到 startRoom 对应的 UI**
        if (!roomUIElements.ContainsKey(startRoom))
        {
            Debug.LogError("StartRoom 的 UI 不存在，无法创建 Player UI！");
            return;
        }

        RectTransform startRoomUI = roomUIElements[startRoom];

        // **生成 `playerUI`，作为 startRoom UI 的子对象**
        GameObject playerUIInstance = Instantiate(playerUIPrefab, startRoomUI);
        playerUI = playerUIInstance.GetComponent<RectTransform>();

        // **对齐位置**
        playerUI.anchoredPosition = Vector2.zero;
    }

    void UpdatePlayerUI(Vector2Int newRoom)
    {
        if (playerUI == null)
        {
            Debug.LogError("Player UI 未初始化！");
            return;
        }

        if (!roomUIElements.ContainsKey(newRoom))
        {
            Debug.LogError("目标房间 UI 不存在！");
            return;
        }

        // **更新 `playerUI` 父对象**
        RectTransform newRoomUI = roomUIElements[newRoom];
        playerUI.SetParent(newRoomUI);

        // **对齐到新房间 UI**
        playerUI.anchoredPosition = Vector2.zero;
    }


    void AdjustRoomUIPosition()
    {
        RectTransform CurrentRoomUI = roomUIElements[CurrentRoom];
        Vector2 CurrentRoomUIPosition = CurrentRoomUI.anchoredPosition;
        panel.anchoredPosition = -CurrentRoomUIPosition;
    }

    void RemoveMaskFromRoomUI(Vector2Int room)
    {
        if (!roomUIElements.ContainsKey(room))
        {
            Debug.LogError($"房间 {room} 的 UI 不存在，无法移除 Mask！");
            return;
        }

        // 获取房间对应的 UI
        RectTransform roomUI = roomUIElements[room];
        
        roomUI.gameObject.SetActive(true);
    }

    void DoorControl(bool open)
    {
        Debug.Log($"Door is: {open}");

        GameObject room = roomInstances[CurrentRoom];

        if(open)
        {
            EnemySpawner enemySpawner = room.GetComponent<EnemySpawner>();
            if (enemySpawner != null)
            {
                enemySpawner.RoomClearEvent -= DoorControl;
            }
        }

        Transform leftDoor = room.transform.Find("Door_Left");
        if(leftDoor != null)
        {
            BCsDoor door = leftDoor.GetComponent<BCsDoor>();
            door.IsOpen(open);
        }
        Transform rightDoor = room.transform.Find("Door_Right");
        if(rightDoor != null)
        {
            BCsDoor door = rightDoor.GetComponent<BCsDoor>();
            door.IsOpen(open);
        }
        Transform bottomDoor = room.transform.Find("Door_Bottom");
        if(bottomDoor != null)
        {
            BCsDoor door = bottomDoor.GetComponent<BCsDoor>();
            door.IsOpen(open);
        }
        Transform topDoor = room.transform.Find("Door_Top");
        if(topDoor != null)
        {
            BCsDoor door = topDoor.GetComponent<BCsDoor>();
            door.IsOpen(open);
        }
    }

    // 当房间敌人全部清除时的回调（RoomClearEvent 的触发）
    void OnRoomClear(bool roomCleared)
    {
        if (roomCleared)
        {
            // 禁用玩家的 ShootingController 和 SkillController
            DisablePlayerControllers();
            ShowRewardSelection();
        }
    }

    /// <summary>
    /// 显示奖励选择 UI，并锁定玩家移动
    /// </summary>
    void ShowRewardSelection()
    {
        // 如果尚未实例化，则创建奖励选择 UI 实例
        if (rewardSelectionUI == null)
        {
            // 假设场景中有 Canvas，并且奖励预制体将挂在 Canvas 下
            GameObject instance = Instantiate(rewardSelectionUIPrefab, GameObject.Find("Canvas").transform);
            rewardSelectionUI = instance.GetComponent<RewardSelectionUI>();
        }
        rewardSelectionUI.gameObject.SetActive(true);
        // 设置玩家选择奖励后的回调
        rewardSelectionUI.onRewardSelected = OnRewardChosen;
        // 锁定玩家移动，防止在奖励选择期间移动
        if (playerMovement != null)
            playerMovement.LockMove(true);
    }

    /// <summary>
    /// 玩家选择奖励后的回调，根据选择提升对应属性，然后打开房间的门
    /// </summary>
    void OnRewardChosen(RewardSelectionUI.RewardType reward)
    {
        GameObject player = playerMovement.gameObject;
        // 从玩家上获取 TeammateManager 组件，里面包含队友列表
        TeammateManager tm = player.GetComponent<TeammateManager>();

        // 如果存在队友管理器，则先清理掉已销毁的队友引用
        if (tm != null)
        {
            for (int i = tm.teammates.Count - 1; i >= 0; i--)
            {
                if (tm.teammates[i] == null)
                {
                    tm.teammates.RemoveAt(i);
                }
            }
        }

        switch (reward)
        {
            case RewardSelectionUI.RewardType.Health:
                {
                    // 玩家加血
                    Health_BC playerHealth = player.GetComponent<Health_BC>();
                    if (playerHealth != null)
                    {
                        playerHealth.maxHealth += 5;
                        Debug.Log("Player HP+5, new maxHealth: " + playerHealth.maxHealth);
                    }
                    else
                    {
                        Debug.LogError("在玩家身上没有找到 Health_BC 组件！");
                    }

                    // 队友加血
                    if (tm != null)
                    {
                        foreach (GameObject teammate in tm.teammates)
                        {
                            if (teammate == null)
                                continue;

                            Health_BC teammateHealth = teammate.GetComponent<Health_BC>();
                            if (teammateHealth != null)
                            {
                                teammateHealth.maxHealth += 5;
                                Debug.Log("Teammate HP+5, new maxHealth: " + teammateHealth.maxHealth);
                            }
                            else
                            {
                                Debug.LogError("在队友身上没有找到 Health_BC 组件！");
                            }
                        }
                    }
                }
                break;

            case RewardSelectionUI.RewardType.Attack:
                {
                    // 玩家获得“增加治疗量”的效果
                    ShootingController playerShooting = player.GetComponent<ShootingController>();
                    if (playerShooting != null)
                    {
                        HealBallVond playerBullet = playerShooting.bulletPrefab.GetComponent<HealBallVond>();
                        if (playerBullet != null)
                        {
                            // 增加治疗量，例如每次加 1
                            playerBullet.healAmount += 1;
                            Debug.Log("Player's heal amount increased, new healAmount: " + playerBullet.healAmount);
                        }
                        else
                        {
                            Debug.LogError("玩家子弹预制体上未找到 HealBallVond 组件！");
                        }
                    }
                    else
                    {
                        Debug.LogError("Player身上没有找到 ShootingController 组件！");
                    }

                    // 队友获得“增加伤害”的效果（针对近战队友）
                    if (tm != null)
                    {
                        foreach (GameObject teammate in tm.teammates)
                        {
                            if (teammate == null)
                                continue;

                            MeleeTeammate melee = teammate.GetComponent<MeleeTeammate>();
                            if (melee != null)
                            {
                                // 增加伤害，例如每次加 1
                                melee.damage += 1;
                                Debug.Log("Teammate's melee damage increased, new damage: " + melee.damage);
                            }
                            else
                            {
                                // 如果没有 MeleeTeammate，则检查是否有 RangedTeammate 组件
                                RangedTeammate range = teammate.GetComponent<RangedTeammate>();
                                if (range != null)
                                {
                                    if (range.bulletPrefab != null)
                                    {
                                        DamageBallTeam teamBullet = range.bulletPrefab.GetComponent<DamageBallTeam>();
                                        if (teamBullet != null)
                                        {
                                            teamBullet.damageAmount += 1;
                                            Debug.Log("Range teammate damage increased, new damage: " + teamBullet.damageAmount);
                                        }
                                        else
                                        {
                                            Debug.LogError("队友子弹预制体上未找到 DamageBallTeam 组件！");
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogError("队友 RangeTeammate 的 bulletPrefab 为空！");
                                    }
                                }
                                else
                                {
                                    Debug.LogError("队友没有找到 MeleeTeammate 或 RangedTeammate 组件！");
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("玩家身上没有找到 TeammateManager 组件，无法给予队友奖励。");
                    }
                }
                break;

            case RewardSelectionUI.RewardType.Speed:
                {
                    // 玩家增加移动速度
                    PlayerMovement pm = player.GetComponent<PlayerMovement>();
                    if (pm != null)
                    {
                        pm.moveSpeed += 1;  // 例如，每次加 1
                        Debug.Log("Player Speed increased, new moveSpeed: " + pm.moveSpeed);
                    }
                    else
                    {
                        Debug.LogError("在玩家身上没有找到 PlayerMovement 组件！");
                    }

                    // 队友增加移动速度
                    if (tm != null)
                    {
                        foreach (GameObject teammate in tm.teammates)
                        {
                            if (teammate == null)
                                continue;

                            // 优先检查是否有 MeleeTeammate 组件
                            MeleeTeammate melee = teammate.GetComponent<MeleeTeammate>();
                            if (melee != null)
                            {
                                melee.moveSpeed += 1;
                                Debug.Log("Melee teammate speed increased, new moveSpeed: " + melee.moveSpeed);
                            }
                            else
                            {
                                // 如果没有 MeleeTeammate，则检查是否有 RangedTeammate 组件
                                RangedTeammate range = teammate.GetComponent<RangedTeammate>();
                                if (range != null)
                                {
                                    range.moveSpeed += 1;
                                    Debug.Log("Range teammate speed increased, new moveSpeed: " + range.moveSpeed);
                                }
                                else
                                {
                                    Debug.LogError("队友没有找到 MeleeTeammate 或 RangedTeammate 组件！");
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("玩家身上没有找到 TeammateManager 组件，无法给予队友速度奖励。");
                    }
                }
                break;
        }

        // 解锁玩家移动
        if (playerMovement != null)
            playerMovement.LockMove(false);

        // 打开当前房间的所有门
        DoorControl(true);
    }


    // 新增：禁用玩家的 ShootingController 和 SkillController
    void DisablePlayerControllers()
    {
        //获取tag为Player的对象
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            ShootingController shootingController = player.GetComponent<ShootingController>();

            if (shootingController != null)
            {
                shootingController.bulletLifetime = 0;
                Debug.Log("ShootingController disabled.");
            }
            SkillController skillController = player.GetComponent<SkillController>();
            if (skillController != null)
            {
                skillController.enabled = false;
                Debug.Log("SkillController disabled.");
            }
        }
    }

    // 新增：启用玩家的 ShootingController 和 SkillController
    void EnablePlayerControllers()
    {

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player = playerMovement.gameObject;
            ShootingController shootingController = player.GetComponent<ShootingController>();
            if (shootingController != null)
            {
                shootingController.bulletLifetime = 2; //临时解决方案
                Debug.Log("ShootingController enabled.");
            }
            SkillController skillController = player.GetComponent<SkillController>();
            if (skillController != null)
            {
                skillController.enabled = true;
                Debug.Log("SkillController enabled.");
            }
        }
    }


    //void WinGame()
    //{
    //    Debug.Log("You Win!");

    //    // 显示 "You Win" UI
    //    if (winUIPrefab != null)
    //    {
    //        Instantiate(winUIPrefab, GameObject.Find("Canvas").transform);
    //    }
    //    else
    //    {
    //        Debug.LogError("winUIPrefab 未在 Inspector 中赋值！");
    //    }

    //    // 禁用玩家移动（如果需要）
    //    if (playerMovement != null)
    //        playerMovement.LockMove(true);
    //}




}
