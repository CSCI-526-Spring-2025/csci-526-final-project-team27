using UnityEngine;

public class PauseController : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameObject keyMappingPanel;
    public static bool isPaused = false; // Shared variable

    // 记录暂停前的状态
    private bool wasMovingEnabled = true;
    private bool wasShootingEnabled = true;
    private float originalTimeScale = 1f; // 记录暂停前的timeScale
    private bool isGamePausedByThisController = false;

    void Start()
    {
        originalTimeScale = Time.timeScale;
        Debug.Log($"Start: originalTimeScale = {originalTimeScale}, current Time.timeScale = {Time.timeScale}");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log($"Before Pause/Resume: isPaused = {isPaused}, originalTimeScale = {originalTimeScale}, current Time.timeScale = {Time.timeScale}");
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

        // 确保暂停状态的一致性
        if (isGamePausedByThisController && Time.timeScale != 0f)
        {
            Time.timeScale = 0f;
        }
    }

    public void ResumeGame()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (keyMappingPanel != null) keyMappingPanel.SetActive(false);

        if (!PlayerMovement.isEnd || !LevelUpRewardSystem.isLevelUp)
        {
            Debug.Log($"ResumeGame: Setting Time.timeScale to {originalTimeScale}");
            if (CtrlCtrl.Instance != null) CtrlCtrl.Instance.LockMove(!wasMovingEnabled);
            if (CtrlCtrl.Instance != null) CtrlCtrl.Instance.ToggleShootCtrler(wasShootingEnabled);

            Time.timeScale = originalTimeScale;
            isGamePausedByThisController = false;
        }

        isPaused = false;
        Debug.Log($"After Resume: isPaused = {isPaused}, Time.timeScale = {Time.timeScale}");
    }

    public void PauseGame()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);

        if (!PlayerMovement.isEnd || !LevelUpRewardSystem.isLevelUp)
        {
            Debug.Log($"PauseGame: Current Time.timeScale = {Time.timeScale}");
            if (CtrlCtrl.Instance != null)
            {
                wasMovingEnabled = !CtrlCtrl.Instance.IsMoveLocked();
                wasShootingEnabled = CtrlCtrl.Instance.IsShootingEnabled();
            }
            if (CtrlCtrl.Instance != null) CtrlCtrl.Instance.LockMove(true);
            if (CtrlCtrl.Instance != null) CtrlCtrl.Instance.ToggleShootCtrler(false);
            
            originalTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            isGamePausedByThisController = true;
        }
        
        isPaused = true;
        Debug.Log($"After Pause: isPaused = {isPaused}, originalTimeScale = {originalTimeScale}, Time.timeScale = {Time.timeScale}");
    }
}