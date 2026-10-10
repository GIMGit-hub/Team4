using UnityEngine;
using UnityEngine.UI;

public class InfoWindow : MonoBehaviour, IWindowInit
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Image image;
    [SerializeField] public Sprite sprite_itigeki;
    [SerializeField] public Sprite sprite_rengeki;

    private System.Action closeAction;

    private void Awake()
    {
        closeButton.onClick.AddListener(() => Close());
        InitData();
    }

    public void WindowInit(System.Action action)
    {
        closeAction = action;
        InitData();
    }

    public void InitData()
    {
        switch (Player.Instance.GetDeckName())
        {
            case "ITIGEKI":
                image.sprite = sprite_itigeki;
                return;
            case "RENGEKI":
                image.sprite = sprite_rengeki;
                return;

            default: break;
        }
    }

    public void Close()
    {
        closeAction?.Invoke();
        Destroy(gameObject);
    }
}
