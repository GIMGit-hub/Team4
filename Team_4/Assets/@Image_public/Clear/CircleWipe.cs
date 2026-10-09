using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// これもAI全投げ
/// ゆるせ
/// 全画面Canvas上のImageにアタッチ。Materialに UI/CircleWipe を割り当てる。
/// 使い方: wipe.Play(0f, 1f, 0.6f, onComplete);  // 中心から広がって画面を覆う
///         wipe.Play(1f, 0f, 0.6f);               // 覆った状態から中心へ縮んで開く
/// </summary>
[RequireComponent(typeof(Image))]
public class CircleWipe : MonoBehaviour
{
    public static CircleWipe Instance { get; private set; }

    static readonly int ProgressId = Shader.PropertyToID("_Progress");
    static readonly int AspectId = Shader.PropertyToID("_Aspect");
    static readonly int ColorId = Shader.PropertyToID("_Color");

    [SerializeField] AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    Image image;
    Material mat;
    Coroutine running;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        image = GetComponent<Image>();
        mat = new Material(image.material); // インスタンス化して共有マテリアルを汚さない
        image.material = mat;
        image.raycastTarget = false;
        SetProgress(0f);
    }

    void OnDestroy()
    {
        if (mat != null) Destroy(mat);
    }

    public void SetProgress(float t)
    {
        var rt = (RectTransform)transform;
        float aspect = rt.rect.height > 0 ? rt.rect.width / rt.rect.height : 1f;
        mat.SetFloat(AspectId, aspect);
        mat.SetFloat(ProgressId, t);
    }

    public Coroutine Play(float from, float to, float duration, Color color, Action onComplete = null)
    {
        if (running != null) StopCoroutine(running);
        mat.SetColor(ColorId, color);
        return running = StartCoroutine(Run(from, to, duration, onComplete));
    }

    IEnumerator Run(float from, float to, float duration, Action onComplete)
    {
        float time = 0f;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime; // Time.timeScale=0 でも動く
            SetProgress(Mathf.Lerp(from, to, curve.Evaluate(time / duration)));
            yield return null;
        }
        SetProgress(to);
        running = null;
        onComplete?.Invoke();
    }
}
