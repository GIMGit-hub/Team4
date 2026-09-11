using UnityEditor.Rendering.Universal;
using UnityEngine;

public class SlotBase : MonoBehaviour
{
    [SerializeField] private float hoverY = -100f;
    private Vector2 basePosition;
    private void Awake()
    {
        basePosition = gameObject.transform.localPosition;
    }

    private void Update()
    {
    }
}
