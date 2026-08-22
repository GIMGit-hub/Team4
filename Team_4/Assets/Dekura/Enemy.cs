using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    private PlayerManager _playerManager;
    private EnemyManager _enemyManager;
    public bool IsDead { get; private set; }

    [Header("Visual")]
    [SerializeField] private GameObject enemySprite;
    [SerializeField] private GameObject enemyHPSprite;
    [SerializeField] private Image highlite;

    [Header("States")]
    [SerializeField] private float enemyMaxHP;
    [SerializeField] private float enemyNowHP;

    [SerializeField] private float attack;

    private float multiplier;
    private float enemyHPSize;
    //[SerializeField] private List<int> attackVariation;

    public event Action OnDeath;

    private void Start()
    {
        enemyHPSize = enemyHPSprite.GetComponent<RectTransform>().sizeDelta.x;
        RefreshReferences();

        gameObject.GetComponent<Button>().onClick.AddListener(() => OnClick());
    }
    public void InitState(float m_multiplier)
    {
        multiplier = m_multiplier;
        enemyMaxHP *= multiplier / 100.0f;
        enemyNowHP *= multiplier / 100.0f;
        attack *= multiplier / 100.0f;
    }
    public void RefreshReferences()
    {
        _playerManager = FindAnyObjectByType<PlayerManager>();
        _enemyManager = FindAnyObjectByType<EnemyManager>();
    }

    /// <summary>
    /// 攻撃時の処理
    /// </summary>
    /// <returns></returns>
    public bool Attack()
    {
        _playerManager.TakeDamage(attack);
        return true;
    }

    /// <summary>
    /// 被弾時の処理
    /// </summary>
    /// <param name="damage">被ダメージ</param>
    public void TakeDamage(float damage)
    {
        Debug.Log("Trying_TakeDamage");
        enemyNowHP = Mathf.Max(enemyNowHP - damage, 0);

        float diff = (float)enemyNowHP / (float)enemyMaxHP;
        float sizediff = enemyHPSize * diff;
        enemyHPSprite.GetComponent<RectTransform>().sizeDelta = new Vector2(sizediff, enemyHPSprite.GetComponent<RectTransform>().sizeDelta.y);

        if (enemyNowHP <= 0)
        {
            Dead();
        }
    }

    /// <summary>
    /// 死亡時の処理
    /// </summary>
    private void Dead()
    {
        Debug.LogWarning($"Dead::{gameObject.name}");

        enemySprite.GetComponent<Image>().color = Color.gray;
        enemyHPSprite.SetActive(false);
        IsDead = true;

        OnDeath?.Invoke();
    }


    private void OnClick()
    {
        RefreshReferences();
        _enemyManager.SetSelectedEnemy(this);
    }

    public void Selected(bool selected)
    {
        highlite.enabled = selected;
    }
}
