using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public partial class PopupShop : Popup
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
        EnsureTileSkyShop();
        if (isInit) return;
        iapButtonRemoveAds.productId = IAPManager.tilesky_removeads;
        iapButtonRemoveAds.button.onClick.AddListener(() => IAPManager.instance.OnIAPClick());
        iapButtonRemoveAds.GetComponentInChildren<TextMeshProUGUI>().text = ShopPrice(IAPManager.tilesky_removeads);
        IAPValues iapRemoveAds = IAPManager.GetIAPValues(IAPManager.tilesky_removeads);
        goldImgRemoveAds.sprite = UnityAuthentication.instance.GetGoldSprite(iapRemoveAds.goldAm);
        txValueRemoveAds[0].text = iapRemoveAds.goldAm.ToString();
        txValueRemoveAds[1].text = $"x{iapRemoveAds.boostersAm}";

        iapButtonFirstPack.productId = IAPManager.tilesky_firstpack;
        iapButtonFirstPack.button.onClick.AddListener(() => IAPManager.instance.OnIAPClick());
        iapButtonFirstPack.GetComponentInChildren<TextMeshProUGUI>().text = ShopPrice(IAPManager.tilesky_firstpack);
        IAPValues iapFirstPack = IAPManager.GetIAPValues(IAPManager.tilesky_firstpack);
        goldImgFirstPack.sprite = UnityAuthentication.instance.GetGoldSprite(iapFirstPack.goldAm);
        txValueFirstPack[0].text = iapFirstPack.goldAm.ToString();
        txValueFirstPack[1].text = $"x{iapFirstPack.boostersAm}";

        iapButtonMegaPack.productId = IAPManager.tilesky_megapack;
        iapButtonMegaPack.button.onClick.AddListener(() => IAPManager.instance.OnIAPClick());
        iapButtonMegaPack.GetComponentInChildren<TextMeshProUGUI>().text = ShopPrice(IAPManager.tilesky_megapack);
        IAPValues iapMegaPack = IAPManager.GetIAPValues(IAPManager.tilesky_megapack);
        goldImgMegaPack.sprite = UnityAuthentication.instance.GetGoldSprite(iapMegaPack.goldAm);
        txValueMegaPack[0].text = iapMegaPack.goldAm.ToString();
        txValueMegaPack[1].text = $"x{iapMegaPack.boostersAm}";

        iapButtonUltiPack.productId = IAPManager.tilesky_ultipack;
        iapButtonUltiPack.button.onClick.AddListener(() => IAPManager.instance.OnIAPClick());
        iapButtonUltiPack.GetComponentInChildren<TextMeshProUGUI>().text = ShopPrice(IAPManager.tilesky_ultipack);
        IAPValues iapUltiPack = IAPManager.GetIAPValues(IAPManager.tilesky_ultipack);
        goldImgUltiPack.sprite = UnityAuthentication.instance.GetGoldSprite(iapUltiPack.goldAm);
        txValueUltiPack[0].text = iapUltiPack.goldAm.ToString();
        txValueUltiPack[1].text = $"x{iapUltiPack.boostersAm}";

        iapButtonGold1.productId = IAPManager.tilesky_gold1;
        iapButtonGold1.button.onClick.AddListener(() => IAPManager.instance.OnIAPClick());
        iapButtonGold1.GetComponentInChildren<TextMeshProUGUI>().text = ShopPrice(IAPManager.tilesky_gold1);
        IAPValues iapGold1 = IAPManager.GetIAPValues(IAPManager.tilesky_gold1);
        goldImgGold1.sprite = UnityAuthentication.instance.GetGoldSprite(iapGold1.goldAm);
        txValueGold1.text = iapGold1.goldAm.ToString();

        iapButtonGold2.productId = IAPManager.tilesky_gold2;
        iapButtonGold2.button.onClick.AddListener(() => IAPManager.instance.OnIAPClick());
        iapButtonGold2.GetComponentInChildren<TextMeshProUGUI>().text = ShopPrice(IAPManager.tilesky_gold2);
        IAPValues iapGold2 = IAPManager.GetIAPValues(IAPManager.tilesky_gold2);
        goldImgGold2.sprite = UnityAuthentication.instance.GetGoldSprite(iapGold2.goldAm);
        txValueGold2.text = iapGold2.goldAm.ToString();

        iapButtonGold3.productId = IAPManager.tilesky_gold3;
        iapButtonGold3.button.onClick.AddListener(() => IAPManager.instance.OnIAPClick());
        iapButtonGold3.GetComponentInChildren<TextMeshProUGUI>().text = ShopPrice(IAPManager.tilesky_gold3);
        IAPValues iapGold3 = IAPManager.GetIAPValues(IAPManager.tilesky_gold3);
        goldImgGold3.sprite = UnityAuthentication.instance.GetGoldSprite(iapGold3.goldAm);
        txValueGold3.text = iapGold3.goldAm.ToString();

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
        GameConfig.OnGoldChange += UpdateShopGold;
    }
    private void OnDisable()
    {
        GameConfig.OnPowerUpsChange -= UpdateBoosterCount;
        GameConfig.OnGoldChange -= UpdateShopGold;
    }
    void UpdateBoosterCount()
    {
        var boosterCount = GameConfig.GetPowerUps();
        for (int i = 0; i < 3; i++)
            txBoostersCount[i].text = shopArt ? $"Owned: {boosterCount[i]}" : $"{boosterCount[i]}";
        if (txBoostersCount.Length > 3 && txBoostersCount[3])
            txBoostersCount[3].text = "";
    }
    public override void SetupTweenCallback()
    {
        EnsureTileSkyShop();
        SetupTweenCallback(() =>
        {
            btClose.onClick.AddListener(() => Close());
            for (int i = 0; i < Mathf.Min(3, btBuyBoosters.Length); i++)
                SetBoosterButton(btBuyBoosters[i], i);
        });
    }
    public override void Open()
    {
        Awake0();
        FitTileSkyShop();
        UpdateShopGold(GameConfig.GetGold(), false);
        foreach (Button bt in btBuyBoosters)
            bt.onClick.RemoveAllListeners();
        btClose.onClick.RemoveAllListeners();
        UpdateBoosterCount();
        iapRect.anchoredPosition = Vector2.zero;
        if (shopScroll)
        {
            shopScroll.StopMovement();
            shopScroll.verticalNormalizedPosition = 1f;
        }
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
