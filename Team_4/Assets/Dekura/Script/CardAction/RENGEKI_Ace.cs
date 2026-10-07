using UnityEngine;
using System.Collections;

public class RENGEKI_Ace : MonoBehaviour, ICardSpecialAction
{
    [SerializeField] private CardEffect.EffectTarget target;
    [SerializeField] private float damage = 100f;
    [SerializeField] private int multipter = 2;

    public IEnumerator SpecialAction()
    {
        int hitCount = Player.Instance.hitCount * multipter;

        Player.Instance.Attack(target, damage, hitCount);
        yield return new WaitForSeconds(CardManager.Instance.activateDuration);
    }
}
