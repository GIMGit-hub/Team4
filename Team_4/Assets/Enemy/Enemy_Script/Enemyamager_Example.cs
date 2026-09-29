using UnityEngine;

public class EnemyManager_Example : MonoBehaviour
{
    [SerializeField] private EnemyController controller;
    [SerializeField] private Player_Example player;

    // 生成ボタン(引数に敵の番号を入れる)
    public void OnClickCreate(int enemyIndex)
    {
        controller.SpawnEnemy(enemyIndex, player.TakeDamage);
    }

    // 敵の行動ボタン
    public void OnClickEnemyAction()
    {
        controller.RunAllActions();
    }
}