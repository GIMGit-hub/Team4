using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayInfoWindow : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] Button leftButton;
    [SerializeField] Button rightButton;

    [SerializeField] ScrollRect scrollRect;
    [SerializeField] float duration = 0.25f;
    Coroutine routine;

    void Awake()
    {
        closeButton.onClick.AddListener(() => Close());
        leftButton.onClick.AddListener(() => MoveByItem(-1));
        rightButton.onClick.AddListener(() => MoveByItem(1));
        scrollRect.onValueChanged.AddListener(_ => UpdateButtons());
    }

    void Start() => UpdateButtons();

    void MoveByItem(int dir)
    {
        RectTransform content = scrollRect.content;
        RectTransform viewport = scrollRect.viewport;

        float scrollable = content.rect.width - viewport.rect.width;
        if (scrollable <= 0f) return;

        // 1—v‘f•ª‚Ì• = —v‘f‚Ì• + spacing
        float itemWidth = GetItemWidth(content);

        // Œ»ÝˆÊ’u‚ð—v‘f’PˆÊ‚ÉŠÛ‚ß‚Ä‚©‚çA}1—v‘f“®‚©‚·
        float currentPx = scrollRect.horizontalNormalizedPosition * scrollable;
        int index = Mathf.RoundToInt(currentPx / itemWidth) + dir;
        float targetPx = Mathf.Clamp(index * itemWidth, 0f, scrollable);

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ScrollTo(targetPx / scrollable));
    }

    float GetItemWidth(RectTransform content)
    {
        var layout = content.GetComponent<HorizontalLayoutGroup>();
        float spacing = layout != null ? layout.spacing : 0f;
        var first = (RectTransform)content.GetChild(0);
        return first.rect.width + spacing;
    }

    IEnumerator ScrollTo(float target)
    {
        scrollRect.velocity = Vector2.zero;
        float start = scrollRect.horizontalNormalizedPosition;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float k = Mathf.SmoothStep(0f, 1f, t / duration);
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(start, target, k);
            yield return null;
        }
        scrollRect.horizontalNormalizedPosition = target;
        routine = null;
    }

    void UpdateButtons()
    {
        float p = scrollRect.horizontalNormalizedPosition;
        leftButton.interactable = p > 0.001f;
        rightButton.interactable = p < 0.999f;
    }

    public void Close()
    {
        Destroy(gameObject);
    }
}
