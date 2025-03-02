using UnityEngine;

public class Room
{
    public Vector2Int Position { get; private set; }
    public RoomType Type { get; private set; }
    public enum RoomType { Normal, Start, End, Elite, Shop }

    public Room(Vector2Int position, RoomType type)
    {
        Position = position;
        Type = type;
    }
}
