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
    public float damageDealtBonusPerAliveEnemyPercent = 0f; //Ver10用

    [Header("攻撃を受ける度与ダメ上昇(上限あり//Ver12用)")]
    public float damageDealtGainPerHitTaken = 0f;      //1回の被弾につき増える%
    public float damageDealtGainPerHitTakenCap = 0f;   //増加の上限%

    [Header("戦闘開始時に一度だけ発動")]
    public float battleStartSelfDamageDealtBuffPercent = 0f; // Ver5用: 自分の与ダメが戦闘開始時に上昇(一回限り)
    public float battleStartSelfDamageTakenBuffPercent = 0f; // Ver6用

    [Header("毎ターン開始時に発動")]
    public float turnStartTargetDamageDealtDebuffPercent = 0f; //Ver8用
    public int turnStartHeal = 0; //Ver9用
    public float turnStartSelfDamageDealtBuffPercent = 0f; //Ver13用
    public int turnStartSelfBuffMaxTurns = 0; //何ターン目まで発動するか
}