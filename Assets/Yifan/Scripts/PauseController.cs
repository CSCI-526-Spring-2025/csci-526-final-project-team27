using UnityEngine;

public class PauseController : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameObject keyMappingPanel;
    public static bool isPaused = false; // Shared variable

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Esc Pressed");
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        keyMappingPanel.SetActive(false); // Hide key mapping if open
        Time.timeScale = 1f;
        isPaused = false; // Update the shared variable
    }

    public void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true; // Update the shared variable
    }
}
