using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUnit : MonoBehaviour
{
    private EnemyData data;
    private int hp;
    private int actionIndex = 0;
    private EnemyController controller;
    private EnemyManager_Example manager;
    private bool isDead = false;
    private int turnCount = 0;

    private float damageDealtBuff = 0f;
    private int buffRemainingTurns = 0;
    private float angerMultiplier = 1f;
    private float damageTakenBuff = 0f;
    private int damageTakenBuffTurns = 0;
    private float defenseMultiplier = 1f;
    private float hitTakenDamageBonus = 0f;
    private float fixedDamageBonus = 0f;
    private ActionAnnounceUI announceUI;

    public void Init(EnemyData data, EnemyController controller,EnemyManager_Example manager, ActionAnnounceUI announceUI)
    {
        this.data = data;
        hp = data.maxHp;
        this.controller = controller;
        this.manager = manager;
        this.announceUI = announceUI;

        controller.Init(TakeTurn);

        if (data.battleStartSelfDamageDealtBuffPercent > 0)
        {
            AddDamageDealtBuff(data.battleStartSelfDamageDealtBuffPercent / 100f, 999);
            Debug.Log($"{data.enemyName}は戦闘開始時に自分の与ダメが{data.battleStartSelfDamageDealtBuffPercent}%上昇！"); // 修正
        }

        if (data.battleStartSelfDamageTakenBuffPercent > 0)
        {
            AddDamageTakenBuff(data.battleStartSelfDamageTakenBuffPercent / 100f, 999);
            Debug.Log($"{data.enemyName}は戦闘開始時に自分の被ダメが{data.battleStartSelfDamageTakenBuffPercent}%減少！"); // 修正
        }
    }

    private void TakeTurn()
    {
        if (isDead || data.actions.Count == 0) return;

        turnCount++;

        if (data.turnStartTargetDamageDealtDebuffPercent > 0 &&
           (data.turnStartTargetDamageDealtDebuffMaxTurns == 0 || turnCount <= data.turnStartTargetDamageDealtDebuffMaxTurns))
        {
            //player.ApplyDamageDealtDebuff(data.turnStartTargetDamageDealtDebuffPercent, 1);
            Debug.Log($"{data.enemyName}はターン開始時に相手の与ダメを{data.turnStartTargetDamageDealtDebuffPercent}%減らした"); // 修正
        }

        if (data.turnStartHeal > 0)
        {
            Heal(data.turnStartHeal);
        }

        if (data.turnStartSelfDamageDealtBuffPercent > 0 && turnCount <= data.turnStartSelfBuffMaxTurns)
        {
            AddDamageDealtBuff(data.turnStartSelfDamageDealtBuffPercent / 100f, 1);
            Debug.Log($"{data.enemyName}はターン開始時に与ダメが{data.turnStartSelfDamageDealtBuffPercent}%上昇({turnCount}/{data.turnStartSelfBuffMaxTurns}ターン目)"); // 修正
        }

        EnemyActionData action = data.actions[actionIndex];
        actionIndex = (actionIndex + 1) % data.actions.Count;

        Debug.Log($"[ターン{turnCount}] {data.enemyName} の行動: {action.actionName}"); // 修正

        //ここ直す
        if (announceUI != null)
        {
            announceUI.Show($"{data.enemyName}の{action.actionName}"); // 修正
        }

        StartCoroutine(controller.ExecuteAction(action, this));

        TickDownBuff();
    }

    public float GetDamageDealtRate()
    {
        float rate = angerMultiplier;
        if (buffRemainingTurns > 0)
        {
            rate += damageDealtBuff;
        }

        if (data.damageDealtBonusPerAliveEnemyPercent > 0)
        {
            int aliveCount = controller.GetAliveEnemyCount();
            rate += aliveCount * data.damageDealtBonusPerAliveEnemyPercent / 100f;
        }

        rate += hitTakenDamageBonus;

        return rate;
    }

    public float GetFixedDamageBonus() => fixedDamageBonus;

    public void AddDamageDealtBuff(float rate, int duration)
    {
        damageDealtBuff += rate;
        buffRemainingTurns = duration + 1;
    }

    public void AddDamageTakenBuff(float rate, int duration)
    {
        damageTakenBuff += rate;
        damageTakenBuffTurns = duration + 1;
    }

    public void Heal(int amount)
    {
        hp += amount;
        if (hp > data.maxHp) hp = data.maxHp;
        Debug.Log($"{data.enemyName}の残りHP:{hp}(回復後)"); // 修正
    }

    public void OnAfterAttack()
    {
        if (data.damageDealtGainPerAttack > 0)
        {
            angerMultiplier *= 1f + data.damageDealtGainPerAttack / 100f;
            Debug.Log($"{data.enemyName}の怒り倍率: x{angerMultiplier:F3}"); // 修正
        }

        if (data.selfActionFixedGainPerHit > 0)
        {
            fixedDamageBonus += data.selfActionFixedGainPerHit;
            fixedDamageBonus = Mathf.Min(fixedDamageBonus, data.selfActionFixedGainCap);
            Debug.Log($"{data.enemyName}の固定ダメージ上乗せ: +{fixedDamageBonus}(上限{data.selfActionFixedGainCap})"); // 修正
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
        Debug.Log($"{data.enemyName}は{finalAmount}ダメージ！ 残りHP:{hp}"); // 修正

        if (data.damageTakenReductionPerAttack > 0)
        {
            defenseMultiplier *= 1f - data.damageTakenReductionPerAttack / 100f;
        }

        if (data.damageDealtGainPerHitTaken > 0)
        {
            hitTakenDamageBonus += data.damageDealtGainPerHitTaken / 100f;
            float capRate = data.damageDealtGainPerHitTakenCap / 100f;
            hitTakenDamageBonus = Mathf.Min(hitTakenDamageBonus, capRate);
            Debug.Log($"{data.enemyName}は攻撃を受け与ダメが上昇！ 現在の上乗せ:+{hitTakenDamageBonus * 100f:F1}%(上限{data.damageDealtGainPerHitTakenCap}%)"); // 修正
        }

        if (hp <= 0)
        {
            isDead = true;
            controller.EnemyDead(gameObject);
            Destroy(gameObject);
        }
    }

    public void TakeSelfDamage(int amount)
    {
        if (isDead) return;

        hp -= amount;
        Debug.Log($"{data.enemyName}は反動で{amount}ダメージ！ 残りHP:{hp}"); // 修正

        if (hp <= 0)
        {
            isDead = true;
            controller.EnemyDead(gameObject);
            Destroy(gameObject);
        }
    }
}