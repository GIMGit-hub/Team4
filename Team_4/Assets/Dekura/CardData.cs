using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardEffect
{
    public enum EffectType
    {
        Attack,
        Defense,
        Heal,
        Buff,
        Debuff,
        Special
    }

    public EffectType type;
    public int value;
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

    public CardZone cardZone { get; private set; } = CardZone.Collection;

    public string cardName;
    public int cost;
    public List<CardEffect> effects;
}
