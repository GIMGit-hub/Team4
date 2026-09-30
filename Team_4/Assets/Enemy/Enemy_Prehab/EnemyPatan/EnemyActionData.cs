using UnityEngine;

public enum ActionEffectType
{
    Damage,               // 即座にダメージを与える
    SelfDamageDealtBuff,  // 自分の「次に与えるダメージ」を増やす(%)
}

[CreateAssetMenu(fileName = "EnemyActionData", menuName = "Scriptable Objects/EnemyActionData")]
public class EnemyActionData : ScriptableObject
{
    [Header("表示名")]
    public string actionName;      // 例:「殴り」「力を溜める」

    [Header("効果")]
    public ActionEffectType effectType;
    public float value;            // Damageならダメージ量、Buffなら増加率(%)
    public int duration = 1;       // Buffが何ターン持続するか(Damageは未使用)
}