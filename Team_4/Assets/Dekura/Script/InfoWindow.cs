using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class InfoWindow : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Image image;
    [SerializeField] public Sprite sprite_itigeki;
    [SerializeField] public Sprite sprite_rengeki;

    private void Awake()
    {
        closeButton.onClick.AddListener(() => Close());
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
        Destroy(gameObject);
    }
}
