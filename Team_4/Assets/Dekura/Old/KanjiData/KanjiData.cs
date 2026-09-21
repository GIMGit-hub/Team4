using UnityEngine;

[CreateAssetMenu(fileName = "KanjiData", menuName = "Scriptable Objects/KanjiData")]
public class KanjiData : ScriptableObject
{
    public enum KanjiType
    {
        Attack,
        Heal_Hp,
        Heal_Debuff,
        Buff,
        Debuff,
    }

    [Header("KanjiState")]
    public string kanjiName;
    public KanjiType kanjiType;
    public float value;
}
