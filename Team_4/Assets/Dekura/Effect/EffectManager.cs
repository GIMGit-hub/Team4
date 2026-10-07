using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    [System.Serializable]
    private class EffectData
    {
        public string name;
        public ParticleSystem particle;
        [Header("Loopする場合に設定")]
        public bool loop = false;
        public float loopDuration = 0f;
    }

    [SerializeField] private Canvas canvas;
    [SerializeField] private Image fade;
    [SerializeField] private EffectData[] effects;

    [SerializeField] private Color healColor;
    [SerializeField] private Color damageColor;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    public void Playfade(string name, float delay = 0f)
    {
        fade.DOKill();
        Color color;

        switch (name)
        {
            case "heal":
                color = healColor;
                break;
            case "damage":
                color = damageColor;
                break;
            default:
                return;
        }

        DOTween.Sequence()
        .SetTarget(fade)
        .Append(fade.DOColor(color, 0.1f))
        .AppendInterval(delay)
        .Append(fade.DOFade(0f, 1f));
    }

    public void PlayEffect(string name, Vector3 position)
    {
        //配列から該当の名前を持つ要素の検索
        var effectData = System.Array.Find(effects, e => e.name == name);
        if (effectData == null)
        {
            Debug.LogWarning($"Effect not Found：{name}");
            return;
        }

        // ここで生成
        ParticleSystem effectInstance = Instantiate(effectData.particle, position, Quaternion.identity);
        //effectInstance.transform.position

        if (!effectData.loop)
            Destroy(effectInstance.gameObject, effectInstance.main.duration + effectInstance.main.startLifetime.constantMax);
        else
            StartCoroutine(StopLoopEffectAfter(effectInstance, effectData.loopDuration));
    }

    //レイヤーを子オブジェクトも含めてNoBloomにし、光らせないcamaraで描画させる
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private IEnumerator StopLoopEffectAfter(ParticleSystem ps, float duration)
    {
        yield return new WaitForSeconds(duration);
        ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        Destroy(ps.gameObject, ps.main.startLifetime.constantMax);
    }
}
