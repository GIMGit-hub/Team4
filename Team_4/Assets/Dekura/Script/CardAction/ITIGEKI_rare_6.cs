using UnityEngine;
using System.Collections;

public class ITIGEKI_rare_6 : MonoBehaviour, ICardSpecialAction
{
    [SerializeField] private CardEffect.EffectTarget target;
    [SerializeField] private float maxDamage = 500f;
    [SerializeField] private float minDamage = 250f;
    [SerializeField] private float maxDamageHpPer = 50f;
    [SerializeField] private float minDamageHpPer = 100f;

    public IEnumerator SpecialAction(EnemyUnit enemy = null)
    {
        Debug.Log("ITIGEKI_rare_6 SpecialAction executed.");

        //現在のHP割合
        float playerHpPer = Player.Instance.nowHp / Player.Instance.maxHp * 100f;
        //最大時のダメージ割合と最小時のダメージ割合を基に、現在のHP割合からダメージの割合を計算
        float percent = ((minDamageHpPer - playerHpPer) / (minDamageHpPer - maxDamageHpPer)) / 100f;

        float finalDamage = Mathf.Min(maxDamage, minDamage + (maxDamage - minDamage) * percent);

        Player.Instance.Attack(target, finalDamage, 1, enemy);
        yield return new WaitForSeconds(CardManager.Instance.activateDuration);
    }
}
