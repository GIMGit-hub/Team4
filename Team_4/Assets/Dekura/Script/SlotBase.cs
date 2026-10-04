using DG.Tweening;
using Unity.VisualScripting;
using UnityEditor.Rendering.Universal;
using UnityEngine;

public class SlotBase : MonoBehaviour
{
    private HandLayout handLayout;

    [Header("出現アニメーション")]
    [SerializeField] private Vector2 expandedPos;                   // カードが近づいた時の位置
    [SerializeField] private float detectionRadius = 300f;          // この距離に入ったら展開
    [SerializeField] private float tweenDuration = 0.2f;

    private Vector2 basePos;

    private bool isAnimating = false;
    private bool isHaving = false;
    private bool isExpanded = false;
    private RectTransform rect;
    private Canvas canvas;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        basePos = rect.anchoredPosition;
    }

    private void Start()
    {
        CardController.OnDragStarted += HandleCardDrag;
        CardController.OnDragEnded += HandleCardDragEnd;
    }

    private void OnEnable()
    {
        CardController.OnDragStarted += HandleCardDrag;
        CardController.OnDragEnded += HandleCardDragEnd;
    }

    private void OnDisable()
    {
        CardController.OnDragStarted -= HandleCardDrag;
        CardController.OnDragEnded -= HandleCardDragEnd;
    }

    private void HandleCardDrag(Vector2 screenPos)
    {
        if (isHaving || isAnimating) return; // カードが入っている間は展開を維持

        Vector2 slotScreenPos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, rect.position);
        float dist = Vector2.Distance(screenPos, slotScreenPos);

        SetExpanded(dist < detectionRadius);
    }

    private void HandleCardDragEnd()
    {
        if (!isHaving) SetExpanded(false);
    }

    private void SetExpanded(bool expand)
    {
        if (isExpanded == expand) return;
        isExpanded = expand;
        isAnimating = true;

        rect.DOKill();
        Tween tween = 
            expand ? rect.DOAnchorPos(expandedPos, tweenDuration).SetEase(Ease.OutBack)
                   : rect.DOAnchorPos(basePos, tweenDuration).SetEase(Ease.InBack);

        tween.OnComplete(() => isAnimating = false);
    }

    public void SetHaving(bool having) { isHaving = having; }
}
