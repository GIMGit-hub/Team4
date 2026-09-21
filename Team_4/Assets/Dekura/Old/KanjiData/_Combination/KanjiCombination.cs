using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Combination
{
    public KanjiData kanjiData_A;
    public KanjiData kanjiData_B;
}

[CreateAssetMenu(fileName = "KanjiCombination", menuName = "Scriptable Objects/KanjiCombination")]
public class KanjiCombination : ScriptableObject
{
    public KanjiData.KanjiType type;
    public int value;
    public List<Combination> kanjiData;
}
