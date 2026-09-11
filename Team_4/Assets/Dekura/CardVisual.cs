using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class CardVisual : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler,IPointerEnterHandler, IPointerExitHandler
{
    public bool isSelected { get; private set; } = false;
    public bool isDraging { get; private set; } = false;
    public SynthesisSlot m_currentSlot { get; private set; } = null;

    private Canvas canvas;
    private RectTransform rect;
    private Transform parent;
    private int siblingIndex;
    private Vector2 size;

    private HandLayout m_handLayout;

    private float tweenDuration = 0.15f;

    public event System.Action<CardVisual> OnDragEnded;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        size = rect.sizeDelta;

        m_handLayout = FindAnyObjectByType<HandLayout>();

        parent = transform.parent;
        siblingIndex=transform.GetSiblingIndex();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (TurnManager.Instance.NowTurn != TurnManager.TurnState.PlayerTurn) return;

        m_currentSlot?.RemoveCard();
        m_currentSlot = null;
        isDraging = true;

        rect.DOKill();                          //dotween動作中止
        canvas.overrideSorting = true;          //表示順変更可能に
        canvas.sortingOrder = 999;              //最前表示
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (TurnManager.Instance.NowTurn != TurnManager.TurnState.PlayerTurn) return;

        rect.anchoredPosition += eventData.delta/canvas.scaleFactor;   //マウス移動量/canvasの大きさ
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDraging = false;
        canvas.overrideSorting = false;         //表示順変更不可能に

        SynthesisSlot slot = SynthesisSlot.FindSlot(rect.position);
        if (slot != null) SnapToSlot(slot);
        else ReturnToHand();

        OnDragEnded?.Invoke(this);
    }

    public void SnapToSlot(SynthesisSlot slot)
    {
        Debug.Log($"SnapToSlot：{slot}");
        m_currentSlot = slot;

        canvas.overrideSorting = false;
        transform.SetParent(slot.transform, worldPositionStays: true);
        rect.localScale = Vector3.one;

        transform.DOKill();
        rect.DOAnchorPos(Vector2.zero, tweenDuration);
        rect.DOLocalRotate(Vector3.zero, tweenDuration);
        rect.DOSizeDelta(slot.rectTransform.sizeDelta, tweenDuration);

        slot.SetCard(gameObject);
    }

    public void ReturnToHand()
    {
        Debug.Log("ReturnToHand");
        m_currentSlot?.RemoveCard();
        m_currentSlot = null;

        canvas.overrideSorting = false;

        transform.DOKill();
        transform.SetParent(parent, worldPositionStays: false);
        transform.SetSiblingIndex(siblingIndex);
        rect.localScale = Vector3.one;

        rect.DOSizeDelta(size, tweenDuration);
    }




    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TurnManager.Instance.NowTurn != TurnManager.TurnState.PlayerTurn) return;

        SoundsManager.Instance.PlaySound("pati");
        isSelected = true;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (TurnManager.Instance.NowTurn != TurnManager.TurnState.PlayerTurn) return;

        isSelected = false;
    }
}
