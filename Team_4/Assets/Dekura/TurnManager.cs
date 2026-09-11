using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public enum TurnState
    {
        PlayerTurn,
        EnemyTurn,
        Changing
    }

    public static TurnManager Instance { get; private set; }
    public TurnState NowTurn { get; private set; } = TurnState.PlayerTurn;
    public event Action<TurnState> OnTurnChanged;          //ターン切り替わり時に通知

    [SerializeField]private Button pturnEndButton;         //Pターン切り替えボタン
    [SerializeField]private float turnChangeDiray = 0.5f;    //ターン切り替わりディレイ

    private bool isTurnChanging = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;

        pturnEndButton.onClick.AddListener(TurnChange);
    }

    private void Update()
    {
        if (NowTurn == TurnState.EnemyTurn) TurnChange();
    }

    public void TurnChange()
    {
        if (isTurnChanging) return;
        StartCoroutine(TurnChanfeRoutine());
    }

    private IEnumerator TurnChanfeRoutine()
    {
        isTurnChanging = true;

        TurnState nextTurn = (NowTurn == TurnState.PlayerTurn) ? TurnState.EnemyTurn : TurnState.PlayerTurn;
        turnAnnounse(nextTurn);
        NowTurn = TurnState.Changing;
        Debug.Log("TurnChanging...");

        yield return new WaitForSeconds(turnChangeDiray);

        Debug.Log("Complete");
        NowTurn = nextTurn;
        OnTurnChanged?.Invoke(NowTurn);
        isTurnChanging = false;
    }

    private void turnAnnounse(TurnState turn)
    {
        switch(turn)
        {
            case TurnState.PlayerTurn:
                Debug.Log("PlayerTurn");
                EffectManager.Instance.Playfade("heal", turnChangeDiray);
                break;

            case TurnState.EnemyTurn:
                Debug.Log("EnemyTurn");
                EffectManager.Instance.Playfade("damage", turnChangeDiray);
                break;
        }
    }
}
