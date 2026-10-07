using System.Collections;
using UnityEngine;

public class RENGEKI_rare_7 : MonoBehaviour, ICardSpecialAction
{
    [SerializeField] private CardEffect.EffectTarget target;
    [SerializeField] private float damage = 50f;
    [SerializeField] private int count = 4;
    [SerializeField] private float heal = 5f;
    public IEnumerator SpecialAction()
    {
        Player.Instance.Attack(target, damage, count);
        for(int i = 0; i < Player.Instance.hitCount; i++)
        {
            Player.Instance.HpHeal(heal);
            yield return new WaitForSeconds(CardManager.Instance.activateDuration);
        }
    }
}
