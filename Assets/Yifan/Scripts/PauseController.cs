using UnityEngine;

public class PauseController : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameObject keyMappingPanel;
    public static bool isPaused = false; // Shared variable

    // 记录暂停前的状态
    private bool wasMovingEnabled = true;
    private bool wasShootingEnabled = true;

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

        if (!PlayerMovement.isEnd)
        {
            Debug.Log("Restore Previous Movement State");
            if (CtrlCtrl.Instance != null) CtrlCtrl.Instance.LockMove(!wasMovingEnabled);
            if (CtrlCtrl.Instance != null) CtrlCtrl.Instance.ToggleShootCtrler(wasShootingEnabled);

            Time.timeScale = 1f;
        }

        isPaused = false; // Update the shared variable
    }

    public void PauseGame()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);

        if (!PlayerMovement.isEnd)
        {
            Debug.Log("Lock Movement");
            // 记录当前状态
            if (CtrlCtrl.Instance != null)
            {
                wasMovingEnabled = !CtrlCtrl.Instance.IsMoveLocked();
                wasShootingEnabled = CtrlCtrl.Instance.IsShootingEnabled();
            }
            // 暂停时锁定移动和射击
            if (CtrlCtrl.Instance != null) CtrlCtrl.Instance.LockMove(true);
            if (CtrlCtrl.Instance != null) CtrlCtrl.Instance.ToggleShootCtrler(false);

            Time.timeScale = 0f;
        }

        isPaused = true; // Update the shared variable
    }
}