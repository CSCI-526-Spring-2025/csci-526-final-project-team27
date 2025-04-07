using System.Reflection;
using UnityEngine;

public class RoomDebug : MonoBehaviour
{
    void Start(){
        SkillcontrollerUI.Instance.ShowSkillUI();
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        var spawner = GetComponent<SimpleSpawner>();
        if (spawner != null)
        {
            var roomSizeField = typeof(SimpleSpawner).GetField("roomSize", BindingFlags.NonPublic | BindingFlags.Instance);
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
