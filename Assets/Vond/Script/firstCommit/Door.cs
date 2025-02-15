using UnityEngine;

public class Door : MonoBehaviour
{
    public Vector2Int direction; // 方向: (1,0) 右, (-1,0) 左, (0,1) 上, (0,-1) 下

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            RoomManager.Instance.ChangeRoom(RoomManager.Instance.currentRoomPosition + direction);
            other.transform.position += new Vector3(direction.x * 9, direction.y * 9, 0);
        }
    }
}
