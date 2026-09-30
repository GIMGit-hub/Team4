using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Enemy_StageData", menuName = "Scriptable Objects/Enemy_StageData")]
public class Enemy_StageData : ScriptableObject
{
    [Header("Stage")]
    public int stage;
    public int floor;

    [Header("Enemy")]
    public List<EnemyData> enemy;
}
