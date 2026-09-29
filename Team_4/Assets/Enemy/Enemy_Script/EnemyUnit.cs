using System;
using UnityEngine;

public class EnemyUnit : MonoBehaviour
{
    private int hp;
    private int damage;
    private Action<int> dealDamageToTarget;
    
    private bool Isdead = false;

    // Controllerから呼ばれる。自分の行動関数をControllerに渡す
    public void Init(EnemyData data, EnemyController controller, Action<int> dealDamageFunc)
    {
        hp = data.maxHp;
        damage = data.damage;
        dealDamageToTarget = dealDamageFunc;
        
        controller.Init(Attack); // Attackという関数そのものをcontrollerに渡す
    }

    // この敵の行動
    private void Attack()
    {
        dealDamageToTarget?.Invoke(damage);
        Debug.Log($"{name}の攻撃！ {damage}ダメージ");
    }

    // 自分がダメージを受けたときの処理
    public void TakeDamage(int amount)
    {
        hp -= amount;
        if (hp <= 0)
        {
            Isdead = true;
            EnemyController.Instance.EnemyDead(gameObject);
        }
    }

}