using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StartImageEffect : MonoBehaviour, IPointerClickHandler
{
    [Header("点滅させるImage")]
    [SerializeField] private Image startImage;

    [Header("遷移先のシーン名")]
    [SerializeField] private string nextSceneName = "TitleScene";

    [Header("点滅時間")]
    [SerializeField] private float blinkDuration = 2f;

    [Header("点滅速度")]
    [SerializeField] private float blinkSpeed = 5f;

    private bool isProcessing;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        OnStartClicked();
    }

    public void OnStartClicked()
    {
        if (isProcessing) return;

        if (startImage == null)
        {
            Debug.LogError("点滅させるImageが設定されていません");
            return;
        }

        SoundsManager.Instance.PlaySound("accept");

        isProcessing = true;
        StartCoroutine(BlinkAndFade());
    }

    private IEnumerator BlinkAndFade()
    {
        Color originalColor = startImage.color;
        float elapsed = 0f;

        // Imageを点滅させる
        while (elapsed < blinkDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float alpha =
                (Mathf.Sin(elapsed * blinkSpeed * Mathf.PI * 2f) + 1f) / 2f;

            Color color = originalColor;
            color.a = alpha;
            startImage.color = color;

            yield return null;
        }

        startImage.color = originalColor;

        // 既存のSceneFaderでフェードアウトして遷移
        SceneFader.LoadScene(nextSceneName);
    }
}