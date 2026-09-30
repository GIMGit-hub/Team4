using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public GameObject prefab;
    public int maxHp;

    [Header("Œˆ‚ß‚ç‚ê‚½s“®‡‚ğŒJ‚è•Ô‚·")]
    public List<EnemyActionData> actions;

    [Header("ŒÅ—L‚ÌŒÂ«")]
    public float damageDealtGainPerAttack = 0f;//Ver2‚ÍUŒ‚‚·‚é‚½‚Ñ3%ã‚ª‚é
}