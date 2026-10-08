using NUnit.Framework.Internal;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    public float nowHp { get; private set; } = 0;
    public float nowDp { get; private set; } = 0;
    public int nowCost { get; private set; } = 0;
    public int useCardCount { get; private set; } = 0;
    public int hitCount { get; private set; } = 0;
    public int countResult { get; private set; } = 0;

    [Header("最大/開始時ステータス")]
    [SerializeField] public float maxHp = 100f;
    [SerializeField] private float startHp = 100f;
    [SerializeField] private float startDp = 20f;
    [SerializeField] public int maxCost = 5;
    [SerializeField] private int startCost = 3;

    [Header("使用デッキ")]
    [SerializeField] public Deck deck;

    [Header("攻撃間隔")]
    [SerializeField] private float atkDuriation = 0.2f;

    [Header("debug用ウィンドウ")]
    [SerializeField] private TextMeshProUGUI debugWindow;

    [System.Serializable]
    private class Buff
    {
        public CardEffect.EffectType type;
        public float value;
        public int enableTurn;
    }

    private List<Buff> turnbuffList = new();
    private List<Buff> countbuffList = new();
    // private List<ここにバフobj> debuffList = new ();

    //debug
    private float attackResult = 0;
    private float attackTotalResult = 0;
    private string atTarget = null;

    public event System.Action BuffAdded;
    public event System.Action HpMoved;

    private void OnEnable() => SetEventSubscribed(true);
    private void OnDisable() => SetEventSubscribed(false);

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
        nowHp = startHp;
        nowDp = startDp;
        nowCost = startCost;
    }

    private void Start()
    {
        SetEventSubscribed(true);
        InitData();
        UpdateUi();
    }

    private void InitData()
    {
        attackResult = 0;
        countResult = 0;
        attackTotalResult = 0;
        useCardCount = 0;
        hitCount = 0;
    }

    private void SetEventSubscribed(bool isEnable)
    {
        if (CardManager.Instance != null)
        {
            CardManager.Instance.OnCardUsed -= UpdateUi;
            if (isEnable) CardManager.Instance.OnCardUsed += UpdateUi;
        }
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnTurnChanged -= SwitchTurn;
            if (isEnable) TurnManager.Instance.OnTurnChanged += SwitchTurn;
        }
    }

    private void UpdateUi()
    {
        if (debugWindow == null) return;

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
            $"ATK_TOTAL:: {attackTotalResult}\n" +
            $"HIT_COUNT:: {hitCount}";
    }

    //------------------------------playerのaction----------------------------//

    public void Attack(CardEffect.EffectTarget target, float value, int count, EnemyUnit enemy = null, bool CountBuffAdaption = true)
    {

        if (target == CardEffect.EffectTarget.Enemy && enemy == null)
        {
            Debug.LogWarning("Enemy_null");
            return;
        }
        StartCoroutine(SpecialAction(target, value, count, enemy, CountBuffAdaption));
    }

    private IEnumerator SpecialAction(CardEffect.EffectTarget target, float value, int count, EnemyUnit enemy, bool CountBuffAdaption)
    {
        float damage = value * Mathf.Max(1f, UseBuff(CardEffect.EffectType.AttackBuff) / 100f);
        int hitcount = Mathf.Max(1, count);

        if (CountBuffAdaption)
            hitcount = Mathf.Max(1, count + (int)UseBuff(CardEffect.EffectType.CountBuff));

        Debug.Log($"Player Attack! Target: {target}, Damage: {damage}, Count: {hitcount}");

        for (int i = 0; i < hitcount; i++)
        {
            switch (target)
            {
                case CardEffect.EffectTarget.Enemy:
                    atTarget = "Enemy";
                    if(!EnemyController.Instance.PlayerAttack(damage, enemy)) yield break;
                    break;
                case CardEffect.EffectTarget.AllEnemy:
                    atTarget = "AllEnemy";
                    EnemyController.Instance.PlayerAttackAll(damage);
                    break;
                default:
                    break;
            }

            attackResult = damage;
            countResult = hitcount;
            hitCount++;
            attackTotalResult += damage;

            SoundsManager.Instance.PlaySound("hit");
            yield return new WaitForSeconds(atkDuriation);
        }
    }

    public void DpHeal(float value)
    {
        nowDp += value;
        EffectManager.Instance.Playfade("heal");
        SoundsManager.Instance.PlaySound("heal");
    }
    public void HpHeal(float value)
    {
        nowHp = Mathf.Min(nowHp + maxHp * (value / 100f), maxHp);
        EffectManager.Instance.Playfade("heal");
        SoundsManager.Instance.PlaySound("heal");
    }
    public void HpHeal_damage(float value)
    {
        float diff = nowHp + (attackResult * value / 100f);
        nowHp = Mathf.Min(diff, maxHp);
        EffectManager.Instance.Playfade("heal");
        SoundsManager.Instance.PlaySound("heal");
    }
    public void HpHeal_count(float value, int count)
    {
        float diff = nowHp + (maxHp * (value / 100f) * count);
        nowHp = Mathf.Min(diff, maxHp);
        EffectManager.Instance.Playfade("heal");
        SoundsManager.Instance.PlaySound("heal");
    }

    public void CostHeal(int value)
    {
        nowCost = Mathf.Min(nowCost + value, maxCost);
    }

    public bool CanUseCost(CardData.CostType type, int cost)
    {
        bool isCanUse = false;
        if (GetEffect(CardEffect.EffectType.CostFree) > 0f) return true;

        switch (type)
        {
            case CardData.CostType.Normal:
                isCanUse = nowCost >= cost - (int)GetEffect(CardEffect.EffectType.CostBuff);
                break;
            case CardData.CostType.Hp:
                isCanUse = nowHp > 1;
                break;
            case CardData.CostType.AllCost:
                isCanUse = true;
                break;
            case CardData.CostType.Ace:
                isCanUse = nowCost >= cost - (int)GetEffect(CardEffect.EffectType.CostBuff_Ace) ||
                           GetEffect(CardEffect.EffectType.CostBuff_Ace) != 0;
                break;
        }

        return isCanUse;
    }

    public void UseCost(CardData.CostType type, int cost)
    {
        if (GetEffect(CardEffect.EffectType.CostFree) == 0f)
        {
            switch (type)
            {
                case CardData.CostType.Normal:
                    nowCost -= cost - (int)UseBuff(CardEffect.EffectType.CostBuff);
                    break;

                case CardData.CostType.Hp:
                    nowHp -= cost;
                    if (nowHp <= 0) nowHp = 1;
                    EffectManager.Instance.Playfade("damage");
                    SoundsManager.Instance.PlaySound("damage");
                    break;

                case CardData.CostType.AllCost:
                    nowCost = 0;
                    break;

                case CardData.CostType.Ace:
                    if (GetEffect(CardEffect.EffectType.CostBuff_Ace) != 0)
                    {
                        UseBuff(CardEffect.EffectType.CostBuff_Ace);
                        foreach (var buff in turnbuffList.ToList())
                        {
                            if (buff.type == CardEffect.EffectType.CostBuff_Ace) turnbuffList.Remove(buff);
                            break;
                        }
                    }
                    else
                    {
                        nowCost -= cost;
                    }
                    break;

                default: break;
            }
        }
        
        BuffAdded?.Invoke();
    }

    public void AddUseCardCount() => useCardCount++;
    //-------------------------------------------------------------------------//

    //被弾処理
    public void TakeDamage(float amount)
    {
        nowHp = Mathf.Max(0f, nowHp - amount);
        EffectManager.Instance.Playfade("damage");
        SoundsManager.Instance.PlaySound("damage");

        if (nowHp == 0)
        {
            //敗北処理
        }

        HpMoved?.Invoke();
    }

    //バフの新規獲得
    public void AddEffect_Turn(CardEffect.EffectType m_type, float m_value, int m_enableTurn)
    {
        turnbuffList.Add(new Buff
        {
            type = m_type,
            value = m_value,
            enableTurn = m_enableTurn,
        });

        BuffAdded?.Invoke();
    }
    public void AddEffect_Count(CardEffect.EffectType m_type, float m_value, int m_enableCount)
    {
        countbuffList.Add(new Buff
        {
            type = m_type,
            value = m_value,
            enableTurn = m_enableCount,
        });
        BuffAdded?.Invoke();
    }
    //バフ総量の確認
    public float GetEffect(CardEffect.EffectType m_type)
    {
        float resultValue = 0;

        foreach(var buff in turnbuffList)
        {
            if (buff.type != m_type) continue;
            resultValue += buff.value;
        }
        foreach (var buff in countbuffList)
        {
            if (buff.type != m_type) continue;
            resultValue += buff.value;
        }

        return resultValue;
    }

    //バフの使用、カウント系は減らす
    public float UseBuff(CardEffect.EffectType m_type)
    {
        float resultValue = GetEffect(m_type);

        foreach (var buff in countbuffList.ToList())
        {
            if (buff.type != m_type) continue;
            buff.enableTurn--;
            if (buff.enableTurn <= 0) countbuffList.Remove(buff);
        }

        return resultValue;
    }
    //バフのターン減少
    public void DecreaseBuffTurn()
    {
        foreach (var buff in turnbuffList.ToList())
        {
            buff.enableTurn--;
            if (buff.enableTurn <= 0) turnbuffList.Remove(buff);
        }
    }

    //ターン変更時の処理
    private void SwitchTurn(TurnManager.TurnState turnState)
    {
        switch (turnState)
        {
            case TurnManager.TurnState.PlayerTurn:
                nowCost = Mathf.Max(nowCost, startCost);
                InitData();
                break;

            case TurnManager.TurnState.EnemyTurn:
                DecreaseBuffTurn();
                break;

            default:
                break;
        }

        UpdateUi();
        UiManager.Instance.UpdateUi();
    }
}
