using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager_Example : MonoBehaviour
{
    [SerializeField] private EnemyController controller;
    [SerializeField] private Player player;

    [Header("全ステージ・全フロアのデータ")]
    [SerializeField] private Enemy_StageData[] allFloors;

    [Header("現在地(デバッグ表示用)")]
    [SerializeField] private int currentStage = 1;
    [SerializeField] private int currentFloor = 1;

    [SerializeField] private ActionAnnounceUI actionAnnounceUI;

    private GameObject selectedTarget;
    private int battleTurnCount = 0;

    private void OnEnable() => SetEventSubscribed(true);
    private void OnDisable() => SetEventSubscribed(false);

    private void Start()
    {
        SetEventSubscribed(true);
        //StartCurrentFloor();
    }

    private void SetEventSubscribed(bool isEnable)
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnTurnChanged -= SwitchTurn;
            if (isEnable) TurnManager.Instance.OnTurnChanged += SwitchTurn;
        }
    }


    // 現在のstage/floorに合うEnemy_StageDataを探す
    private Enemy_StageData FindFloorData(int stage, int floor)
    {
        return allFloors.FirstOrDefault(f => f.stage == stage && f.floor == floor);
    }

    // 現在のフロアの敵を生成する
    public void StartCurrentFloor(int stage,int floor)
    {
        //StageManagerから現在のステージ、フロアを受け取る
        currentFloor = floor;
        currentStage = stage;

        Enemy_StageData data = FindFloorData(currentStage, currentFloor);
        if (data == null)
        {
            Debug.LogWarning($"ステージ{currentStage}-{currentFloor}のデータが見つかりません");
            return;
        }

        Debug.Log($"=== ステージ{currentStage}-{currentFloor} 開始 ===");

        controller.SpawnFloor(data, this, OnFloorClear, actionAnnounceUI);
    }

    // フロアクリア時に呼ばれる(Controllerから)
    private void OnFloorClear()
    {
        Debug.Log($"ステージ{currentStage}-{currentFloor} クリア！");

        //currentFloor++;

        Enemy_StageData next = FindFloorData(currentStage, currentFloor);
        if (next == null)
        {
            Debug.Log($"ステージ{currentStage}の次のフロアがありません(クリアかステージ終了)");
            return;
        }

        //StartCurrentFloor();
    }

    // ステージ選択ボタン(あれば): 指定ステージの1フロア目から開始
    public void OnClickSelectStage(int stage)
    {
        currentStage = stage;
        currentFloor = 1;
        //StartCurrentFloor();
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
        }

        controller.RunAllActions();
    }

    private void SwitchTurn(TurnManager.TurnState turnState)
    {
        switch (turnState)
        {
            case TurnManager.TurnState.PlayerTurn:
                break;

            case TurnManager.TurnState.EnemyTurn:
                StartCoroutine(controller.RunAllActions());
                break;

            default:
                break;
        }

        UiManager.Instance.UpdateUi();
    }
}