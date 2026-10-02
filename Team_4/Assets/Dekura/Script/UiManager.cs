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

    [Header("COST")]
    [SerializeField] private Image cost_active;
    [SerializeField] private Image cost_inactive;
    [SerializeField] private List<Image> costsImage = new List<Image>();
    private int lastCost = 0;

    [Header("ColisionSpace")]
    [SerializeField] private Image colisionSpace_use;

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

    private void Start()
    {
        SetLastData();
        UpdateUi();
    }

    public void UpdateUi()
    {
        float nowHp = Player.Instance.nowHp;
        float nowDp = Player.Instance.nowDp;
        int nowCost = Player.Instance.nowCost;

        foreach (var img in costsImage)
        {
            if (img == null) continue;


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
