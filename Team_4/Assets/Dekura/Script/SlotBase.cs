using UnityEditor.Rendering.Universal;
using UnityEngine;
using DG.Tweening;

public class SlotBase : MonoBehaviour
{
    private HandLayout handLayout;

    [Header("出現アニメーション")]
    [SerializeField] private RectTransform visualRoot;     // 動かす対象(未指定なら自分自身)
    [SerializeField] private Vector2 collapsedPos;         // 普段(ほぼ隠れている)位置
    [SerializeField] private Vector2 expandedPos;          // カードが近づいた時の位置
    [SerializeField] private float detectionRadius = 300f; // この距離に入ったら展開
    [SerializeField] private float tweenDuration = 0.2f;
    [SerializeField] private float hoverY = -100f;

    private RectTransform rect;

    private void Awake()
    {
    }

    private void Update()
    {
    }
}
