using UnityEngine;

public class TutorialSkill : MonoBehaviour
{
    private GameObject[] doors;

    private GameObject player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private enum TutorialState
    {
        Skill1,
        Skill2,
        Skill3,
        Completed
    }

    private TutorialState currentState;

    void Start()
    {
        doors = new GameObject[2];
        doors[0] = transform.Find("Door_Left").gameObject;
        doors[1] = transform.Find("Door_Right").gameObject;
        foreach (GameObject door in doors)
        {
            door.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
