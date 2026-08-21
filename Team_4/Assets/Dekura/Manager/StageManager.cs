using System.Collections;
using TMPro;
using UnityEngine;

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

        debugText.GetComponent<TextMeshProUGUI>().text = turnState.ToString();
        RefreshReferences();
        SpawnRequest();
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
            debugText.GetComponent<TextMeshProUGUI>().text = turnState.ToString();
            StartCoroutine(TurnSwitch());
            //-------------------------------//
        }
        preGameState = gameState;
        preTurnState = turnState;
    }
     
    private IEnumerator TurnSwitch()
    {
        Debug.Log($"Trying_TurnSwitch");
        TurnState nextTurn = turnState;

        switch (turnState)
        {
            case TurnState.PlayerTurn_End:
                nextTurn = TurnState.EnemyTurn;
                break;

            case TurnState.EnemyTurn_End:
                nextTurn = TurnState.PlayerTurn;
                break;
            default:
                yield break;
        }

        yield return new WaitForSeconds(1.0f);
        turnState = nextTurn;

        if (turnState == TurnState.EnemyTurn) _enemyManager.AttackRequest();
    }

    private void SpawnRequest()
    {
        RefreshReferences();
        _enemyManager.SpawnEnemys();
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
