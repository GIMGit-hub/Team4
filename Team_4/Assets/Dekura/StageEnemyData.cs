using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SpownInfo
{
    [Header("Prefab")]
    public GameObject enemyPrefab;

    [Header("個体倍率(%)　(通常：100.0)")]
    public float multiplier = 100.0f;
}


[CreateAssetMenu(fileName = "StageEnemyData", menuName = "Scriptable Objects/StageEnemyData")]
public class StageEnemyData : ScriptableObject
{
    [Header("Stage")]
    public int stage;
    public int floor;

    [Header("Enemy")]
    public List<SpownInfo> enemy;
}
