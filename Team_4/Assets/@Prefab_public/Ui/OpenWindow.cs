using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] List<sponeWindowSetting> settingList=new();

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
            StageManager.Instance.IsTutorialed = true;
        }
    }

    public void ShowWindow(string name)
    {
        foreach (var setting in settingList)
        {
            if(setting.name != name) continue;

            GameObject go = Instantiate(setting.window, windowSpace);
            return;
        }
    }
}
