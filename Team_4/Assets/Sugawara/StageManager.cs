using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("進行状況")]
    [SerializeField] private int currentStage = 1;
    [SerializeField] private int currentFloor = 1;

    [Header("ステージごとの最大フロア数(Index0=ステージ1)")]
    [SerializeField] private int[] maxFloorPerStage = { 3, 3, 3 };

    [Header("最大ステージ数")]
    [SerializeField] private int maxStage = 3;

    [Header("シーン名")]
    [SerializeField] private string mainSceneName = "MainGame";
    [SerializeField] private string stageSelectSceneName = "StageSelectScene";

    [Header("選択中のデッキ")]
    [SerializeField] private Deck selectedDeck;

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
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    // メインシーンに入ったら、自分の持つ階層をEnemyManagerに渡す
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != mainSceneName) return;

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
        currentStage = Mathf.Clamp(stage, 1, maxStage);
        currentFloor = 1;
        SceneManager.LoadScene("MainGame");
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

        EnemyManager_Example manager = FindFirstObjectByType<EnemyManager_Example>();
        manager?.StartCurrentFloor(currentStage, currentFloor);
    }
    //クリアしたときの処理
    private void OnStageClear()
    {
        Debug.Log($"=== ステージ{currentStage} クリア！ ===");

        if (currentStage >= maxStage)
        {
            Debug.Log("=== 全ステージクリア！ ===");
            // TODO: エンディング演出等をここに
            return;
        }

       
        currentStage++;
        currentFloor = 1;

        Debug.Log($"=== ステージ{currentStage} 開始！ ===");

        SceneManager.LoadScene("MainGame");
    }

    private int GetMaxFloor(int stage)
    {
        int index = stage - 1;
        if (maxFloorPerStage != null && index >= 0 && index < maxFloorPerStage.Length)
            return maxFloorPerStage[index];
        return 1;
    }
}