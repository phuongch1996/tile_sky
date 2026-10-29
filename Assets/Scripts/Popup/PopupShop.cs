using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class PopupShop : Popup
{
    public CodelessIAPButton iapButtonRemoveAds;
    public TextMeshProUGUI[] txValueRemoveAds;
    public Image goldImgRemoveAds;
    public CodelessIAPButton iapButtonFirstPack;
    public TextMeshProUGUI[] txValueFirstPack;
    public Image goldImgFirstPack;
    public CodelessIAPButton iapButtonMegaPack;
    public TextMeshProUGUI[] txValueMegaPack;
    public Image goldImgMegaPack;
    public CodelessIAPButton iapButtonUltiPack;
    public TextMeshProUGUI[] txValueUltiPack;
    public Image goldImgUltiPack;
    public CodelessIAPButton iapButtonGold1;
    public TextMeshProUGUI txValueGold1;
    public Image goldImgGold1;
    public CodelessIAPButton iapButtonGold2;
    public TextMeshProUGUI txValueGold2;
    public Image goldImgGold2;
    public CodelessIAPButton iapButtonGold3;
    public TextMeshProUGUI txValueGold3;
    public Image goldImgGold3;
    public TextMeshProUGUI[] txPricesBooster;

    public Button[] btBuyBoosters;
    public TextMeshProUGUI[] txBoostersCount;
    public GameObject[] boostersGO;
    public RectTransform iapRect;
    public Button btClose;

    bool isInit = false;
    private void Awake0()
    {
        if (isInit) return;
        iapButtonRemoveAds.productId = IAPManager.tileevo_removeads;
        iapButtonRemoveAds.button.onClick.AddListener(() => IAPManager.instance.OnIAPClick());
        iapButtonRemoveAds.GetComponentInChildren<TextMeshProUGUI>().text = GameConfig.GetLocalizedPrice(IAPManager.tileevo_removeads);
        IAPValues iapRemoveAds = IAPManager.GetIAPValues(IAPManager.tileevo_removeads);
        goldImgRemoveAds.sprite = UnityAuthentication.instance.GetGoldSprite(iapRemoveAds.goldAm);
        txValueRemoveAds[0].text = $"x{iapRemoveAds.goldAm}";
        txValueRemoveAds[1].text = $"x{iapRemoveAds.boostersAm}";

        iapButtonFirstPack.productId = IAPManager.tileevo_firstpack;
        iapButtonFirstPack.button.onClick.AddListener(() => IAPManager.instance.OnIAPClick());
        iapButtonFirstPack.GetComponentInChildren<TextMeshProUGUI>().text = GameConfig.GetLocalizedPrice(IAPManager.tileevo_firstpack);
        IAPValues iapFirstPack = IAPManager.GetIAPValues(IAPManager.tileevo_firstpack);
        goldImgFirstPack.sprite = UnityAuthentication.instance.GetGoldSprite(iapFirstPack.goldAm);
        txValueFirstPack[0].text = $"x{iapFirstPack.goldAm}";
        txValueFirstPack[1].text = $"x{iapFirstPack.boostersAm}";

        iapButtonMegaPack.productId = IAPManager.tileevo_megapack;
        iapButtonMegaPack.button.onClick.AddListener(() => IAPManager.instance.OnIAPClick());
        iapButtonMegaPack.GetComponentInChildren<TextMeshProUGUI>().text = GameConfig.GetLocalizedPrice(IAPManager.tileevo_megapack);
        IAPValues iapMegaPack = IAPManager.GetIAPValues(IAPManager.tileevo_megapack);
        goldImgMegaPack.sprite = UnityAuthentication.instance.GetGoldSprite(iapMegaPack.goldAm);
        txValueMegaPack[0].text = $"x{iapMegaPack.goldAm}";
        txValueMegaPack[1].text = $"x{iapMegaPack.boostersAm}";

        iapButtonUltiPack.productId = IAPManager.tileevo_ultipack;
        iapButtonUltiPack.button.onClick.AddListener(() => IAPManager.instance.OnIAPClick());
        iapButtonUltiPack.GetComponentInChildren<TextMeshProUGUI>().text = GameConfig.GetLocalizedPrice(IAPManager.tileevo_ultipack);
        IAPValues iapUltiPack = IAPManager.GetIAPValues(IAPManager.tileevo_ultipack);
        goldImgUltiPack.sprite = UnityAuthentication.instance.GetGoldSprite(iapUltiPack.goldAm);
        txValueUltiPack[0].text = $"x{iapUltiPack.goldAm}";
        txValueUltiPack[1].text = $"x{iapUltiPack.boostersAm}";

        iapButtonGold1.productId = IAPManager.tileevo_gold1;
        iapButtonGold1.button.onClick.AddListener(() => IAPManager.instance.OnIAPClick());
        iapButtonGold1.GetComponentInChildren<TextMeshProUGUI>().text = GameConfig.GetLocalizedPrice(IAPManager.tileevo_gold1);
        IAPValues iapGold1 = IAPManager.GetIAPValues(IAPManager.tileevo_gold1);
        goldImgGold1.sprite = UnityAuthentication.instance.GetGoldSprite(iapGold1.goldAm);
        txValueGold1.text = $"x{iapGold1.goldAm}";

        iapButtonGold2.productId = IAPManager.tileevo_gold2;
        iapButtonGold2.button.onClick.AddListener(() => IAPManager.instance.OnIAPClick());
        iapButtonGold2.GetComponentInChildren<TextMeshProUGUI>().text = GameConfig.GetLocalizedPrice(IAPManager.tileevo_gold2);
        IAPValues iapGold2 = IAPManager.GetIAPValues(IAPManager.tileevo_gold2);
        goldImgGold2.sprite = UnityAuthentication.instance.GetGoldSprite(iapGold2.goldAm);
        txValueGold2.text = $"x{iapGold2.goldAm}";

        iapButtonGold3.productId = IAPManager.tileevo_gold3;
        iapButtonGold3.button.onClick.AddListener(() => IAPManager.instance.OnIAPClick());
        iapButtonGold3.GetComponentInChildren<TextMeshProUGUI>().text = GameConfig.GetLocalizedPrice(IAPManager.tileevo_gold3);
        IAPValues iapGold3 = IAPManager.GetIAPValues(IAPManager.tileevo_gold3);
        goldImgGold3.sprite = UnityAuthentication.instance.GetGoldSprite(iapGold3.goldAm);
        txValueGold3.text = $"x{iapGold3.goldAm}";

        txPricesBooster[0].text = $"{GameConfig.powerPrices[0]}";
        txPricesBooster[1].text = $"{GameConfig.powerPrices[1]}";
        txPricesBooster[2].text = $"{GameConfig.powerPrices[2]}";
        if (txPricesBooster.Length > 3 && txPricesBooster[3])
            txPricesBooster[3].transform.parent.gameObject.SetActive(false);
        if (btBuyBoosters.Length > 3 && btBuyBoosters[3])
            btBuyBoosters[3].gameObject.SetActive(false);
        if (boostersGO.Length > 3 && boostersGO[3])
            boostersGO[3].SetActive(false);
        isInit = true;
    }
    private void OnEnable()
    {
        GameConfig.OnPowerUpsChange += UpdateBoosterCount;
    }
    private void OnDisable()
    {
        GameConfig.OnPowerUpsChange -= UpdateBoosterCount;
    }
    void UpdateBoosterCount()
    {
        var boosterCount = GameConfig.GetPowerUps();
        txBoostersCount[0].text = $"{boosterCount[0]}";
        txBoostersCount[1].text = $"{boosterCount[1]}";
        txBoostersCount[2].text = $"{boosterCount[2]}";
        if (txBoostersCount.Length > 3 && txBoostersCount[3])
            txBoostersCount[3].text = "";
    }
    public override void SetupTweenCallback()
    {
        SetupTweenCallback(() =>
        {
            btClose.onClick.AddListener(() => Close());
            for (int i = 0; i < btBuyBoosters.Length; i++)
                SetBoosterButton(btBuyBoosters[i], i);
        });
    }
    public override void Open()
    {
        Awake0();
        foreach (Button bt in btBuyBoosters)
            bt.onClick.RemoveAllListeners();
        btClose.onClick.RemoveAllListeners();
        UpdateBoosterCount();
        iapRect.anchoredPosition = Vector2.zero;
        base.Open();
    }
    public override void Close()
    {
        base.Close();
        if (GameScript.instance)
            GameScript.instance.ContinueGame();
    }
    void SetBoosterButton(Button bt, int booster)
    {
        bt.onClick.AddListener(() => BuyBoosters(booster));
    }
    public void BuyBoosters(int booster)
    {
        int price = GameConfig.powerPrices[booster];
        if (GameConfig.UseGold(price))
        {
            GameConfig.AddPowerUps(1, booster);
            //UpdateBoosterCount();
            //boostersGO
            GameObject go = Instantiate(boostersGO[booster], transform);
            go.transform.position = boostersGO[booster].transform.position;
            go.transform.DOBlendableLocalMoveBy(new Vector3(0, 100), 1).OnStart(() =>
            {
                MaskableGraphic[] graphics = go.GetComponentsInChildren<MaskableGraphic>();
                foreach (var graphic in graphics)
                {
                    graphic.DOFade(0, 1 - Time.deltaTime);
                }
            }).OnComplete(() => { Destroy(go); });
        }
        else
        {
            ToastScript.instance.Toast(0);
        }
    }
    public void Testing()
    {
        GameConfig.AddGold(1000);
    }
}
