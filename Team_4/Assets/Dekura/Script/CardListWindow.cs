using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardListWindow : MonoBehaviour
{
    [SerializeField] private GameObject window;
    [SerializeField] private TextMeshProUGUI windowTitle; 
    [SerializeField] private Transform scrollView;
    [SerializeField] private float cardScare;
    [SerializeField] private float padding;
    [SerializeField] private Button closeButton;

    [SerializeField] private Button discardButton;

    private List<GameObject> showingCards = new();
    private bool isShowing = false;

    private void Awake()
    {
        window.SetActive(false);
        closeButton.onClick.AddListener(() => CloseWindow());
        discardButton.onClick.AddListener(() => OpenWindow(CardZone.Discard));
    }

    public void OpenWindow(CardZone zone)
    {
        if (isShowing) return;
        CardClear();

        List<CardInstance> list = CardManager.Instance.GetZoneList(zone);
        Vector2 size = new();

        foreach (var card in list)
        {
            card.template.SetActive(false);
            GameObject candidateCard = Instantiate(card.template, scrollView, false);
            card.template.SetActive(true);

            candidateCard.GetComponent<CardController>().enabled = false;
            candidateCard.SetActive(true);

            showingCards.Add(candidateCard);

            RectTransform cardRect = candidateCard.GetComponent<RectTransform>();
            cardRect.localScale = new Vector2(cardScare, cardScare);

            size = cardRect.sizeDelta * cardScare;
        }

        switch (zone)
        {
            case CardZone.Deck:
                windowTitle.text = "-山札-";
                break;
            case CardZone.Discard:
                windowTitle.text = "-墓地-";
                break;
            default:break;
        }

        scrollView.GetComponent<GridLayoutGroup>().cellSize = size;
        scrollView.GetComponent<GridLayoutGroup>().spacing = new Vector2(padding, padding);
        window.SetActive(true);
        isShowing = true;
    }

    public void CloseWindow()
    {
        if (!isShowing) return;

        CardClear();
        window.SetActive(false);
        isShowing = false;
    }

    private void CardClear()
    {
        foreach (var card in showingCards)
            Destroy(card);

        showingCards.Clear();
    }
}
