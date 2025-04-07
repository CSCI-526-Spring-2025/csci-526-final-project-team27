using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI; // Reference to Pause Menu UI
    public GameObject keyMappingPanel; // Reference to Key Mapping Panel
    private PauseController pauseController;

    void Start()
    {
        // 获取场景中的PauseController
        pauseController = Object.FindFirstObjectByType<PauseController>();
        if (pauseController == null)
        {
            Debug.LogWarning("PauseController not found in scene!");
        }
    }

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
        // 如果找不到PauseController，则使用备用逻辑
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (keyMappingPanel != null) keyMappingPanel.SetActive(false); // Hide key mapping if open

        if (!PlayerMovement.isEnd)
        {
            // 使用PauseController来恢复游戏
            if (pauseController != null)
            {
                pauseController.ResumeGame();
                return;
            }

            Debug.Log("使用备用恢复逻辑 - 保持当前状态");
            Time.timeScale = 1f; // Resume game time
        }

        PauseController.isPaused = false;
    }

    // public void PauseGame()
    // {
    //     if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
    //     Time.timeScale = 0f; // Pause game time
    //     PauseController.isPaused = true;
    // }

    // public void QuitGame()
    // {
    //     Debug.Log("Quitting Game...");
    //     FirebaseDataUploader firebaseDataUploader = FindFirstObjectByType<FirebaseDataUploader>();
    //     if (firebaseDataUploader != null)
    //     {
    //         firebaseDataUploader.ForceUploadData();
    //     }
    //     Application.Quit(); // Quit the game (only works in a build)
    // }

    public void ShowKeyMapping()
    {
        if (keyMappingPanel != null) keyMappingPanel.SetActive(true);
    }

    public void HideKeyMapping()
    {
        if (keyMappingPanel != null) keyMappingPanel.SetActive(false);
    }
}