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
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (keyMappingPanel != null) keyMappingPanel.SetActive(false); // Hide key mapping if open
        Debug.Log("Unlock Movement");
        if (CtrlCtrl.Instance != null) CtrlCtrl.Instance.LockMove(false);
        if (CtrlCtrl.Instance != null) CtrlCtrl.Instance.ToggleShootCtrler(true);
        Time.timeScale = 1f; // Resume game time
        PauseController.isPaused = false;
    }

    // public void PauseGame()
    // {
    //     if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
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
        if (keyMappingPanel != null) keyMappingPanel.SetActive(true);
    }

    public void HideKeyMapping()
    {
        if (keyMappingPanel != null) keyMappingPanel.SetActive(false);
    }
}