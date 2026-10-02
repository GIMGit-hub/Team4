using UnityEngine;
using System.Collections.Generic;

public enum ActionEffectType
{
    Damage,
    SelfDamageDealtBuff,
    SelfDamageTakenBuff,
    TargetDamageDealtDebuff,
    Heal,
    DoNothing
}

// 1つの効果(ダメージ、バフなど)
[System.Serializable]
public class ActionEffect
{
    public ActionEffectType effectType;
    public float value;
    public int duration = 1;
}

[CreateAssetMenu(fileName = "EnemyActionData", menuName = "Scriptable Objects/EnemyActionData")]
public class EnemyActionData : ScriptableObject
{
    [Header("表示名")]
    public string actionName;

    [Header("効果(複数持てる)")]
    public List<ActionEffect> effects;
}