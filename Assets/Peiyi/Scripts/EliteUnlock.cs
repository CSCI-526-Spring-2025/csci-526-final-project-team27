using UnityEngine;
using System.Collections;
using TMPro;

public class NewMonoBehaviourScript : MonoBehaviour
{
    private bool unlocked = false;

   
    

    void Start()
    {
        
        SimpleSpawner spawner = GetComponent<SimpleSpawner>();
        if (spawner != null)
        {
            spawner.RoomClearEvent += OnRoomCleared;
        }
    }

    void OnRoomCleared(bool success)
    {
        if (!unlocked && success)
        {
            unlocked = true;

            int nextIndex = SkillUiExchanger.Instance.unlockedSkillIndeics.Count;
            Debug.Log($"Elite room cleared! Unlocking skill index {nextIndex}");

            SkillUiExchanger.Instance.UnlockSkill(nextIndex);

            SkillUiExchanger.Instance.elitecanva.SetActive(true);
            

            StartCoroutine(Cooldown());
            
           
        }
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(3f);
        SkillUiExchanger.Instance.elitecanva.SetActive(false);
        yield return new WaitForSeconds(1f);
        SkillUiExchanger.Instance.skillExchangeCanva.SetActive(true);
        SkillUiExchanger.Instance.UpdateCurrentSkillDisplay();
    }
}

