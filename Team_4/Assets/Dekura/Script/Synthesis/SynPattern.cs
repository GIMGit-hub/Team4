using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NeedCardDatas
{
    public CardData card_A;
    public CardData card_B;
}

[CreateAssetMenu(fileName = "SynPattern", menuName = "Scriptable Objects/SynPattern")]
public class SynPattern : ScriptableObject
{
    [Header("合成先")]
    public GameObject resultCard;

    [Header("合成元")]
    public List<NeedCardDatas> requiredCards; // 必要なカードのリスト
}
