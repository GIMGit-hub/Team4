using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GameState
{
    Title,
    MainGame,
    MainGame_Result,
    MainGame_End,
}
public enum TurnState
{
    PlayerTurn,
    PlayerTurn_End,
    EnemyTurn,
    EnemyTurn_End,
    SpawningTurn,
}

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }
    public GameState gameState { get; private set; } = GameState.MainGame;
    private GameState preGameState;
    public TurnState turnState { get; set; } = TurnState.PlayerTurn;
    private TurnState preTurnState;

    private PlayerManager _playerManager;
    private EnemyManager _enemyManager;

    [Header("Stage")]
    public int nowStage;
    public int nowFloor;
    public int[] stageMaxFloor;

    [Header("Debug")]
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private GameObject clearText;

    private bool floorClear = false;
    private bool stageClear = false;
    private bool game_end = false;

    private int turnPoint = 0;

    private void Awake()
    {
        //------インスタンス化------//
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        //---------------------------//

        preGameState = gameState;
        preTurnState = turnState;

        clearText.SetActive(false);
        debugText.text = turnState.ToString();
        RefreshReferences();
        SpawnRequest();

        _enemyManager.OnAllEnemiesDead += FloorClear;
    }

    private void Update()
    {
        if (preGameState != gameState)
        {
            //---GameStateが切り替わる際の処理---//
            //-------------------------------//
        }
        if (preTurnState != turnState)
        {
            //---turnStateが切り替わる際の処理---//
            RefreshReferences();

            debugText.GetComponent<TextMeshProUGUI>().text = turnState.ToString();
            StartCoroutine(TurnSwitch());
            _playerManager.UpdateUi();
            //-------------------------------//
        }
        preGameState = gameState;
        preTurnState = turnState;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (game_end)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    private void FloorClear()
    {
        UiManager_MainGame.Instance.SetDiyalog("フロアクリア！");
        floorClear = true;
        StartCoroutine(GotoNextFloor());
    }

    private IEnumerator GotoNextFloor()
    {
        yield return new WaitForSeconds(1.5f);
        if (stageMaxFloor[nowStage - 1] == nowFloor)
        {
            UiManager_MainGame.Instance.SetDiyalog($"ステージクリア！\nターンリザルト:{turnPoint}");
            clearText.SetActive(true);
            game_end = true;
            yield break;
        }
        stageClear = false;

        nowFloor++;
        SpawnRequest();
        turnState = TurnState.PlayerTurn;
        UiManager_MainGame.Instance.UpdateUI();
    }


    private IEnumerator TurnSwitch()
    {
        Debug.Log($"Trying_TurnSwitch");
        TurnState nextTurn = turnState;
        float waitTime;

        switch (turnState)
        {
            case TurnState.PlayerTurn_End:
                turnPoint++;

                nextTurn = TurnState.EnemyTurn;
                waitTime = 1.0f;

                break;

            case TurnState.EnemyTurn_End:
                nextTurn = TurnState.PlayerTurn;
                waitTime = 0.5f;

                if (_playerManager.playerState == PlayerState.Dead)
                {
                    game_end = true;
                    clearText.SetActive(true);
                }
                else UiManager_MainGame.Instance.SetDiyalog("あなたのターン");

                break;
            default:
                yield break;
        }

        yield return new WaitForSeconds(waitTime);
        turnState = nextTurn;
        if (turnState == TurnState.EnemyTurn) StartCoroutine(_enemyManager.AttackRequest());
    }

    private void SpawnRequest()
    {
        RefreshReferences();
        _enemyManager.SpawnEnemys(nowStage, nowFloor);
    }

    /// <summary>
    /// シーン内の参照を再取得する
    /// </summary>
    public void RefreshReferences()
    {
        _playerManager = FindAnyObjectByType<PlayerManager>();
        _enemyManager = FindAnyObjectByType<EnemyManager>();
    }
}
