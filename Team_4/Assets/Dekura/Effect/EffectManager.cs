using Unity.VisualScripting;
using UnityEngine;
using System.Collections;


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

    [SerializeField] private EffectData[] effects;

    private int fxLayer;


    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;

        fxLayer = LayerMask.NameToLayer("NoBloom");
        if (fxLayer == -1) Debug.LogError($"Layer not Found：'NoBloom'");
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
        // レイヤーの変更
        SetLayerRecursively(effectInstance.gameObject, fxLayer);

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
