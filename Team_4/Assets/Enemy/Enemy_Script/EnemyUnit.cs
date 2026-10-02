using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUnit : MonoBehaviour
{
    [SerializeField] private Button selectButton;

    private EnemyData data;
    private int hp;
    private int actionIndex = 0;
    private Action<int> dealDamageToTarget;
    private Player_Example player;
    private EnemyController controller;
    private EnemyManager_Example manager;
    private bool isDead = false;

    private float damageDealtBuff = 0f; // 一時的な倍率上乗せ(力を溜めるなど)
    private int buffRemainingTurns = 0;
    private float angerMultiplier = 1f; // 恒久的な倍率(攻撃するたび上昇)
    private float damageTakenBuff = 0f;
    private int damageTakenBuffTurns = 0;
    private float defenseMultiplier = 1f; //攻撃するたび被ダメが減る(Ver3用)の恒久倍率


    public void Init(EnemyData data, EnemyController controller, Action<int> dealDamageFunc,Player_Example player, EnemyManager_Example manager)
    {
        this.data = data;
        hp = data.maxHp;
        dealDamageToTarget = dealDamageFunc;
        this.player = player;
        this.controller = controller;
        this.manager = manager;

        controller.Init(TakeTurn); // 行動そのものではなく「ターンを進める」関数を渡す

        if (selectButton != null)
        {
            selectButton.onClick.AddListener(() => manager.OnSelectTarget(gameObject));
        }

        //戦闘開始時に一度だけ発動する効果
        if (data.battleStartSelfDamageDealtBuffPercent > 0)
        {
            AddDamageDealtBuff(data.battleStartSelfDamageDealtBuffPercent / 100f, 999); // 999=実質ずっと効く
            Debug.Log($"{name}は戦闘開始時に自分の与ダメが{data.battleStartSelfDamageDealtBuffPercent}%上昇！");
        }

        //Ver6用
        if (data.battleStartSelfDamageTakenBuffPercent > 0)
        {
            AddDamageTakenBuff(data.battleStartSelfDamageTakenBuffPercent / 100f, 999);
            Debug.Log($"{name}は戦闘開始時に自分の被ダメが{data.battleStartSelfDamageTakenBuffPercent}%減少！");
        }
    }

    // 自分のターン: 今の番の行動をControllerに実行してもらう
    private void TakeTurn()
    {
        if (isDead || data.actions.Count == 0) return;

        EnemyActionData action = data.actions[actionIndex];
        actionIndex = (actionIndex + 1) % data.actions.Count;

        StartCoroutine(controller.ExecuteAction(action, this, dealDamageToTarget,player));

        TickDownBuff();
    }

    // ダメージ計算に使う、現在の倍率
    public float GetDamageDealtRate()
    {
        float rate = angerMultiplier;
        if (buffRemainingTurns > 0) rate += damageDealtBuff;
        return rate;
    }

    // 「力を溜める」などが呼ぶ
    public void AddDamageDealtBuff(float rate, int duration)
    {
        damageDealtBuff += rate;
        buffRemainingTurns = duration;
    }

    //「硬くなる」などが呼ぶ
    public void AddDamageTakenBuff(float rate, int duration)
    {
        damageTakenBuff += rate;
        damageTakenBuffTurns = duration;
    }

    //回復
    public void Heal(int amount)
    {
        hp += amount;
        if (hp > data.maxHp) hp = data.maxHp;
        Debug.Log($"{name}の残りHP:{hp}(回復後)");
    }

    // 攻撃した直後、固有の特性(3%上昇など)を適用
    public void OnAfterAttack()
    {
        if (data.damageDealtGainPerAttack > 0)
        {
            angerMultiplier *= 1f + data.damageDealtGainPerAttack / 100f;
            Debug.Log($"{name}の怒り倍率: x{angerMultiplier:F3}");
        }
    }

    private void TickDownBuff()
    {
        if (buffRemainingTurns > 0)
        {
            buffRemainingTurns--;
            if (buffRemainingTurns <= 0) damageDealtBuff = 0f;
        }

        if (damageTakenBuffTurns > 0)
        {
            damageTakenBuffTurns--;
            if (damageTakenBuffTurns <= 0) damageTakenBuff = 0f;
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        float reduceRate = defenseMultiplier;
        if (damageTakenBuffTurns > 0) reduceRate -= damageTakenBuff;

        int finalAmount = Mathf.Max(0, Mathf.RoundToInt(amount * reduceRate));
        hp -= finalAmount;
        Debug.Log($"{name}は{finalAmount}ダメージ！ 残りHP:{hp}");

        //攻撃を受けるたび、Ver3の恒久軽減を適用
        if (data.damageTakenReductionPerAttack > 0)
        {
            defenseMultiplier *= 1f - data.damageTakenReductionPerAttack / 100f;
            //Ver3デバッグ用
            //Debug.Log($"{name}は被ダメージが{data.damageTakenReductionPerAttack}%減少！ 軽減倍率:x{defenseMultiplier:F4}(四捨五入前の実際の値)");
        }

        if (hp <= 0)
        {
            isDead = true;
            controller.EnemyDead(gameObject);
            Destroy(gameObject);
        }
    }
}
