using UnityEditor.Rendering.Universal;
using UnityEngine;
using DG.Tweening;

public class SlotBase : MonoBehaviour
{
    private HandLayout handLayout;

    [SerializeField] private float hoverY = -100f;
    private Vector2 basePosition;

    private RectTransform rect;

    private void Awake()
    {
        basePosition = gameObject.transform.localPosition;
        handLayout = FindAnyObjectByType<HandLayout>();
        rect = GetComponent<RectTransform>();
    }

    private void Update()
    {
    }
}
