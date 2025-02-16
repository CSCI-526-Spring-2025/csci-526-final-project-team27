using UnityEngine;

public class BCsDoor : MonoBehaviour
{
    public Vector2Int direction; // 方向: (1,0) 右, (-1,0) 左, (0,1) 上, (0,-1) 下
    private bool bIsOpen = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!bIsOpen)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            RoomManager_BC.Instance.MoveTo(other.gameObject, direction);
        }
    }

    public void IsOpen(bool open)
    {
        bIsOpen = open;
    }

}
