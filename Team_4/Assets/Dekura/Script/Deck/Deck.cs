using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Deck", menuName = "Scriptable Objects/Deck")]
public class Deck : ScriptableObject
{
    [System.Serializable]
    public class DeckCard
    {
        public CardData card;
        public int count;
    }

    public string deckName;
    public List<DeckCard> deckCards;
}
