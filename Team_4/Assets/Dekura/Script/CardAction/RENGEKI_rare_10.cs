using UnityEngine;
using System.Collections;

public class RENGEKI_rare_10 : MonoBehaviour, ICardSpecialAction
{
    public IEnumerator SpecialAction()
    {
        yield return new WaitForSeconds(CardManager.Instance.activateDuration);
    }
}
