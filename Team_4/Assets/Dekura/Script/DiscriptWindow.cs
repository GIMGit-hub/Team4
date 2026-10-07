using UnityEngine;

public class DiscriptWindow : MonoBehaviour
{
    public static DiscriptWindow Instance { get; private set; }

    [SerializeField] private GameObject window;
    [SerializeField] private Vector2 offset;

    public bool isOpen { get; private set; } = false;

    private GameObject OpenedWindow;

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
    }

    public void OpenWindow(GameObject card, CardInstance cardInstance)
    {
        if (isOpen) return;

        RectTransform cardRect = card.GetComponent<RectTransform>();
        OpenedWindow = Instantiate(window, cardRect, false);
        RectTransform windowRect = OpenedWindow.GetComponent<RectTransform>();

        float cardWidth = cardRect.rect.width;
        float windowWidth = windowRect.rect.width;

        windowRect.anchoredPosition = cardRect.position.x > Screen.width / 2
            ? new Vector2(-cardWidth / 2 - windowWidth / 2, 0.0f) - offset
            : new Vector2( cardWidth / 2 + windowWidth / 2, 0.0f) + offset;

        OpenedWindow.GetComponent<DiscriptWinUi>().SetCardInfo(cardInstance);

        isOpen = true;
    }

    public void CloseWindow()
    {
        if (!isOpen) return;
        Destroy(OpenedWindow);
        isOpen = false;
    }
}
