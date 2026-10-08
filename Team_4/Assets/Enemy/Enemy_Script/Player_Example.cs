using UnityEngine;

public class Player_Example : MonoBehaviour
{
    [SerializeField] private int maxHp = 100; //hpをmaxHpに統一
    private int hp = 100;
    [SerializeField] private int damage = 1000;

    private float damageDealtMultiplier = 1f;  
    private int damageDealtDebuffTurns = 0;

    private float damageTakenMultiplier = 1f;
    private int damageTakenDebuffTurns = 0;

    public void Attack(EnemyController controller, GameObject target)
    {
        int finalDamage = Mathf.RoundToInt(damage * damageDealtMultiplier); // 倍率を掛ける
        Debug.Log($"    → {target.name}に{finalDamage}ダメージ");
       // controller.PlayerAttack(finalDamage, target);
    }

    public void TakeDamage(int amount)
    {
        hp -= amount;
        Debug.Log($"プレイヤーは{amount}ダメージ！ 残りHP:{hp}");
    }
    //敵の「威嚇」などから呼ばれる
    public void ApplyDamageDealtDebuff(float percent, int duration)
    {
        damageDealtMultiplier *= 1f - percent / 100f;
        damageDealtDebuffTurns = duration;
        Debug.Log($"プレイヤーの与ダメが{percent}%減少！ 現在の倍率:x{damageDealtMultiplier:F3}");
    }
    //敵の「絡みつく」などから呼ばれる
    public void ApplyDamageTakenDebuff(float percent, int duration)
    {
        damageTakenMultiplier *= 1f + percent / 100f;
        damageTakenDebuffTurns = duration;
        Debug.Log($"プレイヤーの被ダメが{percent}%増加！ 現在の倍率:x{damageTakenMultiplier:F3}");
    }

    //Ver14用
    public void ReduceMaxHp(int amount)
    {
        maxHp -= amount;
        if (hp > maxHp)
        {
            hp = maxHp; // 現在HPが上限を超えていたら合わせる
        }
        Debug.Log($"プレイヤーの最大HPが{amount}減少！ 現在:{hp}/{maxHp}");
    }

    public void SetHpToOne()
    {
        hp = 1;
        Debug.Log($"プレイヤーの体力が1になった！ 現在:{hp}/{maxHp}");
    }
}