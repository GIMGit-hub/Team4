using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum EnemyCategory
{
    Soldier,    // 一般兵 (7種類)
    Captain,    // 隊長兵 (5種類)
    Boss,       // ボス   (2種類)
    FinalBoss   // ラスボス (1種類)
}

[System.Serializable]
public class EnemyGroup
{
    public EnemyCategory category;
    public GameObject[] prefabs;
}

[System.Serializable]
public class SpawnEntry
{
    public EnemyCategory category;
    public int weight = 1; // 大きいほど出やすい
}

// 階層ごとの設定
[System.Serializable]
public class FloorSetting
{
    public string label;
    public int minFloor = 1;     // この階層以上で適用される
    public int minEnemies = 1;   // 1回の生成で出る最小数
    public int maxEnemies = 3;   // 1回の生成で出る最大数
    public SpawnEntry[] entries; // 出る種類と出やすさ
}

public class EnemyCreateManager : MonoBehaviour
{
    private const int MaxSimultaneous = 3; // 同時に存在できる敵の上限

    [Header("種類ごとの敵プレハブ")]
    [SerializeField] private EnemyGroup[] groups;

    [Header("階層ごとの設定")]
    [SerializeField] private FloorSetting[] floors;

    [Header("生成位置")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("デバッグ: 階層レベル")]
    [SerializeField, Min(1)] private int floorLevel = 1;
    [SerializeField] private TMP_Text floorLevelText; // 任意。階層表示用

    private readonly List<GameObject> aliveEnemies = new List<GameObject>();

    private void Start()
    {
        UpdateLevelText();
    }

    // 生成ボタンのOnClickに登録する
    public void OnClickCreate()
    {
        aliveEnemies.RemoveAll(e => e == null);

        FloorSetting setting = GetFloorSetting();
        if (setting == null)
        {
            Debug.LogWarning("この階層に対応する設定がありません");
            return;
        }

        int remain = MaxSimultaneous - aliveEnemies.Count;
        if (remain <= 0)
        {
            Debug.Log("これ以上生成できません(最大数に達しています)");
            return;
        }

        int count = Random.Range(setting.minEnemies, setting.maxEnemies + 1);
        count = Mathf.Min(count, remain);

        int spawned = 0;
        for (int i = 0; i < count; i++)
        {
            GameObject prefab = PickPrefab(setting);
            if (prefab == null)
            {
                Debug.LogWarning("敵のプレハブが選べませんでした(EntriesのWeightやGroupsを確認)");
                continue;
            }

            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // スポーンポイント(Canvas内のImage)の子として生成する
            GameObject enemy = Instantiate(prefab, point);

            // UIの場合は、親の中心に合わせて大きさも整える
            if (enemy.transform is RectTransform rect)
            {
                rect.anchoredPosition = Vector2.zero;
                rect.localScale = Vector3.one;
            }

            aliveEnemies.Add(enemy);
            spawned++;
        }

        Debug.Log($"階層{floorLevel}({setting.label}): {spawned}体生成");
    }

    // 現在の階層に当てはまる設定 (minFloorが階層以下で最大のもの)
    private FloorSetting GetFloorSetting()
    {
        FloorSetting best = null;
        foreach (FloorSetting f in floors)
        {
            if (f.minFloor <= floorLevel && (best == null || f.minFloor > best.minFloor))
            {
                best = f;
            }
        }
        return best;
    }

    // 重みで種類を決め、その種類の中からランダムに1体選ぶ
    private GameObject PickPrefab(FloorSetting setting)
    {
        int total = 0;
        foreach (SpawnEntry e in setting.entries) total += e.weight;
        if (total <= 0) return null;

        int r = Random.Range(0, total);
        EnemyCategory picked = setting.entries[0].category;
        foreach (SpawnEntry e in setting.entries)
        {
            if (r < e.weight)
            {
                picked = e.category;
                break;
            }
            r -= e.weight;
        }

        foreach (EnemyGroup g in groups)
        {
            if (g.category == picked && g.prefabs.Length > 0)
            {
                return g.prefabs[Random.Range(0, g.prefabs.Length)];
            }
        }
        return null;
    }

    // ---- デバッグ用: ボタンのOnClickに登録する ----
    public void DebugLevelUp()
    {
        floorLevel++;
        UpdateLevelText();
    }

    public void DebugLevelDown()
    {
        floorLevel = Mathf.Max(1, floorLevel - 1);
        UpdateLevelText();
    }

    // 本番でゲーム側から階層を渡すときはこれを使う
    public void SetFloorLevel(int level)
    {
        floorLevel = Mathf.Max(1, level);
        UpdateLevelText();
    }

    private void UpdateLevelText()
    {
        if (floorLevelText != null)
        {
            floorLevelText.text = $"階層 {floorLevel}";
        }
    }
}