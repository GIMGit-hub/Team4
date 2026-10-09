using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneFader : MonoBehaviour
{
    private const float FadeDuration = 1.0f;

    private static SceneFader instance;
    private CanvasGroup group;
    private bool isFading;

    // 再生開始時(最初のシーンが読み込まれる前)に自動で生成される
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Create()
    {
        var go = new GameObject("SceneFader");
        instance = go.AddComponent<SceneFader>();
        DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        // フェード用のCanvasを最前面に作る
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;
        gameObject.AddComponent<GraphicRaycaster>(); // 下のUIのクリックを遮る

        group = gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;

        // 画面全体を覆う黒いImage
        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(transform, false);
        panel.GetComponent<Image>().color = Color.black;

        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    // どこからでも SceneFader.LoadScene("名前") で呼べる
    public static void LoadScene(string sceneName)
    {
        if (instance == null || instance.isFading)
        {
            if (instance == null) SceneManager.LoadScene(sceneName);
            return;
        }
        instance.StartCoroutine(instance.FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        isFading = true;
        group.blocksRaycasts = true;

        yield return Fade(0f, 1f);

        yield return SceneManager.LoadSceneAsync(sceneName);

        yield return Fade(1f, 0f);

        group.blocksRaycasts = false;
        isFading = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        float time = 0f;
        while (time < FadeDuration)
        {
            time += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, time / FadeDuration);
            yield return null;
        }
        group.alpha = to;
    }
}