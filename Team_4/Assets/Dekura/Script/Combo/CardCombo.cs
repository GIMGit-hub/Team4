using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardCombo", menuName = "Scriptable Objects/CardCombo")] 
public class CardCombo : ScriptableObject
{
    [System.Serializable]
    public class Combos
    {
        public CardData card_A;
        public CardData card_B;
        public GameObject resultCard;
    }

    public List<Combos> combos;
}
