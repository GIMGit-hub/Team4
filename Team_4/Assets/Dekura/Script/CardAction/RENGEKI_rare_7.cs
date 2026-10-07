using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class RENGEKI_rare_7 : MonoBehaviour, ICardSpecialAction
{
    public IEnumerator SpecialAction()
    {
        yield return new WaitForSeconds(CardManager.Instance.activateDuration);
    }
}
