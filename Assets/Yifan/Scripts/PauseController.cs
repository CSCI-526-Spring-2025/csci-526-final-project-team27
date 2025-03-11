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
            {
                Debug.Log("Shall Resume");
                ResumeGame();
            }
            else
            {
                Debug.Log("Shall Pause");
                PauseGame();
            }
        }
    }

    public void ResumeGame()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (keyMappingPanel != null) keyMappingPanel.SetActive(false); // Hide key mapping if open
        if (CtrlCtrl.Instance != null) CtrlCtrl.Instance.LockMove(false);
        if (CtrlCtrl.Instance != null) CtrlCtrl.Instance.ToggleShootCtrler(true);
        Time.timeScale = 1f;
        isPaused = false; // Update the shared variable
    }

    public void PauseGame()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        if (CtrlCtrl.Instance != null) CtrlCtrl.Instance.LockMove(true);
        if (CtrlCtrl.Instance != null) CtrlCtrl.Instance.ToggleShootCtrler(false);
        Time.timeScale = 0f;
        isPaused = true; // Update the shared variable
    }
}
