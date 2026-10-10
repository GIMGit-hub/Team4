using DG.Tweening;
using System.Collections;
using UnityEngine;

public class ClearManager : MonoBehaviour
{
    public static ClearManager Instance { get; private set; }

    [Header("クリアエフェクト")]
    [SerializeField] GameObject window_clear;
    [SerializeField] Color color_clear;
    [SerializeField] GameObject window_lose;
    [SerializeField] Color color_lose;
    [SerializeField] Transform parent;
    [SerializeField] private Vector2 sponePosition;
    [SerializeField] private Vector2 slowPosition;
    [SerializeField] private Vector2 endPosition = new Vector2(0f, -700f);
    [SerializeField] private float showduriation = 1.5f;
    [SerializeField] private float moveduriation = 0.5f;

    [Header("シーン名")]
    [SerializeField] private string mainSceneName = "MainGame";
    [SerializeField] private string stageSelectSceneName = "StageSelectScene";

    [SerializeField] CircleWipe circleWipe;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    public IEnumerator PlayClear()
    {
        GameObject go = Instantiate(window_clear, parent);
        RectTransform rect = go.GetComponent<RectTransform>();
        CanvasGroup cg = go.GetComponent<CanvasGroup>();

        SoundsManager.Instance.PlaySound("win");

        //Append>>終了を待って次を開始
        //Joim>>ひとつ前のAppendと同時に処理、Appendはこれも待つ
        var seq =
            DOTween.Sequence()
            .AppendCallback(() => rect.anchoredPosition = sponePosition)
            .Append(rect.DOAnchorPos(slowPosition, moveduriation).SetEase(Ease.OutExpo))
            .AppendInterval(showduriation)
            .Append(rect.DOAnchorPos(endPosition, moveduriation).SetEase(Ease.InExpo))
            .Join(cg.DOFade(0f, moveduriation).SetEase(Ease.InOutExpo));

        yield return seq.WaitForCompletion();
        yield return new WaitForSeconds(0.2f);
        yield return circleWipe.Play(0f, 1f, 0.6f, color_clear);
    }

    public IEnumerator PlayLose()
    {
        GameObject go = Instantiate(window_lose, parent);
        RectTransform rect = go.GetComponent<RectTransform>();
        CanvasGroup cg = go.GetComponent<CanvasGroup>();

        SoundsManager.Instance.PlaySound("lose");

        //Append>>終了を待って次を開始
        //Joim>>ひとつ前のAppendと同時に処理、Appendはこれも待つ
        var seq =
            DOTween.Sequence()
            .AppendCallback(() => rect.anchoredPosition = sponePosition)
            .Append(rect.DOAnchorPos(slowPosition, moveduriation).SetEase(Ease.OutExpo))
            .AppendInterval(showduriation)
            .Append(rect.DOAnchorPos(endPosition, moveduriation).SetEase(Ease.InExpo))
            .Join(cg.DOFade(0f, moveduriation).SetEase(Ease.InOutExpo));

        yield return seq.WaitForCompletion();
        yield return new WaitForSeconds(0.2f);
        yield return circleWipe.Play(0f, 1f, 0.6f, color_lose);
    }
}
