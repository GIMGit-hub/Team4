using System.Collections;
using TMPro;
using UnityEngine;

public class ActionAnnounceUI : MonoBehaviour
{
    [SerializeField] private RectTransform panel;   // 動かすパネル自体
    [SerializeField] private TMP_Text actionText;    // 行動名を表示するテキスト

    [Header("位置設定")]
    [SerializeField] private float hiddenY = 200f;   // 画面上の隠れ位置(Y座標)
    [SerializeField] private float shownY = 0f;       // 表示されたときのY座標

    [Header("アニメーション設定")]
    [SerializeField] private float slideDuration = 0.3f; // 降りてくる/戻る時間
    [SerializeField] private float stayDuration = 1.0f;   // 表示され続ける時間

    [ContextMenu("Test Show")]
    public void TestShow()
    {
        Show("テスト表示");
    }

    private Coroutine current;

    private void Awake()
    {
        // 最初は画面外(上)に隠しておく
        SetY(hiddenY);
    }

    // 外部から呼ぶ: 行動名を表示する
    public void Show(string text)
    {
        actionText.text = text;

        if (current != null) StopCoroutine(current);
        current = StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        // 降りてくる
        yield return Slide(hiddenY, shownY, slideDuration);

        // 止まって表示され続ける
        yield return new WaitForSeconds(stayDuration);

        // 上に戻る
        yield return Slide(shownY, hiddenY, slideDuration);
    }

    private IEnumerator Slide(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetY(Mathf.Lerp(from, to, t));
            yield return null;
        }
        SetY(to);
    }

    private void SetY(float y)
    {
        Vector2 pos = panel.anchoredPosition;
        pos.y = y;
        panel.anchoredPosition = pos;
    }
}