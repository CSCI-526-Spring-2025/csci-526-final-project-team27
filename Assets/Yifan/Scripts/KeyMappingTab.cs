using UnityEngine;

public class KeyMappingTab : MonoBehaviour
{
    public GameObject keyMappingUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Show key mapping panel while holding Tab
        if (Input.GetKey(KeyCode.Tab))
        {
            if (keyMappingUI != null) keyMappingUI.SetActive(true);
        }
        else
        {
            if (keyMappingUI != null) keyMappingUI.SetActive(false);
        }
    }
}
