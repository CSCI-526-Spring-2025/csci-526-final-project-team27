using System.Reflection;
using UnityEngine;

public class RoomDebug : MonoBehaviour
{
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        var spawner = GetComponent<IntelligentSpawner>();
        if (spawner != null)
        {
            var roomSizeField = typeof(IntelligentSpawner).GetField("roomSize", BindingFlags.NonPublic | BindingFlags.Instance);
            if (roomSizeField != null)
            {
                Vector3 roomSize = (Vector2)roomSizeField.GetValue(spawner);
                if (roomSize != null)
                {
                    Gizmos.DrawWireCube(transform.position, roomSize);
                }
            }
        }
    }
}
