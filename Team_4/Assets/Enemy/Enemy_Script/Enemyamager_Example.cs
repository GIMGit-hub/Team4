using UnityEngine;

public class EnemyManager_Example : MonoBehaviour
{
    [SerializeField] private EnemyController controller;
    [SerializeField] private Player_Example player;

    private GameObject selectedTarget;

    // 生成ボタン
    public void OnClickCreate(int enemyIndex)
    {
        controller.SpawnEnemy(enemyIndex, player.TakeDamage, this);
    }

    // 敵をクリックしたときに呼ばれる(EnemyUnit側から)
    public void OnSelectTarget(GameObject target)
    {
        selectedTarget = target;
        Debug.Log($"攻撃対象: {target.name} を選択");
    }

    // 行動ボタン: プレイヤー攻撃(選択中の相手) → 敵全員の攻撃
    public void OnClickEnemyAction()
    {
        Debug.Log("=== OnClickEnemyAction 呼び出し ===");

        if (selectedTarget == null)
        {
            Debug.Log("攻撃対象が選ばれていません");
        }
        else
        {
            player.Attack(controller, selectedTarget);
        }

        controller.RunAllActions();
    }
}