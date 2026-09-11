using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    [Header("StageEnemyData")]
    [SerializeField] private StageEnemyData[] enemyData;
    [SerializeField] private Transform enemySpace;

    [Header("SpownSetting")]
    [SerializeField] private float spacing = 2.0f;

    private List<Enemy> spawnedEnemy = new List<Enemy>();
    private List<Enemy> deadEnemy = new List<Enemy>();
    public event Action OnAllEnemiesDead;

    private Enemy selectedEnemy;

    private void Awake()
    {
    }

    public void SpawnEnemys(int stage, int floor)
    {
        Debug.Log("Trying_SpawnEnemys");

        for (int i = 0; i < enemyData.Length; i++)
        {
            if (enemyData[i].stage == stage && enemyData[i].floor == floor)
            {
                int count = 0;
                int enemyCount = enemyData[i].enemy.Count;

                foreach (var info in enemyData[i].enemy)
                {
                    Spawn(count, enemyCount, info.enemyPrefab, info.multiplier);
                    count++;
                }
            }
        }


    }
    //入れ子キモイから分けた
    private GameObject Spawn(int count, int enemyCount, GameObject prefab, float multiplier)
    {
        Debug.Log($"Trying_Spawn{count},{enemyCount}");
        var obj = Instantiate(prefab, enemySpace);
        var enemy = obj.GetComponent<Enemy>();

        float totalWidth = (enemyCount - 1) * spacing;
        float startX = -totalWidth / 2f;
        float x = startX + count * spacing;

        obj.transform.localPosition = new Vector2(x, 0f);
        enemy.InitState(multiplier);
        enemy.OnDeath += EnemyDeath;

        spawnedEnemy.Add(enemy);
        if (count == 0)
        {
            selectedEnemy = enemy;
            selectedEnemy.Selected(true);
        }

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
        for (int i = 0; i < spawnedEnemy.Count; i++) 
        {
            var enemy = spawnedEnemy[i];
            if (enemy != selectedEnemy) continue;
            if (enemy.IsDead) continue;

            enemy.TakeDamage(damage);
            return;
        }
    }

    /// <summary>
    /// Enemy->Playerへの攻撃要請
    /// </summary>
    /// <param name="damage"></param>
    public IEnumerator AttackRequest()
    {
        if (spawnedEnemy == null) yield break;

        UiManager_MainGame.Instance.SetDiyalog("あいてのこうげき");
        for (int i = 0; i < spawnedEnemy.Count; i++)
        {
            Debug.Log($"Trying_{i + 1}_TakeDamageRequest_");

            var enemy = spawnedEnemy[i];
            if (enemy.IsDead) continue;

            enemy.Attack();

            yield return new WaitForSeconds(0.2f);
        }
        //のち、全員のアタック終了時に変更
        //いまは全員のアタック要請時
        StageManager.Instance.turnState = TurnState.EnemyTurn_End;
    }

    /// <summary>
    /// Enemy死亡時処理
    /// </summary>
    private void EnemyDeath()
    {
        bool allDead = spawnedEnemy.TrueForAll(e => e == null || e.IsDead);
        if (allDead) StartCoroutine(AllDead());

        if (selectedEnemy == null || selectedEnemy.IsDead)
        {
            foreach (var enemy in spawnedEnemy)
            {
                if (enemy != null && !enemy.IsDead)
                {
                    SetSelectedEnemy(enemy);
                    return;
                }
            }
        }
    }

    private IEnumerator AllDead()
    {
        Debug.LogWarning("enemy_allDead");

        yield return new WaitForSeconds(0.5f);

        foreach (var e in spawnedEnemy)
        {
            if (e != null) Destroy(e.gameObject);
        }
        spawnedEnemy.Clear();
        OnAllEnemiesDead?.Invoke();
    }

    public void SetSelectedEnemy(Enemy enemy)
    {
        if (selectedEnemy != null) selectedEnemy.Selected(false);
        selectedEnemy = enemy;
        selectedEnemy.Selected(true);
    }
}
