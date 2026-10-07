using UnityEngine;

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

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        audioSource = GetComponent<AudioSource>();
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
                audioSource.PlayOneShot(sound[0].soundfile, sound[0].volume);
                break;
            default:
                int rand = Random.Range(0, sound.Length);
                audioSource.PlayOneShot(sound[rand].soundfile, sound[rand].volume);
                break;
        }
    }
}
