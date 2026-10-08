using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyController : MonoBehaviour
{
    public static EnemyController Instance { get; private set; }

    private const int MaxSimultaneous = 3;

    [SerializeField] private Transform[] spawnPoints;
    private Vector2[] spawnPointsStartPosition = new Vector2[MaxSimultaneous];

    [SerializeField] private Vector2 spawnPointPosition_0;
    [SerializeField] private Vector2 spawnPointPosition_2;

    private readonly List<Action> receivedFunctions = new List<Action>();
    private List<GameObject> SpawndEnemy = new List<GameObject>();
    private List<GameObject> SpawndDiedEnemy = new List<GameObject>();

    private Action onFloorClear;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        for (int i = 0; i < MaxSimultaneous; i++) 
        {
            spawnPointsStartPosition[i] = spawnPoints[i].GetComponent<RectTransform>().anchoredPosition;
        }
    }

    public void Init(Action func)
    {
        receivedFunctions.Add(func);
    }

    //dealDamageToTargetに加えてplayer本体も受け取る
    public void SpawnFloor(Enemy_StageData floorData, EnemyManager_Example manager, Action onClear, ActionAnnounceUI actionAnnounceUI)
    {
        Debug.Log($"=== SpawnFloor 呼び出し: ステージ{floorData.stage}-{floorData.floor} 敵数:{floorData.enemy.Count} ===");

        foreach (GameObject old in SpawndEnemy.ToList())
        {
            if (old != null) Destroy(old);
        }
        SpawndEnemy.Clear();
        receivedFunctions.Clear();

        onFloorClear = onClear;

        int count = Mathf.Min(floorData.enemy.Count, MaxSimultaneous);
        Debug.Log($"aaaaaaaaa{count}");
        if (count == 2)
        {
            spawnPoints[0].GetComponent<RectTransform>().anchoredPosition = spawnPointPosition_0;
            //spawnPoints[1].GetComponent<GameObject>().SetActive(false);
            spawnPoints[2].GetComponent<RectTransform>().anchoredPosition = spawnPointPosition_2;
        }
        else
        {
            //spawnPoints[1].GetComponent<GameObject>().SetActive(true);
            for (int i = 0; i < count; i++)
            {
                spawnPoints[i].GetComponent<RectTransform>().anchoredPosition = spawnPointsStartPosition[i];
            }
        }

        for (int i = 0; i < count; i++)
        {
            Debug.Log($"{floorData.enemy[i]},{GetFreeSpawnPoint(i + 1, count)}");
            SpawnOne(floorData.enemy[i], manager, actionAnnounceUI, GetFreeSpawnPoint(i + 1, count));
        }
    }

    //playerを受け取り、Initとターン実行に渡す
    private void SpawnOne(EnemyData data, EnemyManager_Example manager, ActionAnnounceUI announceUI, Transform point)
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

        GameObject go = Instantiate(data.prefab, point);

        if (go.transform is RectTransform rect)
        {
            rect.anchoredPosition = Vector2.zero;
        }

        EnemyUnit unit = go.GetComponent<EnemyUnit>();
        if (unit == null)
        {
            Debug.LogWarning($"{data.enemyName}のプレハブにEnemyUnitが付いていません");
            return;
        }

        unit.Init(data, this, manager, announceUI);
        SpawndEnemy.Add(go);

        Debug.Log($"{data.enemyName}を{point.name}に生成(現在の生存数:{SpawndEnemy.Count})");
    }
    private Transform GetFreeSpawnPoint(int count, int enemyCount) => (enemyCount, count) switch
    {
        (1, 1) => spawnPoints[1],
        (2, 1) => spawnPoints[0],
        (2, 2) => spawnPoints[2],
        (3 ,1) => spawnPoints[0],
        (3 ,2) => spawnPoints[1],
        (3 ,3) => spawnPoints[2],
        _      => null,
    };
    //private Transform oldGetFreeSpawnPoint(int count, int enemyCount)
    //{
    //    List<Transform> free = new List<Transform>();
    //    foreach (Transform p in spawnPoints)
    //    {
    //        bool used = SpawndEnemy.Any(e => e != null && e.transform.parent == p);
    //        if (!used) free.Add(p);
    //    }
    //    if (free.Count == 0) return spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
    //    return free[UnityEngine.Random.Range(0, free.Count)];
    //}

    //playerを受け取り、TargetDamageDealtDebuffに対応
    public IEnumerator ExecuteAction(EnemyActionData action, EnemyUnit self)
    {
        //Debug.Log($"{self.name}の{action.actionName}");

        List<ActionEffect> effectsToRun = new();

        //ランダム行動なら、1つだけ抽選する
        if (action.isRandomPick && action.effects.Count > 0)
        {
            ActionEffect picked =
                action.effects[UnityEngine.Random.Range(0, action.effects.Count)];

            effectsToRun = new List<ActionEffect> { picked };
        }
        else
        {
            effectsToRun = action.effects;
        }

        foreach (ActionEffect effect in effectsToRun)
        {
            switch (effect.effectType)
            {
                case ActionEffectType.Damage:
                    for (int i = 0; i < Mathf.Max(1, effect.hitCount); i++)
                    {
                        float dmg = effect.value * self.GetDamageDealtRate() + self.GetFixedDamageBonus();
                        Player.Instance.TakeDamage(dmg);
                        Debug.Log($"    → {dmg}ダメージ");
                        self.OnAfterAttack();
                    }
                    break;

                case ActionEffectType.SelfDamageDealtBuff:
                    self.AddDamageDealtBuff(effect.value / 100f, effect.duration);
                    Debug.Log($"  → 次のダメージ+{effect.value}%");
                    break;

                case ActionEffectType.SelfDamageTakenBuff:
                    self.AddDamageTakenBuff(effect.value / 100f, effect.duration);
                    Debug.Log($"  → 被ダメージ-{effect.value}%");
                    break;

                case ActionEffectType.TargetDamageDealtDebuff:
                    //player.ApplyDamageDealtDebuff(effect.value, effect.duration);
                    Debug.Log($"  → 相手の与ダメ-{effect.value}%");
                    break;

                case ActionEffectType.Heal:
                    self.Heal(Mathf.RoundToInt(effect.value));
                    Debug.Log($"  → {effect.value}回復");
                    break;

                case ActionEffectType.TargetDamageTakenDebuff:
                    //player.ApplyDamageTakenDebuff(effect.value, effect.duration);
                    Debug.Log($"  → 相手の被ダメ+{effect.value}%");
                    break;

                case ActionEffectType.SelfDamage: 
                    self.TakeSelfDamage(Mathf.RoundToInt(effect.value));
                    Debug.Log($"    → 自身に{effect.value}ダメージ(反動)");
                    break;

                case ActionEffectType.TargetMaxHpReduction:
                    //player.ReduceMaxHp(Mathf.RoundToInt(effect.value));
                    Debug.Log($"    → 相手の最大HPを{effect.value}減らした");
                    break;

                case ActionEffectType.SetTargetHpToOne:
                    //player.SetHpToOne();
                    Debug.Log($"    → 相手の体力を1にした");
                    break;


                case ActionEffectType.DoNothing:
                    break;
            }

            yield return new WaitForSeconds(1);
        }
    }

    public IEnumerator RunAllActions()
    {
        Debug.Log($"=== RunAllActions 呼び出し(登録数:{receivedFunctions.Count}) ===");
        List<Action> snapshot = new List<Action>(receivedFunctions);
        foreach (Action func in snapshot)
        {
            func?.Invoke();
            yield return new WaitForSeconds(1f);
        }
        TurnManager.Instance.TurnChange();
    }

    public bool PlayerAttack(float damage, EnemyUnit unit)
    {
        //Debug.Log($"=== PlayerAttack 呼び出し(target={target?.name}, damage={damage}) ===");

        //if (target == null || !SpawndEnemy.Contains(target))
        //{
        //    Debug.Log("その敵はもういません");
        //    return;
        //}

        //EnemyUnit unit = target.GetComponent<EnemyUnit>();
        GameObject target = unit.gameObject;
        if (unit == null || unit.isDead)  return false;

        Debug.Log($"プレイヤーの攻撃！ {target.name}に{damage}ダメージ");
        unit.TakeDamage(damage);
        return true;
    }

    public void PlayerAttackAll(float damage)
    {
        Debug.Log($"プレイヤーの攻撃！ 全員に{damage}ダメージ");
        foreach (var enemy in SpawndEnemy.ToList())
        {
            EnemyUnit unit = enemy.GetComponent<EnemyUnit>();

            if (unit == null || unit.isDead) continue;
            unit.TakeDamage(damage);
        }
    }

    public int GetAliveEnemyCount() => SpawndEnemy.Count;

    public void EnemyDead(GameObject obj)
    {
        if (SpawndEnemy.Contains(obj))
        {
            Debug.LogWarning($"{obj.name}を倒した(残り生存数:{SpawndEnemy.Count})");

            SpawndDiedEnemy.Add(obj);

            EnemyUnit unit = obj.GetComponent<EnemyUnit>();
            receivedFunctions.RemoveAll(a => a.Target == unit);

            if (SpawndEnemy.Count == SpawndDiedEnemy.Count) 
            {
                Debug.Log("フロアの敵を全滅させました");
                AllEnemyDead();
            }
        }
    }

    public void AllEnemyDead()
    {
        foreach(var enemy in SpawndEnemy.ToList())
        {
            Destroy(enemy);
        }

        SpawndEnemy.Clear();
        SpawndDiedEnemy.Clear();
        onFloorClear?.Invoke();
    }
}