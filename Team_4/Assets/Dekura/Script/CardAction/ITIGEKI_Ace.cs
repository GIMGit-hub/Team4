using System.Collections;
using UnityEngine;

public class ITIGEKI_Ace : MonoBehaviour, ICardSpecialAction
{
    [SerializeField] private CardEffect.EffectTarget target;
    [SerializeField] private float damage = 300f;
    [SerializeField] private float multipter = 1.0f;

    public IEnumerator SpecialAction()
    {
        int useCount = Player.Instance.useCardCount;
        float finalDamage = damage * useCount * multipter;

        Player.Instance.Attack(target, finalDamage, 1);
        yield return new WaitForSeconds(CardManager.Instance.activateDuration);
    }
}
