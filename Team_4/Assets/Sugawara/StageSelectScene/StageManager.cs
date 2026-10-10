using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static TurnManager;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("進行状況")]
    [SerializeField] private int currentStage = 1;
    [SerializeField] private int currentFloor = 1;

    [Header("ステージごとの最大フロア数(Index0=ステージ1)")]
    [SerializeField] private int[] maxFloorPerStage = { 3, 3, 3 };
    [SerializeField] private bool[] isFloorCleared;
    [SerializeField] private Sprite[] floorImage;

    [Header("最大ステージ数")]
    [SerializeField] private int maxStage = 3;

    [Header("シーン名")]
    [SerializeField] private string mainSceneName = "MainGame";
    [SerializeField] private string stageSelectSceneName = "StageSelectScene";
    [SerializeField] private string gameClearSceneName = "ClearScene";

    [Header("選択中のデッキ")]
    [SerializeField] private Deck selectedDeck;

    [SerializeField] private bool isTutorialed = false;
    public bool IsTutorialed { get=>isTutorialed;set=> isTutorialed=value;}

    public int CurrentStage => currentStage;
    public int CurrentFloor => currentFloor;

    public Deck SelectedDeck => selectedDeck;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        //多分よくない
        if (SceneManager.GetActiveScene().name == mainSceneName)
            SceneManager.LoadScene(mainSceneName);
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    // メインシーンに入ったら、自分の持つ階層をEnemyManagerに渡す
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != mainSceneName) return;

        Debug.Log($"[検証] OnSceneLoaded時点のcurrentStage={currentStage}, currentFloor={currentFloor}"); // 追加

        SoundsManager.Instance.PlayBGM("battle");

        if (Player.Instance != null && selectedDeck != null)
        {
            Player.Instance.deck = selectedDeck;
        }

        EnemyManager_Example manager = FindFirstObjectByType<EnemyManager_Example>();
        if (manager == null)
        {
            Debug.LogWarning("EnemyManagerがシーン内に見つかりません");
            return;
        }

        UiManager.Instance.BackGround = floorImage[currentStage - 1];

        Debug.Log($"=== ステージ{currentStage}-{currentFloor} を開始(StageManagerより) ===");
        manager.StartCurrentFloor(currentStage, currentFloor);
    }

    public void SelectDeck(Deck deck)
    {
        selectedDeck = deck;

        if (selectedDeck != null)
        {
            Debug.Log($"デッキ選択: {selectedDeck.deckName}");
        }
    }

    // ステージ選択シーンのボタンから呼ぶ
    public void SelectStage(int stage)
    {
        Debug.Log($"[検証] SelectStage呼び出し: 引数stage={stage}"); // 追加

        currentStage = Mathf.Clamp(stage, 1, maxStage);
        currentFloor = 1;
        SceneManager.LoadScene(mainSceneName);
    }

    // EnemyManagerから、敵全滅時に呼ばれる
    public void OnFloorClear()
    {
        int maxFloor = GetMaxFloor(currentStage);
        currentFloor++;

        if (currentFloor > maxFloor)
        {
            OnStageClear();
            return;
        }

        StartCoroutine(ProceedDirection());
    }
    //クリアしたときの処理
    private void OnStageClear()
    {
        Debug.Log($"=== ステージ{currentStage} クリア！ ===");

        isFloorCleared[currentStage - 1] = true;

        StartCoroutine(StageClearDirection());
    }

    private IEnumerator ProceedDirection()
    {
        int next = currentFloor + 1;
        Coroutine uiCoroutine = StartCoroutine(UiManager.Instance.ProceedDirection(currentStage, next));

        if (next == GetMaxFloor(currentStage))
        {
            Debug.Log($"BOSS-STAGE");
        }
        else
        {
            Debug.Log($"{currentStage}-{next}");
        }

        yield return uiCoroutine;
        
        //演出終了後に生成開始命令
        EnemyManager_Example manager = FindFirstObjectByType<EnemyManager_Example>();
        manager?.StartCurrentFloor(currentStage, currentFloor);
        TurnManager.Instance.TurnChange(TurnState.PlayerTurn);
    }

    private IEnumerator StageClearDirection()
    {
        Debug.Log($"STAGE_CLEAR!");
        if (CheckFloorClear()) SoundsManager.Instance.StopBGM();

        //クリア演出等をここに
        yield return StartCoroutine(ClearManager.Instance.PlayClear());

        InitStagePosition();
        if (CheckFloorClear()) SceneManager.LoadScene(gameClearSceneName);
        else  SceneManager.LoadScene(stageSelectSceneName);
    }

    public IEnumerator PlayerDead()
    {
        yield return  StartCoroutine(ClearManager.Instance.PlayLose());

        InitStagePosition();
        SceneManager.LoadScene(stageSelectSceneName);
    }

    public int GetMaxFloor(int stage)
    {
        int index = stage - 1;
        if (maxFloorPerStage != null && index >= 0 && index < maxFloorPerStage.Length)
            return maxFloorPerStage[index];
        return 1;
    }
    public int GetMaxStage()
    {
        return maxFloorPerStage.Length;
    }

    private bool CheckFloorClear()
    {
        foreach (var floor in isFloorCleared)
        {
            if (!floor) return false;
        }
        Debug.Log("CheckFloorClear");
        return true;
    }

    public bool IsStageUnlocked(int stage)
    {
        if (stage == 1) return true;
        return isFloorCleared != null && stage - 2 < isFloorCleared.Length && isFloorCleared[stage - 2];
    }

    public bool IsStageCleared(int stage)
    {
        return isFloorCleared != null && stage - 1 < isFloorCleared.Length && isFloorCleared[stage - 1];
    }

    public void InitStagePosition()
    {
        currentFloor = 1;
        currentStage = 1;
    }
}