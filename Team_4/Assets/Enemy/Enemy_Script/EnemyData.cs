using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public GameObject prefab;
    public int maxHp;

    [Header("決められた行動順を繰り返す")]
    public List<EnemyActionData> actions;

    [Header("固有の個性")]
    public float damageDealtGainPerAttack = 0f;//Ver2は攻撃するたび3%上がる
    public float damageTakenReductionPerAttack = 0f; //Ver3用(攻撃するたび被ダメ-3%)

    [Header("戦闘開始時に一度だけ発動")]
    public float battleStartSelfDamageDealtBuffPercent = 0f; // Ver5用: 自分の与ダメが戦闘開始時に上昇(一回限り)
    public float battleStartSelfDamageTakenBuffPercent = 0f; // Ver6用
}