using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    [Header("StageEnemyData")]
    [SerializeField] private StageEnemyData[] enemyData;
    [SerializeField] private Transform enemySpace;

    private List<Enemy> spawnedEnemy = new List<Enemy>();

    private void Awake()
    {
        
    }

    public void SpawnEnemys()
    {
        Debug.Log("Trying_SpawnEnemys");
        int stage = StageManager.Instance.nowStage;
        int floor = StageManager.Instance.nowFloor;

        for (int i = 0; i < enemyData.Length; i++)
        {
            if(enemyData[i].stage == stage)
                if (enemyData[i].floor == floor)
                {
                    foreach(var info in enemyData[i].enemy)
                    {
                        for (int j = 0; j < enemyData[i].enemy.Count; j++)
                        {
                            Spawn(enemyData[i].enemy.Count, info.enemyPrefab, info.multiplier);
                        }
                    }
                }
        }
    }
    //入れ子キモイから分けた
    private GameObject Spawn(int count,GameObject prefab, float multiplier)
    {
        Debug.Log("Trying_Spawn");
        var obj = Instantiate(prefab, enemySpace);
        var enemy = obj.GetComponent<Enemy>();
        enemy.InitState(new Vector2(0, 0), multiplier);

        spawnedEnemy.Add(enemy);
        return obj;
    }

    /// <summary>
    /// Player->Enemyへの攻撃処理
    /// </summary>
    /// <param name="damage"></param>
    public void TakeDamageRequest(int damage)
    {
        Debug.Log("Trying_TakeDamageRequest");
        //一旦1体だけ
        var enemy = spawnedEnemy[0];
        if (enemy == null) return;

        enemy.TakeDamage(damage);
    }

    /// <summary>
    /// Enemy->Playerへの攻撃要請
    /// </summary>
    /// <param name="damage"></param>
    public void AttackRequest()
    {
        if (spawnedEnemy == null) return;
        for (int i = 0; i < spawnedEnemy.Count; i++)
        {
            Debug.Log($"Trying_{i + 1}_TakeDamageRequest_");
            //一旦1体だけ
            var enemy = spawnedEnemy[i];
            if (enemy == null) return;

            enemy.Attack();
        }
        StageManager.Instance.turnState = TurnState.EnemyTurn_End;
    }
}
