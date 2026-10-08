using NUnit.Framework.Internal;
using System.Collections;
using UnityEngine;

public class RENGEKI_Ace : MonoBehaviour, ICardSpecialAction
{
    [SerializeField] private CardEffect.EffectTarget target;
    [SerializeField] private float damage = 100f;
    [SerializeField] private int multipter = 2;

    public IEnumerator SpecialAction(EnemyUnit enemy = null)
    {
        int hitCount = Player.Instance.hitCount * multipter;

        Player.Instance.Attack(target, damage, hitCount, enemy);
        yield return new WaitForSeconds(CardManager.Instance.activateDuration);
    }
}
