using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardEffect
{
    public enum EffectType
    {
        Attack,
        Defense,

        HpHeal,
        CostHeal,

        AttackBuff,
        CountBuff,
        CostBuff,

        Call,
        AceCall,
    }
    
    public EffectType type;
    public float value;
    public int valueCount = 1;
}

public class CardData : MonoBehaviour
{
    public enum CardZone
    {
        Collection,
        Hand,
        Deck,
        Discard
    }

    [Header("カード情報")]
    public string cardName;
    public int cost;

    [Header("効果種/効果量/発動回数::効果処理順に書くこと！")]
    public List<CardEffect> effects;

    public CardZone cardZone { get; private set; } = CardZone.Collection;
    public bool Ace { get; private set; } = false;

    public void SetZone(CardZone zone) { cardZone = zone; }
    public void SetAce() {  Ace = true; }
}
