using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class Card : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler
{
    public bool isSelected { get; private set; } = false;
    private HandLayout m_handLayout;

    void Start()
    {
        m_handLayout = FindAnyObjectByType<HandLayout>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TurnManager.Instance.NowTurn != TurnManager.TurnState.PlayerTurn) return;

        SoundsManager.Instance.PlaySound("pati");
        isSelected = true;
        m_handLayout.UpdateLayout();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (TurnManager.Instance.NowTurn != TurnManager.TurnState.PlayerTurn) return;

        isSelected = false;
        m_handLayout.UpdateLayout();
    }
}
