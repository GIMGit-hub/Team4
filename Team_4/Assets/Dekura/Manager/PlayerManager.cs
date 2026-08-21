using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public enum PlayerState
{
    Idle,
    Attack,
    Support,
    Dead,
}

public class PlayerManager : MonoBehaviour
{
    public PlayerState playerState { get; private set; } = PlayerState.Idle;

    private EnemyManager _enemyManager;

    [Header("HP")]
    [SerializeField] private GameObject playerHP;
    [SerializeField] private TextMeshProUGUI playerHpText;
    [SerializeField] private int  playerMaxHP;
    [SerializeField] private int  playerNowHP;

    [Header("Diyalog")]
    [SerializeField] private GameObject diyalog;
    [SerializeField] private string defaultDiyalogText;

    [Header("Command")]
    [SerializeField] private Button attack;
    [SerializeField] private Button support;
    [SerializeField] private Button comfirm;
    [SerializeField] private GameObject attackCommond;
    [SerializeField] private GameObject supportCommond;
    [SerializeField] private List<Button> allKanji;

    private List<int> selectedKanji = new List<int>();

    private float playerStartHP;
    private PlayerState preState;

    private void Awake()
    {
        //-----クリック処理追加------//
        for (int i = 0; i < allKanji.Count; i++)
        {
            int index = i;
            allKanji[index].onClick.AddListener(() => KanjiOnClick(index));
        }
        attack.onClick.AddListener(() => attackOnclick());
        support.onClick.AddListener(() => supportOnclick());
        comfirm.onClick.AddListener(() => ComfarmOnClick());
        //---------------------------//

        //---オブジェクトの状態変更---//
        comfirm.gameObject.SetActive(false);
        //---------------------------//

        playerStartHP = playerHP.GetComponent<RectTransform>().sizeDelta.x;
        UpdateUi();
        RefreshReferences();
    }

    void Update()
    {
        if (preState != playerState)
        {
            //---Stateが切り替わる際の処理---//
            selectedKanji.Clear();
            UpdateUi();
            //-------------------------------//
        }
        preState = playerState;
    }

    /// <summary>
    /// シーン内の参照を再取得する
    /// </summary>
    public void RefreshReferences()
    {
        _enemyManager = FindAnyObjectByType<EnemyManager>();
    }

    /// <summary>
    /// Uiの更新
    /// </summary>
    void UpdateUi()
    {
        Debug.Log($"STATE;;{playerState}");
        switch (playerState)
        {
            case PlayerState.Idle:
                attackCommond.SetActive(false);
                supportCommond.SetActive(false);
                break;

            case PlayerState.Attack:
                attackCommond.SetActive(true);
                supportCommond.SetActive(false);
                break;

            case PlayerState.Support:
                attackCommond.SetActive(false);
                supportCommond.SetActive(true);
                break;

            case PlayerState.Dead:
                attackCommond.SetActive(false);
                supportCommond.SetActive(false); 
                break;

            default:
                break;
        }

        playerHpText.text = $"HP {playerNowHP}/{playerMaxHP}";

        for (int i = 0; i < allKanji.Count; i++)
        {
            bool isSelected = selectedKanji.Contains(i);
            allKanji[i].GetComponentInChildren<TextMeshProUGUI>().color = isSelected ? Color.yellow : Color.black;
        }

        if (selectedKanji.Count == 2)  comfirm.gameObject.SetActive(true) ;
        else                           comfirm.gameObject.SetActive(false); ;
    }

    /// <summary>
    /// 漢字選択時の処理
    /// </summary>
    private void KanjiOnClick(int index)
    {
        if (selectedKanji.Contains(index)) selectedKanji.Remove(index);
        else
        {
            if (selectedKanji.Count >= 2) selectedKanji.RemoveAt(0);
            selectedKanji.Add(index);

            //Debug.Log($"KANJI;;{selectedKanji.Contains(index)}");
            //Debug.Log($"index;;{index}");
        }

        UpdateUi();
    }

    /// <summary>
    /// "確定"押下時の処理
    /// </summary>
    private void ComfarmOnClick()
    {
        RefreshReferences();

        switch (playerState)
        {
            case PlayerState.Attack:
                //今は攻撃処理のみ
                //漢字に合わせた挙動は未実装
                _enemyManager.TakeDamageRequest(20);

                break;

            case PlayerState.Support:
                //今はHP回復処理のみ
                //漢字に合わせた挙動は未実装
                SupportEffect(20);
                break;

            default:
                break;
        }

        playerState = PlayerState.Idle;
        StageManager.Instance.turnState = TurnState.PlayerTurn_End;

        UpdateUi();
    }

    /// <summary>
    /// サポート選択時処理
    /// 今は回復のみ
    /// </summary>
    /// <param name="value"></param>
    private void SupportEffect(int value)
    {
        playerNowHP = Mathf.Min(playerNowHP + value, playerMaxHP);

        float diff = (float)playerNowHP / (float)playerMaxHP;
        float sizediff = playerStartHP * diff;
        playerHP.GetComponent<RectTransform>().sizeDelta = new Vector2(sizediff, playerHP.GetComponent<RectTransform>().sizeDelta.y);
    }

    /// <summary>
    /// 被弾時の処理
    /// </summary>
    /// <param name="damage">被ダメージ</param>
    public void TakeDamage(int damage)
    {
        playerNowHP = Mathf.Max(playerNowHP - damage, 0);

        float diff = (float)playerNowHP / (float)playerMaxHP;
        float sizediff = playerStartHP * diff;
        playerHP.GetComponent<RectTransform>().sizeDelta = new Vector2(sizediff, playerHP.GetComponent<RectTransform>().sizeDelta.y);

        if (playerNowHP <= 0)
        {
            playerState = PlayerState.Dead;
            Dead();
        }

        UpdateUi();
    }

    /// <summary>
    /// 死亡時の処理
    /// </summary>
    private void Dead()
    {

    }

    public void attackOnclick() => playerState = PlayerState.Attack;
    public void supportOnclick() => playerState = PlayerState.Support;
}
