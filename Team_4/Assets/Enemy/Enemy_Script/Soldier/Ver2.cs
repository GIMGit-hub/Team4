using UnityEngine;

public class Enemy_Lv1 : MonoBehaviour
{
    [Header("ステータス")]
    [SerializeField] private int maxHP = 50;
    [SerializeField] private int punchBaseDamage = 8;//殴りの基本ダメージ

    [Header("強化の倍率")]
    [SerializeField] private float angerRate = 0.03f;//攻撃するたび+3%
    [SerializeField] private float chargeRate = 0.08f;//力をためる +8%

    private int hp;
    private float angerMultiplier = 1f;//怒りによる倍率
    private float chargeBonus = 0f;//次の攻撃に乗るための上乗せ
    private bool nextIsCharge = true;

    private void Awake()
    {
        hp = maxHP;
    }

    // 敵のターンに1回呼ぶ
    public void TakeTurn(IDamageable target)
    {
        if (nextIsCharge)
        {
            Charge();
        }
        else
        {
            Punch(target);
        }

        nextIsCharge = !nextIsCharge; // 次のターンは反対の行動
    }

    // ① 力を溜める: 次に与えるダメージを8%増やす
    private void Charge()
    {
        chargeBonus += chargeRate;
        Debug.Log($"{name}は力を溜めた！ 次のダメージ +{chargeBonus * 100f:F0}%");
    }

    // ② 殴り: 8ダメージ
    private void Punch(IDamageable target)
    {
        float damage = punchBaseDamage * angerMultiplier * (1f + chargeBonus);
        int finalDamage = Mathf.RoundToInt(damage);

        target.TakeDamage(finalDamage);
        Debug.Log($"{name}の殴り！ {finalDamage}ダメージ (怒り倍率 x{angerMultiplier:F3})");

        chargeBonus = 0f;                 // 溜めは使い切る
        angerMultiplier *= 1f + angerRate; // 攻撃したので怒りが3%上がる
    }

    // 敵がダメージを受ける
    public void TakeDamage(int amount)
    {
        hp -= amount;
        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }


}
