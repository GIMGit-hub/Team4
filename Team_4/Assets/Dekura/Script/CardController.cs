using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class CardController : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler,IPointerEnterHandler, IPointerExitHandler,IPointerClickHandler
{
    [Header("カードの見た目")]
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI kanjiText;

    [Header("コストのハイライト")]
    [SerializeField] private Color highlightCostColor;
    [SerializeField] private float highlightCostBold = 0.2f;

    public CardInstance BoundInstance { get; private set; }
    public bool isSelected { get; private set; } = false;
    public bool isDraging { get; private set; } = false;
    public SynthesisSlot m_currentSlot { get; private set; } = null;

    private Canvas canvas;
    private RectTransform rect;
    private Transform parent;
    private int siblingIndex;
    private Vector2 size;

    private CardManager m_cardManager;

    private Vector2 discardPoint = new Vector2(-1238, -245);
    private float tweenDuration = 0.15f;

    public static event System.Action<Vector2> OnDragStarted;
    public static event System.Action OnDragEnded;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        size = rect.sizeDelta;

        m_cardManager = FindAnyObjectByType<CardManager>();

        parent = transform.parent;
        siblingIndex=transform.GetSiblingIndex();
    }


    //------------------------------データ更新-----------------------------------//

    public void UpdateCardVisual()
    {
        if( Player.Instance.GetEffect(CardEffect.EffectType.CostBuff) != 0)
        {
            costText.text = (BoundInstance.Cost - (int)Player.Instance.GetEffect(CardEffect.EffectType.CostBuff)).ToString();
            costText.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, highlightCostColor);
            costText.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, highlightCostBold);
        }
        else
        {
            costText.text = BoundInstance.Cost.ToString();
            costText.fontMaterial = costText.font.material;
        }
        kanjiText.text = BoundInstance.CardName;
    }

    public void Bind(CardInstance instance) => BoundInstance = instance;

    //-------------------------------Player操作系----------------------------------//

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
        OnDragStarted?.Invoke(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDraging = false;

        bool isUseCard = RectTransformUtility.RectangleContainsScreenPoint(m_cardManager.hitColision, rect.position);
        SynthesisSlot slot = SynthesisSlot.FindSlot(rect.position);


        if      (slot != null)  SnapToSlot(slot);
        else if (isUseCard)     UsingCard();
        else                    ReturnToHand();

        OnDragEnded?.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;

        GotoDiscard(() =>
        {
            m_cardManager.ConvertCost(BoundInstance); // アニメーション完了後に実行される
        });
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TurnManager.Instance.NowTurn != TurnManager.TurnState.PlayerTurn) return;
        if (isDraging || isSelected) return;

        SoundsManager.Instance.PlaySound("pati");
        isSelected = true;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (TurnManager.Instance.NowTurn != TurnManager.TurnState.PlayerTurn) return;

        isSelected = false;
    }

    //------------------------------カード自身の挙動-------------------------------------//

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
}
