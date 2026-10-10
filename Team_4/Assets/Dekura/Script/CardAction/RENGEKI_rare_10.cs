using UnityEngine;
using System.Collections;

public class RENGEKI_rare_10 : MonoBehaviour, ICardSpecialAction
{
    [SerializeField] private CardEffect.EffectTarget target;
    [SerializeField] private float damage  = 2;
    [SerializeField] private int count = 2;

    public IEnumerator SpecialAction(EnemyUnit enemy = null)
    {
        if (Player.Instance.hitCount > 0)
        {
            Player.Instance.Attack(target, damage, count, enemy, false);
            yield return new WaitForSeconds(Player.Instance.AthDur);
        }
        Player.Instance.Attack(target, damage, count, enemy, false);
        yield return new WaitForSeconds(CardManager.Instance.activateDuration);
    }
}
