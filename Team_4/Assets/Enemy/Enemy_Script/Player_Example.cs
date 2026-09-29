using UnityEngine;

public class Player_Example : MonoBehaviour
{
    [SerializeField] private int hp = 100;

    // Enemy側にはこの関数そのものを渡す
    public void TakeDamage(int amount)
    {
        hp -= amount;
        Debug.Log($"プレイヤーは{amount}ダメージ！ 残りHP:{hp}");
    }
}