using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public static EnemyController Instance { get; private set; }

    private const int MaxSimultaneous = 3;

    [Header("全敵データ")]
    [SerializeField] private EnemyData[] enemyDataList;

    [SerializeField] private Transform[] spawnPoints;

    private readonly List<Action> receivedFunctions = new List<Action>();
    private List<GameObject> SpawndEnemy = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    public void Init(Action func)
    {
        receivedFunctions.Add(func);
    }

    public void SpawnEnemy(int index, Action<int> dealDamageToTarget, EnemyManager_Example manager)
    {
        if (SpawndEnemy.Count >= MaxSimultaneous)
        {
            Debug.Log("これ以上生成できません(最大数に達しています)");
            return;
        }

        if (index < 0 || index >= enemyDataList.Length)
        {
            Debug.LogWarning("その番号の敵データがありません");
            return;
        }

        EnemyData data = enemyDataList[index];

        if (data.prefab == null)
        {
            Debug.LogWarning($"{data.enemyName}にプレハブが設定されていません");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("Spawn Pointsが設定されていません");
            return;
        }

        Transform point = GetFreeSpawnPoint();
        GameObject go = Instantiate(data.prefab, point);

        if (go.transform is RectTransform rect)
        {
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        EnemyUnit unit = go.GetComponent<EnemyUnit>();
        if (unit == null)
        {
            Debug.LogWarning($"{data.enemyName}のプレハブにEnemyUnitが付いていません");
            return;
        }

        unit.Init(data, this, dealDamageToTarget, manager);

        SpawndEnemy.Add(go);
    }

    // 空いているスポーンポイントを優先して選ぶ
    private Transform GetFreeSpawnPoint()
    {
        List<Transform> free = new List<Transform>();
        foreach (Transform p in spawnPoints)
        {
            bool used = false;
            foreach (GameObject enemy in SpawndEnemy)
            {
                if (enemy != null && enemy.transform.parent == p)
                {
                    used = true;
                    break;
                }
            }
            if (!used) free.Add(p);
        }

        if (free.Count == 0) return spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
        return free[UnityEngine.Random.Range(0, free.Count)];
    }

    public void RunAllActions()
    {
        List<Action> snapshot = new List<Action>(receivedFunctions);
        foreach (Action func in snapshot)
        {
            func?.Invoke();
        }
    }

    public int DebugGetDataCount()
    {
        return enemyDataList != null ? enemyDataList.Length : -1;
    }

    // プレイヤーの攻撃を受け取り、指定された敵(GameObject)にダメージを与える
    public void PlayerAttack(int damage, GameObject target)
    {
        if (target == null || !SpawndEnemy.Contains(target))
        {
            Debug.Log("その敵はもういません");
            return;
        }

       
        EnemyUnit unit = target.GetComponent<EnemyUnit>();
        //unit?.TakeDamage(damage);

        Debug.Log($"プレイヤーの攻撃！ {target.name}に{damage}ダメージ");
        unit.TakeDamage(damage);

    }

    public int GetAliveEnemyCount()
    {
        return SpawndEnemy.Count;
    }

    public void EnemyDead(GameObject obj)
    {
        foreach (var func in SpawndEnemy.ToList())
        {
            if (func == obj)
            {
                Debug.LogWarning($"{obj}殺した、お前が殺した");
                SpawndEnemy.Remove(func);

                EnemyUnit unit = obj.GetComponent<EnemyUnit>();
                receivedFunctions.RemoveAll(a => a.Target == unit);

                if (SpawndEnemy.Count == 0)
                {
                    EnemyAnnihilation();
                }
                break;
            }
        }
    }

    public void EnemyAnnihilation()
    {

    }
}