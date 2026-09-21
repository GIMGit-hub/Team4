using NUnit.Framework.Internal;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    private EffectManager effect;

    public float nowHp { get; private set; }
    public int nowCost { get; private set; }

    [Header("最大/開始時ステータス")]
    [SerializeField] private float maxHp = 100f;
    [SerializeField] private float startHp = 100f;
    [SerializeField] private int maxCost = 5;
    [SerializeField] private int startCost = 3;

    [Header("使用デッキ")]
    [SerializeField] public Deck deck;

    [Header("コンポーネント")]
    [SerializeField] private Image hp_Image;
    [SerializeField] private Image cost_Image;

    [Header("debug用ウィンドウ")]
    [SerializeField] private TextMeshProUGUI debugWindow;

    // private List<ここにバフobj> buffList = new ();
    // private List<ここにバフobj> debuffList = new ();

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

        effect = FindAnyObjectByType<EffectManager>();
        nowHp = startHp;
        nowCost = startCost;

        UpdateUi();
    }

    private void UpdateUi()
    {
        debugWindow.text =
            $"HP  :: {nowHp} / {maxHp}\n" +
            $"COST:: {nowCost} / {maxCost}";
    }


    //被弾処理

    public void HpHeal(float value)
    {
        nowHp = Mathf.Min(nowHp + value, maxHp);
        UpdateUi();
        effect.Playfade("heal");
    }
    public void CostHeal(int value)
    {
        nowCost = Mathf.Min(nowCost + value, maxCost);
        UpdateUi();
    }

    public bool CanUseCost(int cost) => nowCost >= cost;
    public void UseCost(int cost)
    {
        nowCost -= cost;
        UpdateUi();
    }

    //所持バフのターン減少(turnMGからevent発火)
}
