using UnityEditor.AdaptivePerformance.Editor;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class DeckCardUI
{
    public int deckIndex;
    public Button button;
    public GameObject equippedLabel; // "装備中"の表示物
}


public class DeckSelectUI : MonoBehaviour
{
    [SerializeField] private DeckCardUI[] deckCards;

    [Header("現在選択中のデッキ表示")]
    [SerializeField] private Image deckImage1;
    [SerializeField] private Image deckImage2;

    private void OnEnable()
    {
        RefreshCards();
       
    }

    private void RefreshCards()
    {
        if (deckCards == null)
        {
            Debug.LogError("DeckSelectUI: deckCards が設定されていません");
            return;
        }

        if (DeckManager.Instance == null)
        {
            Debug.LogError("DeckSelectUI: DeckManager.Instance が存在しません");
            return;
        }

        foreach (var card in deckCards)
        {
            if (card == null)
            {
                Debug.LogError("DeckSelectUI: 配列内のカード情報が null です");
                continue;
            }

            if (card.equippedLabel == null)
            {
                Debug.LogError(
                    $"DeckSelectUI: deckIndex={card.deckIndex} の Equipped Label が未設定です"
                );
                continue;
            }

            bool equipped = DeckManager.Instance.EquippedIndex == card.deckIndex;
            card.equippedLabel.SetActive(equipped);
        }
    }

    // 各デッキカードのOnClickに登録
    public void OnClickDeck(int index)
    {
        SoundsManager.Instance.PlaySound("accept");

        DeckManager.Instance.EquipDeck(index);
        RefreshCards();
        DeckImageSelect(index);
    }

   

    public void DeckImageSelect(int index)
    {
        if (deckImage1 == null || deckImage2 == null)
        {
            Debug.LogError("DeckImage1 または DeckImage2 が設定されていません");
            return;
        }

        if (index == 0)
        {
            //デッキ１の画像に切り替え
            deckImage1.gameObject.SetActive(true);
            deckImage2.gameObject.SetActive(false);
        }
        if(index == 1)
        {
            //デッキ２の画像に切り替え
            deckImage1.gameObject.SetActive(false);
            deckImage2.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"対応していないデッキ番号です: {index}");
        }
    }

}