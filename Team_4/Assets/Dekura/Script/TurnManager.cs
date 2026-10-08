using DG.Tweening;
using System;
using System.Collections;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public enum TurnState
    {
        PlayerTurn,
        EnemyTurn,
        Changing,
    }

    public static TurnManager Instance { get; private set; }
    public TurnState NowTurn { get; private set; } = TurnState.PlayerTurn;
    public event Action<TurnState> OnTurnChanged;          //ターン切り替わり時に通知

    [SerializeField]private Button pturnEndButton;         //Pターン切り替えボタン
    [SerializeField]private float turnChangeDiray = 0.5f;    //ターン切り替わりディレイ

    [Header("TurnChangeWindow")]
    [SerializeField] GameObject window_Player;
    [SerializeField] GameObject window_Enemy;
    [SerializeField] Transform parent;
    [SerializeField] private Vector2 sponePosition;
    [SerializeField] private Vector2 slowPosition;
    [SerializeField] private Vector2 endPosition = new Vector2(0f, -700f);
    [SerializeField] private float duriation = 0.1f;


    private bool isTurnChanging = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;

        pturnEndButton.onClick.AddListener(() => TurnChange());
        StartCoroutine(AnnnounseWindow(TurnState.PlayerTurn));
    }
    private void Start()
    {
    }

    public void FloorClear()
    {
        CardManager.Instance.HandReset();
        NowTurn = TurnState.Changing;
    }

    public void TurnChange(TurnState nextTurn = default)
    {
        if (isTurnChanging) return;
        StartCoroutine(TurnChangeRoutine(nextTurn));
    }

    private IEnumerator TurnChangeRoutine(TurnState nextTurn)
    {
        isTurnChanging = true;

        if (nextTurn == default)
            nextTurn = (NowTurn == TurnState.PlayerTurn) ? TurnState.EnemyTurn : TurnState.PlayerTurn;
        Coroutine announse = StartCoroutine(AnnnounseWindow(nextTurn));

        NowTurn = TurnState.Changing;
        Debug.Log("TurnChanging...");

        if (nextTurn == TurnState.EnemyTurn) CardManager.Instance.HandReset();
        if (nextTurn == TurnState.PlayerTurn) StartCoroutine(CardManager.Instance.Call(6));
        yield return announse;

        Debug.Log("Complete");
        NowTurn = nextTurn;
        OnTurnChanged?.Invoke(NowTurn);
        isTurnChanging = false;
    }

    private IEnumerator AnnnounseWindow(TurnState turn)
    {
        GameObject go = null;
        switch(turn)
        {
            case TurnState.PlayerTurn:
                Debug.Log("PlayerTurn");
                go = Instantiate(window_Player, parent);
                //EffectManager.Instance.Playfade("heal", turnChangeDiray);
                break;

            case TurnState.EnemyTurn:
                Debug.Log("EnemyTurn");
                go = Instantiate(window_Enemy, parent);
                //EffectManager.Instance.Playfade("damage", turnChangeDiray);
                break;

            default: break;
        }
        if (go == null) yield break;

        RectTransform rect = go.GetComponent<RectTransform>();
        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        //rect.anchoredPosition = sponePosition;

        //Append>>終了を待って次を開始
        //Joim>>ひとつ前のAppendと同時に処理、Appendはこれも待つ
        var seq =
            DOTween.Sequence()
            .AppendCallback(() => rect.anchoredPosition = sponePosition)
            .Append(rect.DOAnchorPos(slowPosition, duriation).SetEase(Ease.OutExpo))
            .Append(rect.DOAnchorPos(endPosition, duriation / 2f).SetEase(Ease.InExpo))
            .Join(cg.DOFade(0f, duriation / 2f).SetEase(Ease.InOutExpo));

        yield return seq.WaitForCompletion();

        Destroy(go);
    }
}
