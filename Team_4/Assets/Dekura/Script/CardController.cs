using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static CardEffect;

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

    public static event System.Action<Vector2, EffectTarget> OnDragStarted;
    public static event System.Action OnDragEnded;

    private void OnEnable() => SetEventSubscribed(true);
    private void OnDisable() => SetEventSubscribed(false);

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        size = rect.sizeDelta;

        m_cardManager = FindAnyObjectByType<CardManager>();

        parent = transform.parent;
        siblingIndex=transform.GetSiblingIndex();
    }

    void Start()
    {
        SetEventSubscribed(true);
        UpdateCardVisual();
    }

    //------------------------------データ更新-----------------------------------//

    private void SetEventSubscribed(bool isEnable)
    {
        if (Player.Instance != null)
        {
            Player.Instance.BuffAdded -= UpdateCardVisual;
            if (isEnable) Player.Instance.BuffAdded += UpdateCardVisual;
        }
    }

    public void UpdateCardVisual()
    {
        if (Player.Instance.GetEffect(CardEffect.EffectType.CostFree) != 0)
        {
            costText.text = "0";
            costText.color = highlightCostColor;
        }
        else if ((BoundInstance.costType == CardData.CostType.Normal) && 
                 (BoundInstance.costType == CardData.CostType.Ace) &&
           (Player.Instance.GetEffect(CardEffect.EffectType.CostBuff) != 0))
        {
            costText.text = (Mathf.Max(0, BoundInstance.Cost - (int)Player.Instance.GetEffect(CardEffect.EffectType.CostBuff))).ToString();
            costText.color = highlightCostColor;
            //costText.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, highlightCostColor);
            //costText.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, highlightCostBold);
        }
        else if ((BoundInstance.AceCard) &&
                (Player.Instance.GetEffect(CardEffect.EffectType.CostBuff_Ace) != 0))
        {
            costText.text = "0";
            costText.color = highlightCostColor;
            //costText.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, highlightCostColor);
            //costText.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, highlightCostBold);
        }
        else if (BoundInstance.costType != CardData.CostType.AllCost)
        {
            costText.text = BoundInstance.Cost.ToString();
            costText.color = Color.white;
            //costText.fontMaterial = costText.font.material;
        }
    }

    private EffectTarget GetEffectTarget()
    {
        EffectTarget target = EffectTarget.Player;
        foreach (var ins in BoundInstance.cardData.effects)
        {
            if (ins.target == EffectTarget.Enemy) target = EffectTarget.Enemy;
            if (ins.target == EffectTarget.AllEnemy)
            {
                target = EffectTarget.AllEnemy;
                break;
            }
        }

        return target;
    }

    public void Bind(CardInstance instance) => BoundInstance = instance;

    //-------------------------------Player操作系----------------------------------//

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (TurnManager.Instance.NowTurn != TurnManager.TurnState.PlayerTurn) return;
        if (Player.Instance.isDead) return;

        ReturnToHand();
        isDraging = true;

        rect.eulerAngles = Vector3.zero;
        SetPositionToPointer(eventData);

        DiscriptWindow.Instance.CloseWindow();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (TurnManager.Instance.NowTurn != TurnManager.TurnState.PlayerTurn) return;
        if (Player.Instance.isDead) return;

        rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
        OnDragStarted?.Invoke(eventData.position, GetEffectTarget());
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        bool isUseCard = RectTransformUtility.RectangleContainsScreenPoint(m_cardManager.hitColision, rect.position);
        SynthesisSlot slot = SynthesisSlot.FindSlot(rect.position);

        if (slot != null)
            SnapToSlot(slot);
        else if (GetEffectTarget() == EffectTarget.Enemy && SpawnArea.onCursolEnemy != null)
            UsingCard(SpawnArea.onCursolEnemy);
        else if (isUseCard && GetEffectTarget() != EffectTarget.Enemy)
            UsingCard();
        else
            ReturnToHand();

        isDraging = false;

        OnDragEnded?.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        if (Player.Instance.isDead) return;

        GotoDiscard(() =>
        {
            m_cardManager.ConvertCost(BoundInstance); // アニメーション完了後に実行される
        });
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TurnManager.Instance.NowTurn != TurnManager.TurnState.PlayerTurn) return;
        if (isDraging || isSelected) return;
        if (Player.Instance.isDead) return;

        SoundsManager.Instance.PlaySound("pati");
        if (m_currentSlot == null)
        {
            DiscriptWindow.Instance.OpenWindow(gameObject, BoundInstance);
            isSelected = true;
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (TurnManager.Instance.NowTurn != TurnManager.TurnState.PlayerTurn) return;
        if (Player.Instance.isDead) return;

        isSelected = false;
        DiscriptWindow.Instance.CloseWindow();
    }

    private void SetPositionToPointer(PointerEventData eventData)
    {
        // Screen Space - Overlay の場合は null
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                rect, eventData.position, cam, out Vector3 world))
        {
            rect.position = world;
        }
    }

    //------------------------------カード自身の挙動-------------------------------------//

    private void UsingCard(EnemyUnit enemy = null)
    {
        Debug.Log($"TryUse_Wating::{BoundInstance}");
        if (!m_cardManager.UseCard(BoundInstance, enemy)) ReturnToHand();
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
        isDraging = false;

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
