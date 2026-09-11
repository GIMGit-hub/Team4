using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class SynthesisSlot : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]private GameObject debugSynCard;

    [NonSerialized] public RectTransform rectTransform;
    public bool isHaving {  get; private set; }
    public GameObject havingCard {  get; private set; }

    private static List<SynthesisSlot> allSlots = new();

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable() => allSlots.Add(this);
    private void OnDisable() => allSlots.Remove(this);

    public void SetCard(GameObject card)
    {
        isHaving = true;
        havingCard = card;

        int cardCount = 0;
        foreach (var slot in allSlots)
        {
            if (slot.havingCard != null) cardCount++;
        }
        if (cardCount >= 2) TrySynthesis();
    }

    public void RemoveCard()
    {
        isHaving = false;
        havingCard = null;
    }

    public void TrySynthesis()
    {
        HandLayout handLayout = FindAnyObjectByType<HandLayout>();
        GameObject[] card = allSlots.Select(slot => slot.havingCard).ToArray(); //"Linq"ほぼforeachのような動きで、一行でまとめられる

        GameObject result = CardManager.Instance.Synthesis(card[0], card[1]);
        if (result == null)
        {
            foreach (var slot in allSlots)
            {
                slot.havingCard.GetComponentInChildren<CardVisual>().ReturnToHand();
            }
            return;
        }

        foreach (var slot in allSlots)
        {
            handLayout.RemoveCard(slot.havingCard);
            slot.RemoveCard();
        }
        handLayout.AddCard(debugSynCard, new Vector2(800, 800));
    }

    public static SynthesisSlot FindSlot(Vector2 cardPosition)
    {
        foreach (var slot in allSlots)
        {
            bool isHit = RectTransformUtility.RectangleContainsScreenPoint(slot.rectTransform, cardPosition);
            if (!slot.isHaving && isHit) return slot;
        }
        return null;
    }

    public void OnPointerEnter(PointerEventData eventData) { }
    public void OnPointerExit(PointerEventData eventData) { }
}
