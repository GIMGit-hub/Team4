using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DiscriptWinUi : MonoBehaviour
{
    [SerializeField] private RectTransform synCandidatesArea;
    [SerializeField] private TextMeshProUGUI synCandidatesText_none;
    [SerializeField] private float scare = 0.6f;
    [SerializeField] private float spacing = 200.0f;

    [SerializeField] private TextMeshProUGUI discriptText;
    [SerializeField] private TextMeshProUGUI discriptText_none;

    private List<GameObject> candidateCards = new List<GameObject>();

    private void Awake()
    {
        synCandidatesText_none.enabled = true;
        discriptText_none.enabled = true;
    }

    public void SetCardInfo(CardInstance cardInstance)
    {
        List<GameObject> candidatesInstances = CardManager.Instance.GetCandidates(cardInstance);
        foreach (var card in candidatesInstances)
        {
            card.SetActive(false);
            GameObject candidateCard = Instantiate(card, synCandidatesArea);
            card.SetActive(true);

            candidateCard.GetComponent<CardController>().enabled = false;
            candidateCard.SetActive(true);

            candidateCards.Add(candidateCard);
        }

        if(candidateCards.Count != 0)
        {
            synCandidatesText_none.enabled = false;

            float totalWidth = (candidateCards.Count - 1) * spacing;
            float startX = -totalWidth / 2f;

            for (int i = 0; i < candidateCards.Count; i++)
            {
                float x = startX + i * spacing;
                candidateCards[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(x, 0f);
                candidateCards[i].GetComponent<RectTransform>().localScale = new Vector2(scare, scare);
            }
        }

        


    }
}
