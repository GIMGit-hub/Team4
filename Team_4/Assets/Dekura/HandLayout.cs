using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HandLayout : MonoBehaviour
{
    [Header("カード生成")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform handArea;

    [Header("デバッグ用")]
    [SerializeField] private int debugCardCount = 5;      // 生成する枚数

    [Header("配置設定")]
    [SerializeField] private float maxCardWidth = 120f;   // 少枚数の時の間隔(最大)
    [SerializeField] private float minCardWidth = 40f;    // 重なりが最も強い時の間隔(最小)
    [SerializeField] private float maxTotalWidth = 900f;  // 手札全体が許容する最大横幅
    [SerializeField] private float maxRotation = 8f;
    [SerializeField] private float arcHeight = 30f;
    [SerializeField] private Vector2 sponePoint = new Vector2(2000f,-100f);         // カード生成時の位置
    [SerializeField] private Vector2 handAreaPosition;

    [Header("ホバー設定")]
    [SerializeField] private float hoverHandAreaY = 100f;
    [SerializeField] private float hoverCardY = 60f;
    [SerializeField] private float hoverExpand = 2f;
    [SerializeField] private float tweenDuration = 0.15f;

    private List<GameObject> cards = new List<GameObject>();

    private void Start()
    {
        handAreaPosition = handArea.localPosition;
        SpawnDebugCards();
    }

    private void SpawnDebugCards()
    {
        foreach (var c in cards) Destroy(c.gameObject);
        cards.Clear();

        for (int i = 0; i < debugCardCount; i++)
        {
            AddCard();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) AddCard();
        if (Input.GetKeyDown(KeyCode.DownArrow) && cards.Count > 0) RemoveCard(cards[cards.Count - 1]);

        if (Input.GetKeyDown(KeyCode.Space)) UpdateLayout();
    }

    public void AddCard(GameObject card = null)
    {
        GameObject go = Instantiate(cardPrefab, handArea != null ? handArea : transform);
        go.transform.position = sponePoint;

        cards.Add(go);
        UpdateLayout();
    }

    public void RemoveCard(GameObject card)
    {
        cards.Remove(card);
        Destroy(card);
        UpdateLayout();
    }

    public void UpdateLayout()
    {
        Debug.Log("UpdateLayout");

        int count = cards.Count;
        if (count == 0) return;

        float idealWidth = maxCardWidth * (count - 1);
        float cardWidth = idealWidth > maxTotalWidth
        ? maxTotalWidth / (count - 1)
        : maxCardWidth;
        cardWidth = Mathf.Max(cardWidth, minCardWidth);

        float totalWidth = cardWidth * (count - 1);
        float startX = -totalWidth / 2f;

        bool isSelecting = false;

        for (int i = 0; i < count; i++)
        {
            RectTransform rect = cards[i].GetComponent<RectTransform>();
            Card card = cards[i].GetComponentInChildren<Card>();

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