using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI; // Reference to Pause Menu UI
    public GameObject keyMappingPanel; // Reference to Key Mapping Panel
    // private bool isPaused = false;

    void Update()
    {
        // Detect Esc key press to toggle pause
        // if (Input.GetKeyDown(KeyCode.Escape))
        // {
        //     Debug.Log("Esc Pressed");
        //     if (isPaused)
        //         ResumeGame();
        //     else
        //         PauseGame();
        // }
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        keyMappingPanel.SetActive(false); // Hide key mapping if open
        Time.timeScale = 1f; // Resume game time
        PauseController.isPaused = false;
    }

    // public void PauseGame()
    // {
    //     pauseMenuUI.SetActive(true);
    //     Time.timeScale = 0f; // Pause game time
    //     PauseController.isPaused = true;
    // }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit(); // Quit the game (only works in a build)
    }

    public void ShowKeyMapping()
    {
        keyMappingPanel.SetActive(true);
    }

    public void HideKeyMapping()
    {
        keyMappingPanel.SetActive(false);
    }
}