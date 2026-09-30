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
    private EnemyController controller;
    private EnemyManager_Example manager;
    private bool isDead = false;

    private float damageDealtBuff = 0f; // 一時的な倍率上乗せ(力を溜めるなど)
    private int buffRemainingTurns = 0;
    private float angerMultiplier = 1f; // 恒久的な倍率(攻撃するたび上昇)

    public void Init(EnemyData data, EnemyController controller, Action<int> dealDamageFunc, EnemyManager_Example manager)
    {
        this.data = data;
        hp = data.maxHp;
        dealDamageToTarget = dealDamageFunc;
        this.controller = controller;
        this.manager = manager;

        controller.Init(TakeTurn); // 行動そのものではなく「ターンを進める」関数を渡す

        if (selectButton != null)
        {
            selectButton.onClick.AddListener(() => manager.OnSelectTarget(gameObject));
        }
    }

    // 自分のターン: 今の番の行動をControllerに実行してもらう
    private void TakeTurn()
    {
        if (isDead || data.actions.Count == 0) return;

        EnemyActionData action = data.actions[actionIndex];
        actionIndex = (actionIndex + 1) % data.actions.Count;

        controller.ExecuteAction(action, this, dealDamageToTarget);

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
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        hp -= amount;
        Debug.Log($"{name}は{amount}ダメージ！ 残りHP:{hp}");

        if (hp <= 0)
        {
            isDead = true;
            controller.EnemyDead(gameObject);
            Destroy(gameObject);
        }
    }
}
