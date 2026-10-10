using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IWindowInit
{
    void WindowInit(System.Action action);
}

public class OpenWindow : MonoBehaviour
{
    public static OpenWindow Instance { get; private set; }

    [System.Serializable]
    public class sponeWindowSetting
    {
        public string name;
        public Button button;
        public GameObject window;
    }

    [Header("合成リスト用")]
    [SerializeField] Transform windowSpace;
    [SerializeField] List<sponeWindowSetting> settingList = new();

    private GameObject showingWindow;

    private void Awake()
    {
        //------インスタンス化------//
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        //---------------------------//

        foreach (var setting in settingList)
        {
            setting.button.onClick.AddListener(()=>ShowWindow(setting.name));
        }
    }
    private void Start()
    {
        if (!StageManager.Instance.IsTutorialed)
        {
            ShowWindow("Tutorial");
        }
    }

    public void ShowWindow(string name)
    {
        if (showingWindow != null) return;

        SoundsManager.Instance.PlaySound("info");

        foreach (var setting in settingList)
        {
            if(setting.name != name) continue;

            GameObject go = Instantiate(setting.window, windowSpace);
            go.GetComponent<IWindowInit>().WindowInit(CloseWindow);
            showingWindow = go;
            return;
        }

        Debug.LogError("window_notfound");
    }

    public void CloseWindow()
    {
        if (showingWindow == null) return;

        showingWindow = null;
        SoundsManager.Instance.PlaySound("accept");

        if (!StageManager.Instance.IsTutorialed)
        {
            StageManager.Instance.IsTutorialed = true;
            TurnManager.Instance.NowFloorAnnnounse();
        }
    }
}
