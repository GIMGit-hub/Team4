using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HandLayout : MonoBehaviour
{
    [Header("手札スペース")]
    [SerializeField] private Transform handArea;

    [Header("デバッグ用")]
    [SerializeField] private GameObject debugCardPrefab;
    [SerializeField] private int debugCardCount = 5;      // 生成する枚数

    [Header("カード配置設定")]
    [SerializeField] private float maxCardWidth = 120f;   // 少枚数の時の間隔(最大)
    [SerializeField] private float minCardWidth = 40f;    // 重なりが最も強い時の間隔(最小)
    [SerializeField] private float maxTotalWidth = 900f;  // 手札全体が許容する最大横幅
    [SerializeField] private float maxRotation = 8f;
    [SerializeField] private float arcHeight = 30f;
    [SerializeField] private Vector2 sponePoint = new Vector2(2000f, -100f);         // カード生成時の位置

    [Header("カードホバー設定")]
    [SerializeField] private float hoverHandAreaY = 100f;
    [SerializeField] private float hoverCardY = 60f;
    [SerializeField] private float hoverExpand = 2f;
    [SerializeField] private float tweenDuration = 0.15f;

    private Vector2 handAreaPosition;
    private List<GameObject> cards = new List<GameObject>();
    private Dictionary<CardInstance, GameObject> activeCards = new();

    public bool isSelected { get; private set; }

    private void Start()
    {
        handAreaPosition = handArea.localPosition;
        CardManager.Instance.OnCardMoved -= HandCardMoved;
        CardManager.Instance.OnCardMoved += HandCardMoved;
    }

    private void OnEnable()  { if (CardManager.Instance != null) CardManager.Instance.OnCardMoved += HandCardMoved; }
    private void OnDisable() { if (CardManager.Instance != null) CardManager.Instance.OnCardMoved -= HandCardMoved; }

    private void Update()
    {
        UpdateLayout();
    }

    private void HandCardMoved(CardInstance cardObj, CardZone zone)
    {
        if (zone == CardZone.Hand) AddCard(cardObj);
        else RemoveCard(cardObj);
    }

    public void AddCard(CardInstance instance)
    {
        Debug.Log($"AddCard: {instance.CardName}");

        GameObject go = Instantiate(instance.template, handArea, false);
        go.GetComponent<CardController>().Bind(instance);
        go.transform.position = instance.sponePosition ?? sponePoint;
        go.transform.SetAsFirstSibling();

        activeCards[instance] = go;
        cards.Add(go);
    }

    public void RemoveCard(CardInstance instance)
    {
        if (activeCards.TryGetValue(instance, out var go))
        {
            activeCards.Remove(instance);
            cards.Remove(go);
            Destroy(go); 
        }
    }

    public void UpdateLayout()
    {
        int count = cards.Count;
        if (count == 0) return;

        float idealWidth = maxCardWidth * (count - 1);
        float cardWidth = idealWidth > maxTotalWidth ? maxTotalWidth / (count - 1) : maxCardWidth;
        cardWidth = Mathf.Max(cardWidth, minCardWidth);

        float totalWidth = cardWidth * (count - 1);
        float startX = -totalWidth / 2f;

        bool isSelecting = false;

        for (int i = 0; i < count; i++)
        {
            RectTransform rect = cards[i].GetComponent<RectTransform>();
            CardController card = cards[i].GetComponentInChildren<CardController>();

            if (card.isDraging || card.m_currentSlot != null)  continue;

            float t = count == 1 ? 0.5f : (float)i / (count - 1);
            float centeredT = t - 0.5f;

            float x = startX + cardWidth * i;
            float y = -arcHeight * (centeredT * centeredT) * 4f + arcHeight;
            float angle = Mathf.Lerp(maxRotation, -maxRotation, t);

            isSelected = card != null && card.isSelected;

            Vector2 targetPos;
            Quaternion targetRot;

            if (isSelected)
            {
                targetPos = new Vector2(x, y + hoverCardY);
                targetRot = Quaternion.identity;
                isSelecting = true;
            }
            else
            {
                targetPos = new Vector2(x, y);
                targetRot = Quaternion.Euler(0, 0, angle);
            }

            rect.DOLocalMove(targetPos, tweenDuration);
            rect.DOLocalRotateQuaternion(targetRot, tweenDuration);
            rect.SetSiblingIndex(isSelected ? cards.Count - 1 : i);
        }

        if(isSelecting)
        {
            Vector2 position = new Vector2(handAreaPosition.x, handAreaPosition.y + hoverHandAreaY);
            handArea.DOLocalMove(position, tweenDuration);
        }
        else
        {
            handArea.DOLocalMove(handAreaPosition, tweenDuration);
        }
    }
}