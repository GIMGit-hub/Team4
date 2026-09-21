using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CardZone
{
    Collection,
    Hand,
    Deck,
    Discard
}

[System.Serializable]
public class CardInstance
{
    public string instanceId;
    public GameObject template;      // どの種類か(プレハブの参照、Instantiateはしない)
    public CardZone zone;
    public Vector2? sponePosition;

    public string CardName => template.GetComponent<CardData>().cardName;
}

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [Header("全部のカード(コンボカードもここにいれる)")]
    [SerializeField] private List<GameObject> allCardList = new();

    [Header("カード使用判定")]
    [SerializeField] public RectTransform hitColision;

    private List<CardInstance> hand = new();
    private List<CardInstance> deck = new();
    private List<CardInstance> discard = new();

    private Queue<CardEffect> queue = new Queue<CardEffect>();

    public event System.Action<CardInstance, CardZone> OnCardMoved;

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

        InitDeck();
    }

    //------------------------------------デッキ初期設定------------------------------------//

    private void InitDeck()
    {
        hand.Clear();
        deck.Clear();
        discard.Clear();

        Deck playerDeck = FindAnyObjectByType<Player>().deck;

        foreach(var deckCard in playerDeck.deckCards)
        {
            for(int i = 0; i < deckCard.count; i++)
            {
                Debug.Log($"deck.Add::{deckCard.card}");

                deck.Add(new CardInstance
                {
                    instanceId = System.Guid.NewGuid().ToString(),
                    template = deckCard.card.gameObject, // Instantiateしない、参照だけ
                    zone = CardZone.Deck
                });
            }
        }

        DeckShuffle();
    }

    private void DeckReset()
    {
        Debug.Log("DeckReseting...");

        foreach(var card in discard)
        {
            CardMove(card, CardZone.Deck);
        }

        DeckShuffle();
    }


    private void DeckShuffle()
    {
        for (int i = deck.Count - 1; i >= 0; i--) 
        {
            int j = Random.Range(0, i + 1);
            (deck[i], deck[j]) = (deck[j], deck[i]);    //タプル::一行で入れ替えられる！
        }

        StartCoroutine(Call(6));
    }


    //---------------------------------------------------------------------------------------//

    //------------------------------------カード使用準備-------------------------------------//

    public bool UseCard(CardInstance instance)
    {
        int needCost = instance.template.GetComponent<CardData>().cost;

        Debug.Log($"needCost::{needCost}");
        if (!Player.Instance.CanUseCost(needCost)) return false;

        Debug.Log($"{instance.CardName}::発動準備");

        foreach(var effects in instance.template.GetComponent<CardData>().effects)
        {
            queue.Enqueue(effects);
        }

        Player.Instance.UseCost(needCost);
        CardMove(instance, CardZone.Discard);
        
        if (queue.Count > 0) StartCoroutine(CardActivation());
        return true;
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
                yield return StartCoroutine(HpHeal(effect.value));
                break;
            case CardEffect.EffectType.CostHeal:
                yield return StartCoroutine(CostHeal((int)effect.value));
                break;
            case CardEffect.EffectType.AttackBuff:
                break;
            case CardEffect.EffectType.CountBuff:
                break;
            case CardEffect.EffectType.CostBuff:
                break;
            case CardEffect.EffectType.Call:
                yield return StartCoroutine(Call((int)effect.value));
                break;
            case CardEffect.EffectType.AceCall:
                break;
        }

        yield return CardActivation();
    }

    //---------------------------------カード使用時の処理-------------------------------------//

    private void CardMove(CardInstance instance, CardZone zone)
    {
        if (GetZoneList(instance.zone).Contains(instance) != false)
            GetZoneList(instance.zone).Remove(instance);
        GetZoneList(zone).Add(instance);
        instance.zone = zone;

        OnCardMoved?.Invoke(instance, zone);
    }

    public IEnumerator HpHeal(float value)
    {
        Debug.Log($"HpHeal...{value}");

        Player.Instance.HpHeal(value);
        yield return new WaitForSeconds(0.1f);
    }
    public IEnumerator CostHeal(int value)
    {
        Debug.Log($"CostHeal...{value}");

        Player.Instance.CostHeal(value);
        yield return new WaitForSeconds(0.1f);
    }

    public IEnumerator Call(int count)
    {
        count = Mathf.Min(count, deck.Count);
        if (count == 0) yield break;

        Debug.Log($"CallStart...{count},{deck.Count}");

        for (int i = 0; i < count; i++)
        {
            CardMove(deck[0], CardZone.Hand);
            SoundsManager.Instance.PlaySound("call");
            Debug.Log($"Calling...{deck[0]}::count{i}");
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
    public CardInstance Synthesis(CardInstance card_A, CardInstance card_B, Vector2 spawnPosition)
    {
        GameObject result = null;

        foreach (var card in allCardList)
        {
            CardData data = card.GetComponentInChildren<CardData>();
            if (data.cardName == "promo_Syn")
            {
                result = card.gameObject;
                break;
            }
        }
        if (result == null) return null;

        CardMove(card_A, CardZone.Discard);
        CardMove(card_B, CardZone.Discard);

        CardInstance resultInstance = new CardInstance
        {
            instanceId = System.Guid.NewGuid().ToString(),
            template = result,
            zone = CardZone.Hand,
            sponePosition = spawnPosition
        };

        CardMove(resultInstance, CardZone.Hand);

        return resultInstance;
    }

    public List<CardInstance> GetZoneList(CardZone zone) => zone switch
    {
        CardZone.Hand => hand,
        CardZone.Deck => deck,
        CardZone.Discard => discard,
        _ => null
    };
}
