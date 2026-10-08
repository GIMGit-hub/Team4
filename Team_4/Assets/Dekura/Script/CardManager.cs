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

/// <summary>
/// カードの特殊効果を実行するためのインターフェース
/// インターフェース:
///     クラスに対して、このような処理をもつと宣言するもの
///      >>>中身が全く違う処理でも、同じ名前で呼び出せる
/// </summary>
public interface ICardSpecialAction 
{
    IEnumerator SpecialAction(EnemyUnit enemy = null);
}

/// <summary>
/// カードの実体
/// これを使ってカードの情報をやり取りする
/// </summary>
[System.Serializable]
public class CardInstance
{
    public string instanceId;
    public GameObject template;      // どの種類か(プレハブの参照、Instantiateはしない)
    public CardZone zone;
    public Vector2? sponePosition;

    public CardData cardData => template.GetComponent<CardData>();
    public bool AceCard => cardData.Ace;
    public int Cost => cardData.cost;
    public string CardName => cardData.cardName;
    public CardData.CostType costType => cardData.costType;
   
}

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [Header("合成のリスト")]
    [SerializeField] private List<SynPattern> synthesisList = new();

    [Header("カード使用判定")]
    [SerializeField] public RectTransform hitColision;

    [Header("カード使用時の効果処理遅延")]
    [SerializeField] public float activateDuration = 0.5f;

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

    public void HandReset()
    {
        Debug.Log("HandReset...");
        HandLayout handLayout = FindAnyObjectByType<HandLayout>();

        foreach (var card in hand.ToList())
        {
            handLayout.GetActiveCard(card).GetComponent<CardController>().GotoDiscard(() =>
            {
                CardMove(card, CardZone.Discard);
            });
        }
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

    public bool UseCard(CardInstance instance, EnemyUnit enemy = null)
    {
        //コスト支払い可能かの確認
        CardData.CostType costType = instance.template.GetComponent<CardData>().costType;
        int needCost = instance.Cost;

        if (!Player.Instance.CanUseCost(costType, needCost)) return false;

        Debug.Log($"needCost::{needCost}");
        Debug.Log($"{instance.CardName}::発動準備");

        //特殊効果を持つカードか確認
        ICardSpecialAction cardSpecialAction = instance.template.GetComponent<ICardSpecialAction>();
        if (cardSpecialAction == null) 
        {
            //効果をqueに入れてく
            foreach (var effects in instance.template.GetComponent<CardData>().effects)
            {
                queue.Enqueue(effects);
            }
        }

        //コスト支払い
        Player.Instance.UseCost(costType, needCost);

        //カードの移動、合成カードは削除
        if (instance.template.GetComponent<CardData>().cardType == CardData.CardType.DeckCard)
            CardMove(instance, CardZone.Discard);
        else
        {
            GetZoneList(instance.zone).Remove(instance);
            OnCardMoved?.Invoke(instance, CardZone.Discard);
        }

        //効果発動
        StartCoroutine(CardActivation(enemy, cardSpecialAction));
        return true;
    }

    public void ConvertCost(CardInstance instance)
    {
        //カードの移動、合成カードは削除
        if (instance.template.GetComponent<CardData>().cardType == CardData.CardType.DeckCard)
            CardMove(instance, CardZone.Discard);
        else
        {
            GetZoneList(instance.zone).Remove(instance);
            OnCardMoved?.Invoke(instance, CardZone.Discard);
        }

        StartCoroutine(CostHeal(1));

        //カード使用しましたよ～_OnCardUsed発火
        OnCardUsed?.Invoke();
    }


    private IEnumerator CardActivation(EnemyUnit enemy, ICardSpecialAction cardSpecialAction = null)
    {
        if (cardSpecialAction != null) 
        {
            Debug.Log($"特殊効果待機");
            yield return StartCoroutine(cardSpecialAction.SpecialAction(enemy));
            cardSpecialAction = null;
        }
        else
        {
            //queが残り無ければ終了
            if (queue.Count == 0) yield break;

            //データを取得して、そのqueを解除
            CardEffect effect = queue.Dequeue();

            switch (effect.type)
            {
                case CardEffect.EffectType.Attack:
                    yield return StartCoroutine(Attack(effect.target, effect.value, effect.valueCount, enemy));
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
                case CardEffect.EffectType.CostFree:
                    yield return StartCoroutine(AddBuff(effect.type, effect.value, effect.valueCount));
                    break;
                case CardEffect.EffectType.Call:
                    yield return StartCoroutine(Call((int)effect.value));
                    break;
                case CardEffect.EffectType.AceCall:
                    yield return StartCoroutine(AceCall());
                    yield return StartCoroutine(AddBuff(CardEffect.EffectType.CostBuff_Ace, 1, 1));
                    break;

                case CardEffect.EffectType.HpHeal_damage:
                    yield return StartCoroutine(HpHeal_damage(effect.value));
                    break;
                case CardEffect.EffectType.HpHeal_count:
                    yield return StartCoroutine(HpHeal_count(effect.value, effect.valueCount));
                    break;
            }
        }

        //カード使用しましたよ～_OnCardUsed発火
        OnCardUsed?.Invoke();
        Player.Instance.AddUseCardCount();
        yield return CardActivation(enemy);
    }

    //---------------------------------カード使用時の処理-------------------------------------//

    private void CardMove(CardInstance instance, CardZone zone)
    {
        //Listを取得、移動元のListから削除、移動先のListに追加、カードのzoneを更新
        if (GetZoneList(instance.zone).Contains(instance) != false) 
            GetZoneList(instance.zone).Remove(instance);

        GetZoneList(zone).Add(instance);
        instance.zone = zone;

        OnCardMoved?.Invoke(instance, zone);
    }

    public IEnumerator Attack(CardEffect.EffectTarget target, float value, int count, EnemyUnit enemy)
    {
        Player.Instance.Attack(target, value, count, enemy);
        yield return new WaitForSeconds(activateDuration);
    }

    public IEnumerator HpHeal(float value)
    {
        Debug.Log($"HpHeal...{value}");

        Player.Instance.HpHeal(value);
        yield return new WaitForSeconds(activateDuration);
    }
    public IEnumerator DpHeal(float value)
    {
        Debug.Log($"DpHeal...{value}");

        Player.Instance.DpHeal(value);
        yield return new WaitForSeconds(activateDuration);
    }
    public IEnumerator CostHeal(int value)
    {
        Debug.Log($"CostHeal...{value}");

        Player.Instance.CostHeal(value);
        yield return new WaitForSeconds(activateDuration);
    }

    public IEnumerator AddBuff(CardEffect.EffectType m_type, float m_value, int m_enableTurn) 
    {
        Player.Instance.AddEffect_Turn(m_type, m_value, m_enableTurn);
        yield return new WaitForSeconds(activateDuration);
    }

    public IEnumerator Call(int count)
    {
        Debug.Log($"CallStart...{count},{deck.Count}");

        for (int i = 0; i < count; i++)
        {
            if (deck.Count == 0) DeckReset();

            SoundsManager.Instance.PlaySound("call");
            Debug.Log($"Calling...{deck[0]}::count{i}");

            //deck先頭(山上)を手札に移動
            CardMove(deck[0], CardZone.Hand);
            Debug.Log($"DeckCount::{deck.Count}");
            yield return new WaitForSeconds(0.1f);
        }
    }

    public IEnumerator AceCall()
    {
        Debug.Log($"AceCallStart...");

        //デッキにエースカードがあれば手札に移動、無ければ捨て札から探す
        foreach (var card in deck.ToList())
        {
            if (card.AceCard) 
            {
                SoundsManager.Instance.PlaySound("call");
                Debug.Log($"AceCalling...");
                CardMove(card, CardZone.Hand);
                Debug.Log($"DeckCount::{deck.Count}");
                yield return new WaitForSeconds(0.1f);
                yield break;
            }
        }
        foreach (var card in discard.ToList())
        {
            if (card.AceCard)
            {
                SoundsManager.Instance.PlaySound("call");
                Debug.Log($"AceCalling...");
                CardMove(card, CardZone.Hand);
                Debug.Log($"DeckCount::{deck.Count}");
                yield return new WaitForSeconds(0.1f);
                yield break;
            }
        }
        yield return new WaitForSeconds(0.1f);
    }

    public IEnumerator HpHeal_damage(float value)
    {
        Debug.Log($"HpHeal_damage...{value}");
        Player.Instance.HpHeal_damage(value);
        yield return new WaitForSeconds(0.1f);
    }
    public IEnumerator HpHeal_count(float value, int count)
    {
        Debug.Log($"HpHeal_count...{value}::count{count}");
        Player.Instance.HpHeal_count(value, count);
        yield return new WaitForSeconds(0.1f);
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

        foreach (var pattern in synthesisList)
        {
            foreach (var needCard in pattern.requiredCards)
            {
                if ((needCard.card_A.cardName == card_A.CardName && needCard.card_B.cardName == card_B.CardName)||
                    (needCard.card_A.cardName == card_B.CardName && needCard.card_B.cardName == card_A.CardName))
                {
                    result = pattern.resultCard;
                    break;
                }
            }
        }
        if (result == null)
        {
            Debug.LogWarning($"Synthesis failed: {card_A.CardName} + {card_B.CardName}");
            return null;
        }

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

    public List<GameObject> GetCandidates(CardInstance card)
    {
        // Implementation for getting synthesis candidates
        List<GameObject> candidates = new List<GameObject>();

        foreach (var pattern in synthesisList)
        {
            foreach (var needCard in pattern.requiredCards)
            {
                if (needCard.card_A.cardName == card.CardName) candidates.Add(needCard.card_B.cardPrefab);
                else if (needCard.card_B.cardName == card.CardName) candidates.Add(needCard.card_A.cardPrefab);
            }
        }

        return candidates;
    }

    public List<CardInstance> GetZoneList(CardZone zone) => zone switch
    {
        CardZone.Hand => hand,
        CardZone.Deck => deck,
        CardZone.Discard => discard,
        _ => null
    };
}
