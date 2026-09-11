using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class Card : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler,IPointerEnterHandler, IPointerExitHandler
{
    public bool isSelected { get; private set; } = false;
    public bool isDraging { get; private set; } = false;

    private Canvas canvas;
    private RectTransform rect;
    private Transform parent;
    private int siblingIndex;
    private Vector2 size;

    private HandLayout m_handLayout;

    public event System.Action<Card> OnDragEnded;

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
        isDraging=false;
        canvas.overrideSorting = false;         //表示順変更不可能に

        ReturnToHand();
        OnDragEnded?.Invoke(this);
    }
    public void ReturnToHand()
    {
        canvas.overrideSorting = false;

        transform.SetParent(parent,worldPositionStays:false);
        transform.SetSiblingIndex(siblingIndex);

        rect.DOSizeDelta(size,0.15f);
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
