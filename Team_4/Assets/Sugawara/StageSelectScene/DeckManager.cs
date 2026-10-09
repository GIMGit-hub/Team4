using UnityEngine;
using UnityEngine.SceneManagement;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    [Header("使用できるデッキ")]
    [SerializeField] private Deck[] decks;

    [Header("シーン名")]
    [SerializeField] private string mainSceneName = "MainGame";

    // 現在装備中のデッキ番号
    public int EquippedIndex { get; private set; } = 0;

    // 現在選択中のデッキ
    public Deck SelectedDeck
    {
        get
        {
            if (decks == null ||
                EquippedIndex < 0 ||
                EquippedIndex >= decks.Length)
            {
                return null;
            }

            return decks[EquippedIndex];
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // デッキ選択画面で呼ぶ
    public void EquipDeck(int index)
    {
        if (decks == null ||index < 0 ||index >= decks.Length)
        {
            Debug.LogError($"存在しないデッキ番号です: {index}");
            return;
        }

        if (decks[index] == null)
        {
            Debug.LogError($"デッキ {index} が設定されていません");
            return;
        }

        EquippedIndex = index;

        Debug.Log($"装備デッキ: {SelectedDeck.deckName}");

        StageManager.Instance.SelectDeck(decks[index]);
    }

    
}

