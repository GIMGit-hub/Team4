using UnityEngine;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class DamagePopup : MonoBehaviour
{
    [Header("通常ヒット設定")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private float normalFontSize = 36f;

    [Header("クリティカル設定")]
    [SerializeField] private Color criticalColor = new Color(1f, 0.25f, 0.2f);
    [SerializeField] private float criticalFontSize = 52f;

    [Header("アニメーション設定")]
    [SerializeField] private float moveUpDistance = 80f; // UI座標(px)なので大きめの値
    [SerializeField] private float duration = 0.8f;

    // UI要素の参照
    [SerializeField] private TextMeshProUGUI damageText;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    //Managerから呼び出され、初期化
    //初期化後にpopup
    public void Setup(int damage, bool isCritical)
    {
        damageText.text = damage.ToString();

        if (isCritical)
        {
            damageText.color = criticalColor;
            damageText.fontSize = criticalFontSize;
            damageText.text += "!";
        }
        else
        {
            damageText.color = normalColor;
            damageText.fontSize = normalFontSize;
        }

        PlayAnimation(isCritical);
    }

    private void PlayAnimation(bool isCritical)
    {
        rectTransform.localScale = Vector3.zero;
        canvasGroup.alpha = 1f;

        Sequence seq = DOTween.Sequence();

        // 出現時のポップ演出
        seq.Append(rectTransform.DOScale(isCritical ? 1.3f : 1f, 0.15f).SetEase(Ease.OutBack));

        // 上に移動しつつフェードアウト
        seq.Join(rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + moveUpDistance, duration).SetEase(Ease.OutCubic));
        seq.Append(canvasGroup.DOFade(0f, 0.25f));

        seq.OnComplete(() => Destroy(gameObject));
    }
}
