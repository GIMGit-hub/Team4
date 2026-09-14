using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using static UnityEngine.GraphicsBuffer;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [Header("全部のカード(コンボカードもここにいれる)")]
    [SerializeField] private List<GameObject> allCardList = new();
    [SerializeField] private Deck firstDeck;

    private Dictionary<CardData, GameObject> allCardData = new();

    private List<CardData> hand = new();
    private List<CardData> deck = new();
    private List<CardData> discard = new();

    private Queue<CardEffect> queue = new Queue<CardEffect>();

    public event System.Action<GameObject, CardData.CardZone> OnCardMoved;
    public event System.Action<CardData> OnCardDataChanged;

    private void Awake()
    {
        //------インスタンス化------//
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        //---------------------------//

        foreach (var card in allCardList)
        {
            CardData data = card.GetComponentInChildren<CardData>();
            if (data != null) allCardData[data] = card;
        }

        initDeck();
    }

    //------------------------------------デッキ初期設定------------------------------------//

    private void initDeck()
    {
        hand.Clear();
        deck.Clear();
        discard.Clear();

        foreach(var deckCard in firstDeck.deckCards)
        {
            for(int i = 0; i < deckCard.count; i++)
            {
                deck.Add(deckCard.card);
                deckCard.card.SetZone(CardData.CardZone.Deck);
            }
        }

        shuffle();
    }

    private void shuffle()
    {
        for (int i = deck.Count - 1; i >= 0; i--) 
        {
            int j = Random.Range(0, i + 1);
            (deck[i], deck[j]) = (deck[j], deck[i]);
        }

        StartCoroutine(Call(6));
    }


    //---------------------------------------------------------------------------------------//

    //------------------------------------カード使用準備-------------------------------------//

    public void UseCard(CardData data)
    {
        Debug.Log($"{data.cardName}::発動準備");

        foreach(var effects in data.effects)
        {
            queue.Enqueue(effects);
        }

        CardMove(data, CardData.CardZone.Discard);
        if (queue.Count > 0) StartCoroutine(CardActivation());
    }

    private IEnumerator CardActivation()
    {
        if (queue.Count == 0) yield break;

        CardEffect effect = queue.Dequeue();

        switch (effect.type)
        {
            case CardEffect.EffectType.Attack:
                break;
            case CardEffect.EffectType.Defense:
                break;
            case CardEffect.EffectType.HpHeal:
                break;
            case CardEffect.EffectType.CostHeal:
                break;
            case CardEffect.EffectType.AttackBuff:
                break;
            case CardEffect.EffectType.CountBuff:
                break;
            case CardEffect.EffectType.CostBuff:
                break;
            case CardEffect.EffectType.Call:
                yield return StartCoroutine(Call(effect.valueCount));
                break;
            case CardEffect.EffectType.AceCall:
                break;
        }

        yield return CardActivation();
    }

    //---------------------------------カード使用時の処理-------------------------------------//

    private void CardMove(CardData data, CardData.CardZone zone)
    {
        GameObject cardObj = allCardData[data];

        GetZoneList(data.cardZone).Remove(data);
        GetZoneList(zone).Add(data);
        data.SetZone(CardData.CardZone.Hand);

        Debug.Log($"{cardObj} : MoveTo {zone}");
        OnCardMoved?.Invoke(cardObj, zone); //このカードが〇〇に移動した、と伝える
    }

    public IEnumerator Call(int count)
    {
        for (int i = 0; i <= count; i++) 
        {
            CardMove(deck[0], CardData.CardZone.Hand);
            yield return new WaitForSeconds(0.1f);
        }
    }

    //----------------------------------------------------------------------------------------//

    /// <summary>
    /// 合成処理
    /// </summary>
    /// <param name="card_A"></param>
    /// <param name="card_B"></param>
    /// <returns></returns>
    public GameObject Synthesis(GameObject card_A, GameObject card_B)
    {
        foreach (var card in allCardList)
        {
            CardData data = card.GetComponentInChildren<CardData>();
            if (data.cardName == "promo_Syn") return card.gameObject;
        }
        return null;
    }

    public List<CardData> GetZoneList(CardData.CardZone zone) => zone switch
    {
        CardData.CardZone.Hand => hand,
        CardData.CardZone.Deck => deck,
        CardData.CardZone.Discard => discard,
        _ => null
    };
}
