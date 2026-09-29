using UnityEngine;

public class Player_Example : MonoBehaviour
{
    [SerializeField] private int hp = 100;
    [SerializeField] private int damage = 10;

    public void Attack(EnemyController controller, GameObject target)
    {
        controller.PlayerAttack(damage, target);
    }

    public void TakeDamage(int amount)
    {
        hp -= amount;
        Debug.Log($"プレイヤーは{amount}ダメージ！ 残りHP:{hp}");
    }
}