using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeScreen : MonoBehaviour
{
    [SerializeField] private Image fadeImage; // 画面全体を覆う黒いImage
    [SerializeField] private float fadeDuration = 1.5f; // 1回あたりの時間(往復で約3秒)

    private void Awake()
    {
        SetAlpha(0f);
        fadeImage.raycastTarget = false; // 透明時はクリックを邪魔しない
    }

    // 黒くする→onBlackを実行→元に戻す、という一連の流れ
    public void FadeOutIn(Action onBlack)
    {
        StartCoroutine(FadeRoutine(onBlack));
    }

    private IEnumerator FadeRoutine(Action onBlack)
    {
        fadeImage.raycastTarget = true;
        yield return Fade(0f, 1f, fadeDuration); // 暗くする

        onBlack?.Invoke(); // 真っ暗の間に画面切り替え

        yield return new WaitForSeconds(0.2f);
        yield return Fade(1f, 0f, fadeDuration); // 元に戻す
        fadeImage.raycastTarget = false;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(from, to, elapsed / duration));
            yield return null;
        }
        SetAlpha(to);
    }

    private void SetAlpha(float a)
    {
        Color c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
    }
}