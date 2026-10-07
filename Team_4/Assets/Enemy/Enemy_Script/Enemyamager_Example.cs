using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager_Example : MonoBehaviour
{
    [SerializeField] private EnemyController controller;
    [SerializeField] private Player_Example player;

    [Header("全ステージ・全フロアのデータ")]
    [SerializeField] private Enemy_StageData[] allFloors;

    [Header("現在地(デバッグ表示用)")]
    [SerializeField] private int currentStage = 1;
    [SerializeField] private int currentFloor = 1;

    [SerializeField] private ActionAnnounceUI actionAnnounceUI;

    private GameObject selectedTarget;
    private int battleTurnCount = 0;

    private void Start()
    {
        StartCurrentFloor();
    }

    // 現在のstage/floorに合うEnemy_StageDataを探す
    private Enemy_StageData FindFloorData(int stage, int floor)
    {
        return allFloors.FirstOrDefault(f => f.stage == stage && f.floor == floor);
    }

    // 現在のフロアの敵を生成する
    private void StartCurrentFloor()
    {
        Enemy_StageData data = FindFloorData(currentStage, currentFloor);
        if (data == null)
        {
            Debug.LogWarning($"ステージ{currentStage}-{currentFloor}のデータが見つかりません");
            return;
        }

        Debug.Log($"=== ステージ{currentStage}-{currentFloor} 開始 ===");

       

        controller.SpawnFloor(data, player.TakeDamage,player, this, OnFloorClear,actionAnnounceUI);
    }

    // フロアクリア時に呼ばれる(Controllerから)
    private void OnFloorClear()
    {
        Debug.Log($"ステージ{currentStage}-{currentFloor} クリア！");

        currentFloor++;

        Enemy_StageData next = FindFloorData(currentStage, currentFloor);
        if (next == null)
        {
            Debug.Log($"ステージ{currentStage}の次のフロアがありません(クリアかステージ終了)");
            return;
        }

        StartCurrentFloor();
    }

    // ステージ選択ボタン(あれば): 指定ステージの1フロア目から開始
    public void OnClickSelectStage(int stage)
    {
        currentStage = stage;
        currentFloor = 1;
        StartCurrentFloor();
    }

    public void OnSelectTarget(GameObject target)
    {
        selectedTarget = target;
        Debug.Log($"攻撃対象: {target.name} を選択");
    }

   

    public void OnClickEnemyAction()
    {
        battleTurnCount++;
        Debug.Log($"========== ターン{battleTurnCount} ==========");

        if (selectedTarget == null)
        {
            Debug.Log("攻撃対象が選ばれていません");
        }
        else
        {
            Debug.Log($"[ターン{battleTurnCount}] Player の行動: 攻撃");
            player.Attack(controller, selectedTarget);
        }

        controller.RunAllActions();
    }
}