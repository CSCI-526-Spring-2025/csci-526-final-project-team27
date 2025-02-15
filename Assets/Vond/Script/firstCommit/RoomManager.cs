using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;
    
    public GameObject roomPrefab; // 房间预制体
    private Dictionary<Vector2Int, Room> rooms = new Dictionary<Vector2Int, Room>();
    
    public Vector2Int currentRoomPosition;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void CreateRoom(Vector2Int position)
    {
        if (rooms.ContainsKey(position)) return;

        GameObject newRoom = Instantiate(roomPrefab, new Vector3(position.x * 35, position.y * 20, 0), Quaternion.identity);
        Room room = newRoom.GetComponent<Room>();
        room.position = position;
        rooms.Add(position, room);
    }

    public void ChangeRoom(Vector2Int newPosition)
    {
        if (!rooms.ContainsKey(newPosition)) CreateRoom(newPosition);
        currentRoomPosition = newPosition;
        FindAnyObjectByType<MiniMap>().UpdateMap(newPosition);
    }

    private void Start()
    {
        CreateRoom(Vector2Int.zero); // 在 (0,0) 生成第一个房间
        Debug.Log("房间数量：" + rooms.Count);
    }

}
