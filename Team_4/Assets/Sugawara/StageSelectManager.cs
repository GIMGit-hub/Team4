using UnityEngine;

public class StageSelectManager : MonoBehaviour
{
    [Header("使用できるデッキ")]
    [SerializeField] private Deck deck1;
    [SerializeField] private Deck deck2;

    private Deck selectedDeck;

    public void OnClickDeck1()
    {
        selectedDeck = deck1;

        Debug.Log($"デッキ1を選択: {selectedDeck.deckName}");
    }

    public void OnClickDeck2()
    {
        selectedDeck = deck2;

        Debug.Log($"デッキ2を選択: {selectedDeck.deckName}");
    }
    public void OnClickStage1()
    {
        StartGame(1);
    }

    public void OnClickStage2()
    {
        StartGame(2);
    }

    public void OnClickStage3()
    {
        StartGame(3);
    }

    private void StartGame(int stage)
    {
        //if(selectedDeck == null)
        //{
        //    Debug.LogWarning("デッキが選択されていません！");
        //    return;
        //}

        // StageManagerにデッキを渡す
        if (selectedDeck != null)
            StageManager.Instance.SelectDeck(selectedDeck);

        // ステージを選択
        StageManager.Instance.SelectStage(stage);
    }
}