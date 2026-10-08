using UnityEngine;
using System.Collections;

public class RENGEKI_rare_10 : MonoBehaviour, ICardSpecialAction
{
    [SerializeField] private CardEffect.EffectTarget target;
    [SerializeField] private int count = 2;
    public IEnumerator SpecialAction(EnemyUnit enemy = null)
    {
        yield return new WaitForSeconds(CardManager.Instance.activateDuration);
    }
}
