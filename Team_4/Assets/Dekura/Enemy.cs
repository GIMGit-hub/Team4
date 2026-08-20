using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;

public class Enemy : MonoBehaviour
{
    private PlayerManager _playerManager;

    [Header("States")]
    [Header("HP")]
    [SerializeField] private GameObject enemyHP;
    [SerializeField] private int enemyMaxHP;
    [SerializeField] private int enemyNowHP;

    [SerializeField] private int attack;

    private Vector2 position;
    private float multiplier;
    private float enemyHPSize;
    //[SerializeField] private List<int> attackVariation;

    private void Start()
    {
        enemyHPSize = enemyHP.GetComponent<RectTransform>().sizeDelta.x;
        RefreshReferences();
    }
    public void InitState(Vector2 m_position, float m_multiplier)
    {
        position = m_position;
        multiplier = m_multiplier;
    }
    public void RefreshReferences()
    {
        _playerManager = FindAnyObjectByType<PlayerManager>();
    }
    public bool Attack()
    {
        _playerManager.TakeDamage(attack);
        return true;
    }

    /// <summary>
    /// 被弾時の処理
    /// </summary>
    /// <param name="damage">被ダメージ</param>
    public void TakeDamage(int damage)
    {
        Debug.Log("Trying_TakeDamage");
        enemyNowHP = Mathf.Max(enemyNowHP - damage, 0);

        float diff = (float)enemyNowHP / (float)enemyMaxHP;
        float sizediff = enemyHPSize * diff;
        enemyHP.GetComponent<RectTransform>().sizeDelta = new Vector2(sizediff, enemyHP.GetComponent<RectTransform>().sizeDelta.y);
    }
}
