using UnityEngine;
using System.Collections;

public class ITIGEKI_DispectAce : MonoBehaviour, ICardSpecialAction
{
    [SerializeField] private CardEffect.EffectTarget target;
    [SerializeField] private float damage = 999999f;
    [SerializeField] private float multipter = 1.0f;

    public IEnumerator SpecialAction(EnemyUnit enemy = null)
    {
        Player.Instance.Attack(target, damage, 1, enemy);
        Player.Instance.AddEffect_Count(CardEffect.EffectType.Revive, 1, 1);
        yield return new WaitForSeconds(CardManager.Instance.activateDuration);
    }
}
