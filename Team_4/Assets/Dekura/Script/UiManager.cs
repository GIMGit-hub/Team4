using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance { get; private set; }

    [Header("HP")]
    [SerializeField] private Image playerHpBar_hp;
    [SerializeField] private Image playerHpBar_dp;
    [SerializeField] private Image playerHpBar_dmg;
    private float lastHp = 0;
    private float lastDp = 0;
    private float hpBar_size = 0;

    [Header("COST")]
    [SerializeField] private Sprite cost_active;
    [SerializeField] private Sprite cost_inactive;
    [SerializeField] private List<Image> costsImage = new List<Image>();
    private int lastCost = 0;

    [Header("ColisionSpace")]
    [SerializeField] private Image colisionSpace_use;

    private void OnEnable() => SetEventSubscribed(true);
    private void OnDisable() => SetEventSubscribed(false);

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

        hpBar_size = playerHpBar_hp.GetComponent<RectTransform>().sizeDelta.x;
    }

    private void Start()
    {
        SetEventSubscribed(true);

        SetLastData();
        UpdateUi();
    }

    private void SetEventSubscribed(bool isEnable)
    {
        if (CardManager.Instance != null)
        {
            CardManager.Instance.OnCardUsed -= UpdateUi;
            if (isEnable) CardManager.Instance.OnCardUsed += UpdateUi;
        }
        if (Player.Instance != null)
        {
            Player.Instance.HpMoved -= UpdateUi;
            if (isEnable) Player.Instance.HpMoved += UpdateUi;
        }
    }

    public void UpdateUi()
    {
        float nowHp = Player.Instance.nowHp;
        float maxHp = Player.Instance.maxHp;
        float nowDp = Player.Instance.nowDp;
        int nowCost = Player.Instance.nowCost;

        float diff = nowHp / maxHp;
        float sizediff = hpBar_size * diff;
        playerHpBar_hp.GetComponent<RectTransform>().sizeDelta = new Vector2(sizediff, playerHpBar_hp.GetComponent<RectTransform>().sizeDelta.y);

        diff = nowDp / maxHp;
        sizediff = hpBar_size * diff;
        playerHpBar_dp.GetComponent<RectTransform>().sizeDelta = new Vector2(sizediff, playerHpBar_dp.GetComponent<RectTransform>().sizeDelta.y);

        for (int i = 0; i < costsImage.Count; i++) 
        {
            if (costsImage[i] == null) continue;

            if (nowCost > i) costsImage[i].sprite = cost_active;
            else costsImage[i].sprite = cost_inactive;
        }

        SetLastData();
    }

    private void SetLastData()
    {
        lastHp = Player.Instance.nowHp;
        lastDp = Player.Instance.nowDp;
        lastCost = Player.Instance.nowCost;
    }
}
