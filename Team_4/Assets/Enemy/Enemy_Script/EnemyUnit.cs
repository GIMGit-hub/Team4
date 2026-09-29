using System;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUnit : MonoBehaviour
{
    [SerializeField] private Button selectButton;

    private int hp;
    private int damage;
    private Action<int> dealDamageToTarget;
    private EnemyController controller;
    private EnemyManager_Example manager;
    private bool isDead = false;

    public void Init(EnemyData data, EnemyController controller, Action<int> dealDamageFunc, EnemyManager_Example manager)
    {
        hp = data.maxHp;
        damage = data.damage;
        dealDamageToTarget = dealDamageFunc;
        this.controller = controller;
        this.manager = manager;

        controller.Init(Attack);

        if (selectButton != null)
        {
            selectButton.onClick.AddListener(() => manager.OnSelectTarget(gameObject));
        }
    }

    private void Attack()
    {
        if (isDead) return;

        dealDamageToTarget?.Invoke(damage);
        Debug.Log($"{name}の攻撃！ {damage}ダメージ");
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