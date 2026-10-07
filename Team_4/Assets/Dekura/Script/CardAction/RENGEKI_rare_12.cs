using UnityEngine;
using System.Collections;

public class RENGEKI_rare_12 : MonoBehaviour, ICardSpecialAction
{
    [SerializeField] private CardEffect.EffectTarget target;
    [SerializeField] private float damage = 100f;
    [SerializeField] private int maxCount = 10;
    [SerializeField] private int minCount = 1;
    [SerializeField] private float maxCountHpPer = 100f;
    [SerializeField] private float minCountHpPer = 10f;
    public IEnumerator SpecialAction()
    {
        float playerHpPer = Player.Instance.nowHp / Player.Instance.maxHp * 100f;
        //最大時のダメージ割合と最小時のダメージ割合を基に、現在のHP割合からダメージの割合を計算
        float percent = ((minCountHpPer - playerHpPer) / (minCountHpPer - maxCountHpPer)) / 100f;

        float finalDamage = Mathf.Min(maxCount, minCount + (maxCount - minCount) * percent);
        yield return new WaitForSeconds(CardManager.Instance.activateDuration);
    }
}
