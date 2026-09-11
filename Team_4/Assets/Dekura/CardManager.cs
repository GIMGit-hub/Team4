using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [Header("全部のカード(コンボカードもここにいれる)")]
    [SerializeField] private List<GameObject> allCardList = new();
    private List<CardData> allCardData = new();

    public event System.Action<CardData> OnCardMoved;
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
            if (data != null) allCardData.Add(data);
        }
    }

    public GameObject Synthesis(GameObject card_A, GameObject card_B)
    {
        foreach (var card in allCardData)
        {
            if(card.cardName == "promo_Syn") return card.gameObject;
        }

        return null;
    }




    /// <summary>
    /// カードデータ取得
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public CardData GetCardData(string name)
    {
        foreach (var data in allCardData)
        {
            if (data.cardName == name) return data;
        }
        return null;
    }

    /// <summary>
    /// カード強化処理
    /// shopで使ってくれや
    /// </summary>
    /// <param name="name">カードの名前</param>
    /// <param name="type">効果の種類</param>
    /// <param name="value">上昇数値</param>
    public void UpgradeCardData(string name, CardEffect.EffectType type, int value)
    {
        foreach (var data in allCardData)
        {
            if (data.cardName != name) continue;

            foreach (var effect in data.effects)
            {
                if (effect.type == type)
                {
                    effect.value += value;
                    continue;
                }
            }
        }
    }
}
