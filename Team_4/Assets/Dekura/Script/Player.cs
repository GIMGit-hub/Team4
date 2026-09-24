using NUnit.Framework.Internal;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    private EffectManager effect;

    private float nowHp = 0;
    private float nowDp = 0;
    private int nowCost = 0;

    [Header("最大/開始時ステータス")]
    [SerializeField] private float maxHp = 100f;
    [SerializeField] private float startHp = 100f;
    [SerializeField] private int maxCost = 5;
    [SerializeField] private int startCost = 3;

    [Header("使用デッキ")]
    [SerializeField] public Deck deck;

    [Header("コンポーネント")]
    [SerializeField] private Image hp_Image;
    [SerializeField] private Image cost_Image;

    [Header("debug用ウィンドウ")]
    [SerializeField] private TextMeshProUGUI debugWindow;

    [System.Serializable]
    private class Buff
    {
        public CardEffect.EffectType type;
        public float value;
        public int enableTurn;
    }

    private List<Buff> buffList = new();
    // private List<ここにバフobj> debuffList = new ();

    //debug
    private float attackResult = 0;
    private int countResult = 1;
    private string atTarget = null;

    private void OnEnable()  { if (CardManager.Instance != null) CardManager.Instance.OnCardUsed += UpdateUi; }
    private void OnDisable() { if (CardManager.Instance != null) CardManager.Instance.OnCardUsed -= UpdateUi; }

    private void Awake()
    {
        //------インスタンス化------//
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        //---------------------------//

        effect = FindAnyObjectByType<EffectManager>();
        nowHp = startHp;
        nowCost = startCost;

        UpdateUi();
    }

    private void Start()
    {
        CardManager.Instance.OnCardUsed -= UpdateUi;
        CardManager.Instance.OnCardUsed += UpdateUi;
    }

    private void UpdateUi()
    {
        debugWindow.text =
            $"HP  :: {nowHp} / {maxHp}\n" +
            $"DP  :: {nowDp}\n" +
            $"COST:: {nowCost} / {maxCost}\n" +
            $"\n" +
            $"BUFF_ATTACK:: +{GetEffect(CardEffect.EffectType.AttackBuff)}%\n" +
            $"BUFF_COUNT :: +{GetEffect(CardEffect.EffectType.CountBuff)}\n" +
            $"BUFF_COST  :: +{GetEffect(CardEffect.EffectType.CostBuff)}\n" +
            $"\n" +
            $"ATTACKED:: To {atTarget} , {attackResult} × {countResult}\n" +
            $"";
    }

    //------------------------------playerのaction----------------------------//

    public void Attack(CardEffect.EffectTarget target, float value, int count)
    {
        switch (target)
        {
            case CardEffect.EffectTarget.Enemy:
                atTarget = "Enemy";
                break;
            case CardEffect.EffectTarget.AllEnemy:
                atTarget = "AllEnemy";
                break;
            default:
                break;
        }
        attackResult = value + GetEffect(CardEffect.EffectType.AttackBuff);
        countResult = count;
    }

    

    public void DpHeal(float value)
    {
        nowDp += value;
        effect.Playfade("heal");
    }
    public void HpHeal(float value)
    {
        nowHp = Mathf.Min(nowHp + value, maxHp);
        effect.Playfade("heal");
    }
    public void CostHeal(int value)
    {
        nowCost = Mathf.Min(nowCost + value, maxCost);
    }

    public bool CanUseCost(int cost)
    {
        int useCost = cost - (int)GetEffect(CardEffect.EffectType.CostBuff);

        Debug.Log($"CanUseCost...{useCost}");
        return nowCost >= useCost;
    }
    public void UseCost(int cost)
    {
        nowCost -= cost - (int)GetEffect(CardEffect.EffectType.CostBuff);
    }

    //--------------------------------------------n----------------------------//



    //被弾処理

    //バフの新規獲得
    public void AddEffect(CardEffect.EffectType m_type, float m_value, int m_enableTurn)
    {
        buffList.Add(new Buff
        {
            type = m_type,
            value = m_value,
            enableTurn = m_enableTurn,
        });
    }
    public float GetEffect(CardEffect.EffectType m_type)
    {
        float resultValue = 0;

        foreach(var buff in buffList)
        {
            if (buff.type != m_type) continue;

            resultValue += buff.value;
        }

        return resultValue;
    }
    //所持バフのターン減少(turnMGからevent発火)
}
