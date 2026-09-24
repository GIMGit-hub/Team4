using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using static Deck;

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
    public event System.Action OnCardUsed;

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
    }

    private void Start()
    {
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
                Debug.Log($"deckAdd::{deckCard.card}");

                deck.Add(new CardInstance
                {
                    instanceId = System.Guid.NewGuid().ToString(),
                    template = deckCard.card.gameObject, // Instantiateしない、参照だけ
                    zone = CardZone.Deck
                });
            }
        }

        Debug.Log($"deck::{deck.Count}");

        DeckShuffle();
        StartCoroutine(Call(6));
    }

    private void DeckReset()
    {
        Debug.Log("DeckReseting...");

        foreach (var card in discard.ToList())
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
    }


    //---------------------------------------------------------------------------------------//

    //------------------------------------カード使用準備-------------------------------------//

    public bool UseCard(CardInstance instance)
    {
        //コスト支払い可能かの確認
        int needCost = instance.template.GetComponent<CardData>().cost;
        Debug.Log($"needCost::{needCost}");
        if (!Player.Instance.CanUseCost(needCost)) return false;

        Debug.Log($"{instance.CardName}::発動準備");

        //効果をqueに入れてく
        foreach(var effects in instance.template.GetComponent<CardData>().effects)
        {
            queue.Enqueue(effects);
        }

        //コスト支払い
        Player.Instance.UseCost(needCost);

        //カードの移動、合成カードは削除
        if (instance.template.GetComponent<CardData>().cardType == CardData.CardType.DeckCard)
            CardMove(instance, CardZone.Discard);
        else
            GetZoneList(instance.zone).Remove(instance);

        //効果発動
        if (queue.Count > 0) StartCoroutine(CardActivation());
        return true;
    }

    private IEnumerator CardActivation()
    {
        //queが残り無ければ終了
        if (queue.Count == 0) yield break;

        //データを取得して、そのqueを解除
        CardEffect effect = queue.Dequeue();

        switch (effect.type)
        {
            case CardEffect.EffectType.Attack:
                yield return StartCoroutine(Attack(effect.target, effect.value, effect.valueCount));
                break;
            case CardEffect.EffectType.Defense:
                yield return StartCoroutine(DpHeal(effect.value));
                break;
            case CardEffect.EffectType.HpHeal:
                yield return StartCoroutine(HpHeal(effect.value));
                break;
            case CardEffect.EffectType.CostHeal:
                yield return StartCoroutine(CostHeal((int)effect.value));
                break;
            case CardEffect.EffectType.AttackBuff:
            case CardEffect.EffectType.CountBuff:
            case CardEffect.EffectType.CostBuff:
                yield return StartCoroutine(AddBuff(effect.type, effect.value, effect.valueCount));
                break;
            case CardEffect.EffectType.Call:
                yield return StartCoroutine(Call((int)effect.value));
                break;
            case CardEffect.EffectType.AceCall:
                break;
        }

        //カード使用しましたよ～_OnCardUsed発火
        OnCardUsed?.Invoke();
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

    public IEnumerator Attack(CardEffect.EffectTarget target, float value, int count)
    {
        //その他色々な攻撃加算処理

        Player.Instance.Attack(target, value, count);
        yield return new WaitForSeconds(0.1f);
    }

    public IEnumerator HpHeal(float value)
    {
        Debug.Log($"HpHeal...{value}");

        Player.Instance.HpHeal(value);
        yield return new WaitForSeconds(0.1f);
    }
    public IEnumerator DpHeal(float value)
    {
        Debug.Log($"DpHeal...{value}");

        Player.Instance.DpHeal(value);
        yield return new WaitForSeconds(0.1f);
    }
    public IEnumerator CostHeal(int value)
    {
        Debug.Log($"CostHeal...{value}");

        Player.Instance.CostHeal(value);
        yield return new WaitForSeconds(0.1f);
    }

    public IEnumerator AddBuff(CardEffect.EffectType m_type, float m_value, int m_enableTurn) 
    {
        Player.Instance.AddEffect(m_type, m_value, m_enableTurn);
        yield return new WaitForSeconds(0.1f);
    }

    public IEnumerator Call(int count)
    {
        Debug.Log($"CallStart...{count},{deck.Count}");

        for (int i = 0; i < count; i++)
        {
            if (deck.Count == 0) DeckReset();

            SoundsManager.Instance.PlaySound("call");
            Debug.Log($"Calling...{deck[0]}::count{i}");
            CardMove(deck[0], CardZone.Hand);
            Debug.Log($"DeckCount::{deck.Count}");
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
