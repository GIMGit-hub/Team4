using UnityEngine;
using System.Collections;

public class RENGEKI_rare_8 : MonoBehaviour, ICardSpecialAction
{
    [SerializeField] private CardEffect.EffectTarget target;
    [SerializeField] private float damage = 100f;
    [SerializeField] private int count = 2;
    [SerializeField] private float damageUp = 50f;
    public IEnumerator SpecialAction(EnemyUnit enemy = null)
    {
        count+=(int)Player.Instance.GetEffect(CardEffect.EffectType.CountBuff);

        for (int i = 0; i < count; i++) 
        {
            float finalDamage = damage + damageUp * i;
            Player.Instance.Attack(target, finalDamage, 1, enemy, false);
            yield return new WaitForSeconds(Player.Instance.AthDur);
        }
        yield return new WaitForSeconds(CardManager.Instance.activateDuration);
    }
}
