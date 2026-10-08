using Unity.VisualScripting;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }

    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private DamagePopup damagePopupPrefab;

    [Header("表示位置のブレ設定")]
    [SerializeField] private float randomOffsetRangeX = 20f; // px単位

    //[SerializeField] private Canvas canvas;

    void Awake()
    {
        //インスタンス化のみ
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// ワールド座標を基準にダメージ数字を表示する
    /// </summary>
    public void ShowDamage(Vector3 worldPosition, int damage, bool isCritical)
    {
        if (damagePopupPrefab == null || canvasRect == null) return;

        Canvas canvas = canvasRect.GetComponentInParent<Canvas>();
        Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        // Overlayはworldpositionがそのままスクリーン座標
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, worldPosition);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint,
            null,
            out Vector2 localPoint
        );

        localPoint.x += Random.Range(-randomOffsetRangeX, randomOffsetRangeX);

        //ここで生成
        DamagePopup popup = Instantiate(damagePopupPrefab, canvasRect);
        popup.GetComponent<RectTransform>().anchoredPosition = localPoint;
        popup.Setup(damage, isCritical);
    }
}
