using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardEffect
{
    public enum EffectType
    {
        Attack,             //攻撃
        Defense,            //アーマー取得

        HpHeal,             //HP回復
        CostHeal,           //コスト回復

        AttackBuff,         //攻撃力増幅
        CountBuff,          //効果発動回数
        CostBuff,           //コスト軽減

        Call,               //カードドロー
        AceCall,            //エースカードドロー

        ReceiveDamageUp,    //受けるダメージ上昇(%)
    }
    public enum EffectTarget
    {
        Player,
        Enemy,
        AllEnemy,
    }

    [Header("効果対象")]
    public EffectTarget target;

    [Header("効果内容")]
    public EffectType type;
    public float value;

    [Header("効果回数[攻撃＝攻撃回数、バフ＝ターン数]")]
    public int valueCount = 1;
}

public class CardData : MonoBehaviour
{
    public enum CardType
    {
        DeckCard,
        SynsethisCard
    }

    [Header("カード情報")]
    public CardType cardType;
    public string cardName;
    public int cost;

    [Header("効果種/効果量/発動回数::効果処理順に書くこと！")]
    public List<CardEffect> effects;
    public bool Ace = false;
}
