using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class CardController : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler,IPointerEnterHandler, IPointerExitHandler,IPointerClickHandler
{
    public CardInstance BoundInstance { get; private set; }
    public bool isSelected { get; private set; } = false;
    public bool isDraging { get; private set; } = false;
    public SynthesisSlot m_currentSlot { get; private set; } = null;

    private Canvas canvas;
    private RectTransform rect;
    private Transform parent;
    private int siblingIndex;
    private Vector2 size;
    private CardData thisCardData;

    private HandLayout m_handLayout;
    private CardManager m_cardManager;

    private Vector2 discardPoint = new Vector2(-1238, -245);
    private float tweenDuration = 0.15f;

    public event System.Action<CardController> OnDragEnded;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        size = rect.sizeDelta;

        thisCardData = GetComponent<CardData>();

        m_handLayout = FindAnyObjectByType<HandLayout>();
        m_cardManager = FindAnyObjectByType<CardManager>();

        parent = transform.parent;
        siblingIndex=transform.GetSiblingIndex();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (TurnManager.Instance.NowTurn != TurnManager.TurnState.PlayerTurn) return;

        m_currentSlot?.RemoveCard();
        m_currentSlot = null;
        isDraging = true;

        rect.DOKill();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (TurnManager.Instance.NowTurn != TurnManager.TurnState.PlayerTurn) return;

        rect.anchoredPosition += eventData.delta/canvas.scaleFactor; 
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDraging = false;

        bool isUseCard = RectTransformUtility.RectangleContainsScreenPoint(m_cardManager.hitColision, rect.position);
        SynthesisSlot slot = SynthesisSlot.FindSlot(rect.position);


        if      (slot != null)  SnapToSlot(slot);
        else if (isUseCard)     UsingCard();
        else                    ReturnToHand();

        OnDragEnded?.Invoke(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;

        GotoDiscard(() =>
        {
            Debug.Log("aaaaaaaaaaaa");
            m_cardManager.ConvertCost(BoundInstance); // アニメーション完了後に実行される
        });
    }

    private void UsingCard()
    {
        Debug.Log($"TryUse_Wating::{BoundInstance}");
        if (!m_cardManager.UseCard(BoundInstance)) ReturnToHand();
    }

    public void SnapToSlot(SynthesisSlot slot)
    {
        Debug.Log($"SnapToSlot�F{slot}");
        m_currentSlot = slot;

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

        transform.DOKill();
        transform.SetParent(parent, worldPositionStays: false);
        transform.SetSiblingIndex(siblingIndex);
        rect.localScale = Vector3.one;

        rect.DOSizeDelta(size, tweenDuration);
    }

    public void GotoDiscard(System.Action onComplete = null)
    {
        isDraging = true;
        gameObject.GetComponent<RectTransform>().DOLocalMove(discardPoint, tweenDuration)
            .OnComplete(() => { onComplete?.Invoke(); });
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

    public void Bind(CardInstance instance) => BoundInstance = instance;
}
