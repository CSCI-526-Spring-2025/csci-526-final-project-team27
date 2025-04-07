using UnityEngine;

public class SkillcontrollerUI : MonoBehaviour
{
    public static SkillcontrollerUI Instance;
    private GameObject panel;
    private GameObject player;

    void Awake()
    {
        Instance = this;
        panel = GameObject.Find("Panel");
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void HideSkillUI()
    {
        if (panel == null || player == null) return;

        panel.transform.Find("Skill1")?.gameObject.SetActive(false);
        panel.transform.Find("Skill2")?.gameObject.SetActive(false);
        panel.transform.Find("Skill3")?.gameObject.SetActive(false);
        panel.gameObject.SetActive(false);
        player.GetComponent<SkillController>().enabled = false;

    }

    public void ShowSkillUI()
    {
        if (panel == null || player == null) return;

        panel.transform.Find("Skill1")?.gameObject.SetActive(true);
        panel.transform.Find("Skill2")?.gameObject.SetActive(true);
        panel.transform.Find("Skill3")?.gameObject.SetActive(true);
        panel.gameObject.SetActive(true);
        player.GetComponent<SkillController>().enabled = true;
    }
}