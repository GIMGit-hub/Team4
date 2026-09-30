using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public static EnemyController Instance { get; private set; }

    private const int MaxSimultaneous = 3;

    [SerializeField] private Transform[] spawnPoints;

    private readonly List<Action> receivedFunctions = new List<Action>();
    private List<GameObject> SpawndEnemy = new List<GameObject>();

    private Action onFloorClear; // フロアの敵を全滅させたときに呼ぶ

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

    // 変更: フロアのデータをまとめて受け取り、その敵を全部(上限まで)生成する
    public void SpawnFloor(Enemy_StageData floorData, Action<int> dealDamageToTarget, EnemyManager_Example manager, Action onClear)
    {
        Debug.Log($"=== SpawnFloor 呼び出し: ステージ{floorData.stage}-{floorData.floor} 敵数:{floorData.enemy.Count} ===");

        // リセット(前のフロアの残りが無いように)
        foreach (GameObject old in SpawndEnemy.ToList())
        {
            if (old != null) Destroy(old);
        }
        SpawndEnemy.Clear();
        receivedFunctions.Clear();

        onFloorClear = onClear;

        int count = Mathf.Min(floorData.enemy.Count, MaxSimultaneous);
        for (int i = 0; i < count; i++)
        {
            SpawnOne(floorData.enemy[i], dealDamageToTarget, manager);
        }
    }

    // EnemyUnitから呼ばれる: 1つの行動を実行する共通ロジック
    public void ExecuteAction(EnemyActionData action, EnemyUnit self, Action<int> dealDamageToTarget)
    {
        switch (action.effectType)
        {
            case ActionEffectType.Damage:
                int dmg = Mathf.RoundToInt(action.value * self.GetDamageDealtRate());
                dealDamageToTarget?.Invoke(dmg);
                Debug.Log($"{self.name}の{action.actionName}！ {dmg}ダメージ");
                self.OnAfterAttack(); // 攻撃後の特性(3%上昇など)を適用
                break;

            case ActionEffectType.SelfDamageDealtBuff:
                self.AddDamageDealtBuff(action.value / 100f, action.duration);
                Debug.Log($"{self.name}は{action.actionName}！ 次のダメージ+{action.value}%");
                break;
        }
    }

    private void SpawnOne(EnemyData data, Action<int> dealDamageToTarget, EnemyManager_Example manager)
    {
        if (data == null || data.prefab == null)
        {
            Debug.LogWarning("敵データまたはプレハブが設定されていません");
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

        Debug.Log($"{data.enemyName}を{point.name}に生成(現在の生存数:{SpawndEnemy.Count})");
    }

    private Transform GetFreeSpawnPoint()
    {
        List<Transform> free = new List<Transform>();
        foreach (Transform p in spawnPoints)
        {
            bool used = SpawndEnemy.Any(e => e != null && e.transform.parent == p);
            if (!used) free.Add(p);
        }
        if (free.Count == 0) return spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
        return free[UnityEngine.Random.Range(0, free.Count)];
    }

    public void RunAllActions()
    {
        Debug.Log($"=== RunAllActions 呼び出し(登録数:{receivedFunctions.Count}) ===");
        List<Action> snapshot = new List<Action>(receivedFunctions);
        foreach (Action func in snapshot) func?.Invoke();
    }

    public void PlayerAttack(int damage, GameObject target)
    {
        Debug.Log($"=== PlayerAttack 呼び出し(target={target?.name}, damage={damage}) ===");

        if (target == null || !SpawndEnemy.Contains(target))
        {
            Debug.Log("その敵はもういません");
            return;
        }

        EnemyUnit unit = target.GetComponent<EnemyUnit>();
        if (unit == null) return;

        Debug.Log($"プレイヤーの攻撃！ {target.name}に{damage}ダメージ");
        unit.TakeDamage(damage);
    }

    public int GetAliveEnemyCount() => SpawndEnemy.Count;

    public void EnemyDead(GameObject obj)
    {
        if (SpawndEnemy.Remove(obj))
        {
            Debug.LogWarning($"{obj.name}を倒した(残り生存数:{SpawndEnemy.Count})");

            EnemyUnit unit = obj.GetComponent<EnemyUnit>();
            receivedFunctions.RemoveAll(a => a.Target == unit);

            if (SpawndEnemy.Count == 0)
            {
                Debug.Log("フロアの敵を全滅させました");
                onFloorClear?.Invoke();
            }
        }
    }
}