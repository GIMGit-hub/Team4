
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ClearSceneController : MonoBehaviour
{
    [Header("フェード用の黒いImage")]
    [SerializeField] private Image fadeImage;

    [Header("タイトルシーン名")]
    [SerializeField] private string titleSceneName = "TitleScene";

    private void Start()
    {
        // 最初は透明にする
        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;

        StartCoroutine(ClearSequence());
    }

    private IEnumerator ClearSequence()
    {
        // クリア画面を10秒間表示
        yield return new WaitForSeconds(10f);

        // 約3秒かけてフェードアウト
        float elapsed = 0f;
        const float fadeDuration = 3f;

        Color color = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            color.a = Mathf.Clamp01(elapsed / fadeDuration);
            fadeImage.color = color;

            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;

        // タイトル画面へ遷移
        SceneManager.LoadScene(titleSceneName);
    }
}