using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public static EnemyController Instance { get; private set; }

    [Header("全敵データ")]
    [SerializeField] private EnemyData[] enemyDataList;

    [SerializeField] private Transform spawnPoint;

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

    // EnemyUnitから行動の関数を受け取って保存する
    public void Init(Action func)
    {
        receivedFunctions.Add(func);
    }

    // Managerから呼ばれる: 敵を1体生成する
    public void SpawnEnemy(int index, Action<int> dealDamageToTarget)
    {
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

        GameObject go = Instantiate(data.prefab, spawnPoint);

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

        unit.Init(data, this, dealDamageToTarget);

    }

    // Managerから呼ばれる: 好きなタイミングで全員の行動を呼び出す
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

    //playerの攻撃の指示
    public void PlayerActioninstructions()
    {

    }

    //敵死亡時の処理
    public void EnemyDead(GameObject obj)
    {
        foreach (var func in SpawndEnemy.ToList())
        {
            if (func == obj)
            {
                Debug.LogWarning($"{obj}殺した、お前が殺した");
                SpawndEnemy.Remove(func);
                if (SpawndEnemy.Count == 0)
                {
                    EnemyAnnihilation();
                }
                break;
            }

        }
    }

    //敵全滅時の処理
    public void EnemyAnnihilation()
    {

    }
}