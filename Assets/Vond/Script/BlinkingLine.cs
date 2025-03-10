using UnityEngine;
using System.Collections;

public class BlinkingLine : MonoBehaviour
{
    public float blinkInterval = 0.2f; // 闪烁间隔
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        StartCoroutine(Blink());
    }

    IEnumerator Blink()
    {
        while (true)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}
