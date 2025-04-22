using UnityEngine;
using System.Collections;

public class newskillglow : MonoBehaviour
{
    public float interval = 1f; // 閃爍間隔
    private Coroutine flickerCoroutine;
    private CanvasGroup canvasGroup;

    public void StartFlicker()
    {
        gameObject.SetActive(true);
        if (flickerCoroutine == null)
        {
            flickerCoroutine = StartCoroutine(FlickerLoop());
        }
    }

    public void StopFlicker()
    {
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
            flickerCoroutine = null;
        }
        gameObject.SetActive(false);
    }

    IEnumerator FlickerLoop()
{
    if (canvasGroup == null)
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    float elapsedTime = 0f;
    while (elapsedTime < 5f) // ⏱ 只持續閃 5 秒
    {
        canvasGroup.alpha = 1f;
        yield return new WaitForSeconds(interval);
        elapsedTime += interval;

        canvasGroup.alpha = 0f;
        yield return new WaitForSeconds(interval);
        elapsedTime += interval;
    }

    // ✨ Flicker 結束後自動隱藏自己
    gameObject.SetActive(false);
    flickerCoroutine = null;
}
}