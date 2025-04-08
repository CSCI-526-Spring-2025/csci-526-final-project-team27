using UnityEngine;

public class BossRoom : MonoBehaviour
{
    public Transform RoomTransform;
    public GameObject bossPrefab;
    public GameObject WinUI;

    private bool isBossSpawned = false;
    private GameObject boss;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // on trigger enter
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if(!isBossSpawned)
            {
                SpawnBoss();
                RoomManager_BC.Instance.DoorControl(false); 
                isBossSpawned = true;
            }
        }
    }

    void SpawnBoss()
    {
        // Instantiate the boss prefab at the center of the room
        Vector3 spawnPosition = RoomTransform.position;
        boss = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
        boss.GetComponent<Temp_Boss>().SetRoomInfo(RoomTransform.position, RoomTransform.localScale);
        boss.GetComponent<EnemyHealth>().OnDeath.AddListener(BossDie);
        //debug boss name
        Debug.Log("Boss spawned: " + boss.name);
    }

    void BossDie(GameObject boss)
    {
        if(boss == null)
        {
            Debug.Log("Boss is already dead");
            return;
        }

        RoomManager_BC.Instance.DoorControl(true);
        // Show the win UI
        if(WinUI != null)
        {
            WinUI.SetActive(true);
        }
    }

    public void CloseUI()
    {
        if(WinUI != null)
        {
            WinUI.SetActive(false);
        }
    }
}
