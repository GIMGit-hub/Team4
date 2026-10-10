using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundsManager : MonoBehaviour
{
    public static SoundsManager Instance {  get; private set; }

    [System.Serializable]
    private class SoundsData
    {
        public string name;
        public AudioClip soundfile;
        [Range(0f, 1f)] public float volume; 
    }

    [SerializeField] private SoundsData[] sounds;
    [SerializeField] private SoundsData[] bgms;

    private AudioSource audioSource_bgm;
    private AudioSource audioSource_se;

    private void OnEnable() => SceneManager.sceneLoaded += SetSceneBGM;
    private void OnDisable() => SceneManager.sceneLoaded -= SetSceneBGM;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        audioSource_bgm = gameObject.AddComponent<AudioSource>();
        audioSource_bgm.playOnAwake = false;
        audioSource_bgm.loop = true;
        audioSource_se = gameObject.AddComponent<AudioSource>();
        audioSource_se.playOnAwake = false;
        audioSource_se.loop = false;

        SetSceneBGM(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void SetSceneBGM(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainGame") return;
        PlayBGM("normal");
    }

    public void PlaySound(string name)
    {
        //配列から該当の名前を持つ要素の検索
        var sound = System.Array.FindAll(sounds, s => s.name == name);

        switch(sound.Length)
        {
            case 0:
                Debug.LogWarning($"Sound not Found：{name}");
                break;
            case 1:
                audioSource_se.PlayOneShot(sound[0].soundfile, sound[0].volume);
                break;
            default:
                int rand = Random.Range(0, sound.Length);
                audioSource_se.PlayOneShot(sound[rand].soundfile, sound[rand].volume);
                break;
        }
    }

    public void PlayBGM(string name)
    {
        var sound = System.Array.FindAll(bgms, s => s.name == name);
        if (sound.Length == 0)
        {
            Debug.LogWarning($"BGM not Found：{name}");
            return;
        }

        if (audioSource_bgm.clip == sound[0].soundfile && audioSource_bgm.isPlaying) return;
        audioSource_bgm.clip = sound[0].soundfile;
        audioSource_bgm.volume = sound[0].volume;
        audioSource_bgm.Play();
    }

    public void StopBGM() => audioSource_bgm.Stop();
}
