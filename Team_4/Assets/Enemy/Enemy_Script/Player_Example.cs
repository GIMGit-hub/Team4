using UnityEngine;

public class Player_Example : MonoBehaviour
{
    [SerializeField] private int hp = 100;
    [SerializeField] private int damage = 1000;

    private float damageDealtMultiplier = 1f;  
    private int damageDealtDebuffTurns = 0;     

    public void Attack(EnemyController controller, GameObject target)
    {
        int finalDamage = Mathf.RoundToInt(damage * damageDealtMultiplier); // 倍率を掛ける
        controller.PlayerAttack(finalDamage, target);
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
}