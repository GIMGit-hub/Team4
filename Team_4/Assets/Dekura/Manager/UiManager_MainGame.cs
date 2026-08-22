using TMPro;
using UnityEngine;

public class UiManager_MainGame : MonoBehaviour
{
    public static UiManager_MainGame Instance { get; private set; }

    [Header("UiText")]
    [SerializeField] private TextMeshProUGUI diyalogText;
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private TextMeshProUGUI floorText;

    private void Awake()
    {
        //------インスタンス化------//
        //-----DontDestoroyなし-----//
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        //---------------------------//
    }

    private void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        int stage = StageManager.Instance.nowStage;
        int floor = StageManager.Instance.nowFloor;
        int maxFloor = StageManager.Instance.stageMaxFloor[stage - 1];
        stageText.GetComponent<TextMeshProUGUI>().text = $"Stage:{stage}";
        floorText.GetComponent<TextMeshProUGUI>().text = $"Floor:{floor}/{maxFloor}";
    }

    public void SetDiyalog(string text)
    {
        diyalogText.GetComponent<TextMeshProUGUI>().text = text;
    }
}
