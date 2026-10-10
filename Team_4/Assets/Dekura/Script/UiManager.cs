using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    [Header("BackGround")]
    [SerializeField] private SpriteRenderer background;
    [SerializeField] Vector3 proceedScareVolume = new Vector3(0.1f, 0.1f, 0f);
    [SerializeField] float amplitude = 0.1f; // 上下の振れ幅
    [SerializeField] float frequency = 2f;   // 1秒あたりの歩数(上下回数)

    [Header("StageInfo")]
    [SerializeField] private GameObject obj;
    [SerializeField] Transform parent;
    [SerializeField] private Vector2 sponePosition;
    [SerializeField] private float fadeDuriation = 0.1f;
    [SerializeField] private float proceedDuriation = 1.5f;

    public Sprite BackGround { set => background.sprite = value; }

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

    public IEnumerator ProceedDirection(int stage, int floor)
    {
        Transform ts = background.GetComponent<Transform>();
        ts.DOScale(ts.localScale + proceedScareVolume, proceedDuriation);

        Vector3 basePos = ts.localPosition;

        GameObject go = Instantiate(obj, parent);
        TextMeshProUGUI textUi = go.GetComponentInChildren<TextMeshProUGUI>();
        CanvasGroup cg = go.GetComponent<CanvasGroup>();

        int nowFloor = StageManager.Instance.CurrentFloor;
        int maxFloor = StageManager.Instance.GetMaxFloor(StageManager.Instance.CurrentStage);

        if (nowFloor == maxFloor)
        {
            textUi.text = $"- BOSS BATTLE!! -";
            EffectManager.Instance.Playfade("damage", proceedDuriation);
        }
        else
        {
            textUi.text = $"- BATTLE {nowFloor} / {maxFloor} -";
        }

        float t = 0f;
        while (t < proceedDuriation) 
        {
            t += Time.deltaTime;
            // abs(sin)で「着地で跳ね返る」歩行っぽい動きになるらしい
            float offset = Mathf.Abs(Mathf.Sin(t * frequency * Mathf.PI)) * amplitude;
            ts.localPosition = basePos + Vector3.up * offset;
            yield return null; //次回ここから再開する
        }

        ts.localPosition = basePos;  // 元の位置に戻す
        cg.DOFade(0f, fadeDuriation)
            .SetEase(Ease.InOutExpo)
            .OnComplete(() => Destroy(go));

        yield return new WaitForSeconds(0.5f);
    }
}
