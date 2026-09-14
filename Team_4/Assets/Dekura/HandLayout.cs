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

    private void Start()
    {
        handAreaPosition = handArea.localPosition;
        //CardManager.Instance.StartCoroutine(CardManager.Instance.Call(5));
        //SpawnDebugCards();
    }

    private void OnEnable() => CardManager.Instance.OnCardMoved += HandCardMoved;
    private void OnDisable() => CardManager.Instance.OnCardMoved -= HandCardMoved;

    private void Update()
    {
        UpdateLayout();

        if (Input.GetKeyDown(KeyCode.UpArrow)) AddCard();
        if (Input.GetKeyDown(KeyCode.DownArrow) && cards.Count > 0) RemoveCard(cards[cards.Count - 1]);
    }



    private void SpawnDebugCards()
    {
        for (int i = 0; i < debugCardCount; i++) AddCard();
    }

    private void HandCardMoved(GameObject cardObj, CardData.CardZone zone)
    {
        if (zone == CardData.CardZone.Hand) AddCard(cardObj);
        else RemoveCard(cardObj);
    }

    public void AddCard(GameObject card = null, Vector2 sponePosition = default)
    {
        if (card == null) card = debugCardPrefab;
        if (sponePosition == default) sponePosition = sponePoint;

        Debug.Log($"AddCard: {card.name} at {sponePosition}");

        GameObject go = Instantiate(card, handArea);
        go.transform.position = sponePosition;

        cards.Add(go);
        UpdateLayout();
    }

    public void RemoveCard(GameObject card)
    {
        if (cards.Contains(card))
        {
            cards.Remove(card);
            Destroy(card);
            UpdateLayout();
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
            CardVisual card = cards[i].GetComponentInChildren<CardVisual>();

            if (card.isDraging || card.m_currentSlot != null)  continue;

            float t = count == 1 ? 0.5f : (float)i / (count - 1);
            float centeredT = t - 0.5f;

            float x = startX + cardWidth * i;
            float y = -arcHeight * (centeredT * centeredT) * 4f + arcHeight;
            float angle = Mathf.Lerp(maxRotation, -maxRotation, t);

            bool isSelected = card != null && card.isSelected;

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