using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class StagePanelUI
{
    public int stageNumber;
    public Button button;
    public GameObject newLabel;   // "NEW!!"の表示物
    public GameObject clearLabel; // "CLEAR!!"の表示物
    public GameObject lockedOverlay; // 未解放のグレーアウト
}

public class StageSelectUI : MonoBehaviour
{
    [SerializeField] private StagePanelUI[] stagePanels;
    [SerializeField] private FadeScreen fadeScreen;
    [SerializeField] private GameObject stagePanelRoot; // ステージ画面全体
    [SerializeField] private GameObject deckPanelRoot;  // デッキ画面全体

    private void Start()
    {
        RefreshPanels();
        ShowStagePanel();
    }

    private void RefreshPanels()
    {
        foreach (var panel in stagePanels)
        {
            bool unlocked = StageManager.Instance.IsStageUnlocked(panel.stageNumber);
            bool cleared = StageManager.Instance.IsStageCleared(panel.stageNumber);

            panel.button.interactable = unlocked;
            panel.lockedOverlay.SetActive(!unlocked);
            panel.newLabel.SetActive(unlocked && !cleared);
            panel.clearLabel.SetActive(cleared);
        }
    }

    // 各ステージボタンのOnClickに登録
    public void OnClickStage(int stageNumber)
    {
        SoundsManager.Instance.PlaySound("accept");
        StageManager.Instance.SelectStage(stageNumber);
    }

    // デッキボタンのOnClickに登録
    public void OnClickDeckButton()
    {
        SoundsManager.Instance.PlaySound("accept");
        fadeScreen.FadeOutIn(() =>
        {
            ShowDeckPanel();
        });
    }

    // デッキ画面のOKボタンから呼ばれる
    public void OnClickDeckOK()
    {
        SoundsManager.Instance.PlaySound("accept");
        fadeScreen.FadeOutIn(() =>
        {
            ShowStagePanel();
        });
    }

    private void ShowStagePanel()
    {
        stagePanelRoot.SetActive(true);
        deckPanelRoot.SetActive(false);
    }

    private void ShowDeckPanel()
    {
        stagePanelRoot.SetActive(false);
        deckPanelRoot.SetActive(true);
    }
}