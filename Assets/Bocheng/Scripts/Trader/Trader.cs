using UnityEngine;

public class Trader : MonoBehaviour
{
    public Canvas worldSpaceCanvas;  // 商人的交互提示 Canvas
    public GameObject shopUI;        // 商店UI
    private bool isPlayerNear = false;

    private void Start()
    {
        worldSpaceCanvas.gameObject.SetActive(false);
        shopUI.SetActive(false);
    }

    public void PlayerEnteredRange()
    {
        isPlayerNear = true;
        worldSpaceCanvas.gameObject.SetActive(true);
    }

    public void PlayerExitedRange()
    {
        isPlayerNear = false;
        worldSpaceCanvas.gameObject.SetActive(false);
        shopUI.SetActive(false); // 关闭商店UI
    }

    private void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            ToggleShop();
        }
    }

    private void ToggleShop()
    {
        shopUI.SetActive(!shopUI.activeSelf);
    }
}
