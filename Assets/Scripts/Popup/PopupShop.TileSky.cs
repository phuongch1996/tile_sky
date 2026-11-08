using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public partial class PopupShop
{
    [SerializeField] Sprite[] tileSkyShopBoosters;
    RectTransform shopArt;
    ScrollRect shopScroll;
    TextMeshProUGUI shopWallet;
    Image shopWalletCoin;
    TextMeshProUGUI shopFont;
    Material shopTextMaterial;
    Sprite shopRounded;
    Texture2D shopRoundedTexture;
    Sprite shopCoin;
    Sprite shopBuyButton;
    readonly List<Sprite> shopSlices = new();
    readonly Sprite[] shopBoosters = new Sprite[3];

    public void EnsureTileSkyShop()
    {
        if (shopArt) return;
        Sprite panelArt = Resources.Load<Sprite>("TileSkyShop/Panel");
        shopBuyButton = Resources.Load<Sprite>("TileSkyShop/BuyButton");
        Texture2D controls = Resources.Load<Texture2D>("HeavenlyFreeGold/Buttons");
        if (!panelArt || !shopBuyButton || !controls || tileSkyShopBoosters == null || tileSkyShopBoosters.Length < 3)
            return;
        for (int i = 0; i < 3; i++)
            if (!tileSkyShopBoosters[i]) return;
        shopFont = txValueGold1;
        shopTextMaterial = new Material(shopFont.fontSharedMaterial);
        shopTextMaterial.SetFloat("_OutlineWidth", 0.14f);
        shopTextMaterial.SetColor("_OutlineColor", new Color32(20, 43, 105, 255));
        CreateShopRounded();
        for (int i = 0; i < 3; i++)
            shopBoosters[i] = tileSkyShopBoosters[i];
        shopCoin = goldImgGold1.sprite;

        Transform safeArea = btClose.transform.parent;
        while (safeArea.parent && safeArea.name != "SafeAreaView" && safeArea != panel.transform)
            safeArea = safeArea.parent;
        shopArt = ShopRect("Tile Sky Shop", safeArea, new Vector2(1000, 1500), Vector2.zero);
        Image frame = shopArt.gameObject.AddComponent<Image>();
        frame.sprite = panelArt;
        frame.raycastTarget = true;
        if (bg)
        {
            bg.sprite = null;
            bg.color = new Color32(9, 25, 59, 245);
        }
        isBGChange = false;
        showTopBar = false;
        fadeIgnore.Clear();

        RectTransform wallet = ShopRect("Gold balance", shopArt, new Vector2(310, 67), new Vector2(0, 385));
        ShopImage(wallet, shopRounded, new Color32(14, 68, 157, 240));
        shopWalletCoin = ShopIcon("Balance coin", wallet, shopCoin, new Vector2(58, 58), Vector2.zero);
        shopWallet = ShopLabel("Balance", wallet, new Vector2(205, 55), new Vector2(30, 0), 38);
        UpdateShopGold(GameConfig.GetGold(), false);

        RectTransform scroll = ShopRect("Shop scroll", shopArt, new Vector2(850, 940), new Vector2(0, -115));
        shopScroll = scroll.gameObject.AddComponent<ScrollRect>();
        RectTransform viewport = ShopRect("Viewport", scroll, new Vector2(850, 940), Vector2.zero);
        ShopImage(viewport, null, Color.clear).raycastTarget = true;
        viewport.gameObject.AddComponent<RectMask2D>();
        iapRect = ShopRect("Shop offers", viewport, new Vector2(820, 1520), Vector2.zero);
        iapRect.anchorMin = iapRect.anchorMax = new Vector2(0.5f, 1f);
        iapRect.pivot = new Vector2(0.5f, 1f);
        shopScroll.viewport = viewport;
        shopScroll.content = iapRect;
        shopScroll.horizontal = false;
        shopScroll.vertical = true;
        shopScroll.movementType = ScrollRect.MovementType.Clamped;
        shopScroll.scrollSensitivity = 55f;
        AddShopScrollbar();

        ShopSection("BOOSTERS", -23);
        string[] boosterNames = { "UNDO", "HINT", "SHUFFLE" };
        for (int i = 0; i < 3; i++)
        {
            RectTransform card = ShopCard(boosterNames[i], new Vector2(250, 280), new Vector2((i - 1) * 270, -196));
            ShopLabel(boosterNames[i], card, new Vector2(230, 46), new Vector2(0, 105), 33);
            MoveShopText(txBoostersCount[i], card, new Vector2(230, 35), new Vector2(0, -49), 26);
            ShopPlace((RectTransform)boostersGO[i].transform, card, new Vector2(112, 112), new Vector2(0, 26));
            foreach (Transform child in boostersGO[i].transform) child.gameObject.SetActive(false);
            ShopImage((RectTransform)boostersGO[i].transform, shopBoosters[i], Color.white).preserveAspect = true;
            boostersGO[i].SetActive(true);
            MoveShopButton(btBuyBoosters[i], card, new Vector2(220, 65), new Vector2(0, -103), Color.white);
            btBuyBoosters[i].image.sprite = shopBuyButton;
            btBuyBoosters[i].image.type = Image.Type.Simple;
            btBuyBoosters[i].image.preserveAspect = true;
            var priceCoin = ShopIcon("Gold price", btBuyBoosters[i].transform, shopCoin, new Vector2(36, 36), Vector2.zero);
            PlaceShopGoldAmount(priceCoin, txPricesBooster[i], btBuyBoosters[i].transform,
                Vector2.zero, 36, 34, GameConfig.powerPrices[i], 180);
        }
        if (boostersGO.Length > 3) boostersGO[3].SetActive(false);
        if (btBuyBoosters.Length > 3) btBuyBoosters[3].gameObject.SetActive(false);

        ShopSection("GIFT PACKS", -384);
        BuildShopPack("NO ADS", IAPManager.tilespace_removeads, iapButtonRemoveAds, goldImgRemoveAds, txValueRemoveAds, new Vector2(-210, -588));
        BuildShopPack("STARTER PACK", IAPManager.tilespace_firstpack, iapButtonFirstPack, goldImgFirstPack, txValueFirstPack, new Vector2(210, -588));
        BuildShopPack("MEGA PACK", IAPManager.tilespace_megapack, iapButtonMegaPack, goldImgMegaPack, txValueMegaPack, new Vector2(-210, -938));
        BuildShopPack("ULTIMATE PACK", IAPManager.tilespace_ultipack, iapButtonUltiPack, goldImgUltiPack, txValueUltiPack, new Vector2(210, -938));
        ShopSection("GOLD", -1150);
        BuildShopGold(IAPManager.tilespace_gold1, iapButtonGold1, goldImgGold1, txValueGold1, -270);
        BuildShopGold(IAPManager.tilespace_gold2, iapButtonGold2, goldImgGold2, txValueGold2, 0);
        BuildShopGold(IAPManager.tilespace_gold3, iapButtonGold3, goldImgGold3, txValueGold3, 270);

        MoveShopButton(btClose, shopArt, new Vector2(105, 105), new Vector2(405, 600), Color.white);
        btClose.image.sprite = ShopSlice(controls, new Rect(402, 62, 446, 436));
        btClose.image.type = Image.Type.Simple;
        btClose.image.preserveAspect = true;
        ShopLabel("Swipe to explore", shopArt, new Vector2(450, 40), new Vector2(0, -620), 24);
        foreach (Transform child in safeArea)
            if (child != shopArt && (!bg || child != bg.transform)) child.gameObject.SetActive(false);
        box = shopArt;
        FitTileSkyShop();
    }

    void BuildShopPack(string title, string id, CodelessIAPButton purchase, Image coins, TextMeshProUGUI[] amounts, Vector2 position)
    {
        var values = IAPManager.GetIAPValues(id);
        RectTransform card = ShopCard(title, new Vector2(400, 330), position);
        ShopLabel(title, card, new Vector2(370, 52), new Vector2(0, 122), 36);
        if (values.removeAds) ShopLabel("Ad-free play", card, new Vector2(290, 35), new Vector2(0, 78), 25);
        PlaceShopGoldAmount(coins, amounts[0], card, new Vector2(-87, 0), 64, 42, values.goldAm, 190);
        var cluster = ShopRect("Booster reward cluster", card, new Vector2(132, 114), new Vector2(99, -4));
        Vector2[] boosterPositions = { new(-27, 26), new(27, 26), new(0, -24) };
        for (int i = 0; i < 3; i++)
            ShopIcon("Booster reward", cluster, shopBoosters[i], new Vector2(56, 56), boosterPositions[i]);
        var badge = ShopRect("Booster quantity", cluster, new Vector2(76, 40), new Vector2(45, -43));
        ShopImage(badge, shopRounded, new Color32(15, 63, 139, 245));
        MoveShopText(amounts[1], badge, new Vector2(72, 38), Vector2.zero, 30);
        amounts[1].text = "x" + values.boostersAm;
        BuildShopPurchase(purchase, id, card, new Vector2(360, 65), new Vector2(0, -118));
    }

    void BuildShopGold(string id, CodelessIAPButton purchase, Image coins, TextMeshProUGUI amount, float x)
    {
        RectTransform card = ShopCard("Gold pack", new Vector2(250, 280), new Vector2(x, -1344));
        PlaceShopGoldAmount(coins, amount, card, new Vector2(0, 25), 75, 41, IAPManager.GetIAPValues(id).goldAm, 220);
        BuildShopPurchase(purchase, id, card, new Vector2(225, 65), new Vector2(0, -103));
    }

    void BuildShopPurchase(CodelessIAPButton purchase, string id, RectTransform card, Vector2 size, Vector2 position)
    {
        purchase.productId = id;
        var price = purchase.GetComponentInChildren<TextMeshProUGUI>(true);
        var button = purchase.GetComponent<Button>();
        MoveShopButton(button, card, size, position, new Color32(70, 127, 195, 255));
        MoveShopText(price, button.transform, size - new Vector2(12, 5), Vector2.zero, 28);
        price.text = ShopPrice(id);
        if (!IAPManager.PurchasingEnabled)
        {
            purchase.enabled = false;
            button.interactable = false;
        }
    }

    static string ShopPrice(string id) => IAPManager.PurchasingEnabled ? GameConfig.GetLocalizedPrice(id) : "Coming soon";

    void PlaceShopGoldAmount(Image coin, TextMeshProUGUI amount, Transform parent,
        Vector2 center, float iconSize, float fontSize, int value, float maxWidth)
    {
        ShopPlace(coin.rectTransform, parent, Vector2.one * iconSize, Vector2.zero);
        coin.gameObject.SetActive(true);
        coin.enabled = true;
        coin.color = Color.white;
        coin.preserveAspect = true;
        coin.raycastTarget = false;
        MoveShopText(amount, parent, new Vector2(maxWidth, fontSize * 1.45f), Vector2.zero, fontSize);
        amount.text = value.ToString();
        AlignShopGoldAmount(coin, amount, center, iconSize, maxWidth);
    }

    static void AlignShopGoldAmount(Image coin, TextMeshProUGUI amount, Vector2 center, float iconSize, float maxWidth)
    {
        const float gap = 6f;
        float textWidth = amount.GetPreferredValues(amount.text).x + 2f;
        if (textWidth + iconSize + gap > maxWidth)
        {
            amount.fontSize *= Mathf.Clamp((maxWidth - iconSize - gap) / textWidth, 0.5f, 1f);
            textWidth = amount.GetPreferredValues(amount.text).x + 2f;
        }
        float totalWidth = iconSize + gap + textWidth;
        coin.rectTransform.anchoredPosition = center + Vector2.left * (totalWidth - iconSize) * 0.5f;
        amount.rectTransform.sizeDelta = new Vector2(textWidth, amount.fontSize * 1.45f);
        amount.rectTransform.anchoredPosition = center + Vector2.right * (iconSize + gap) * 0.5f;
    }

    void AddShopScrollbar()
    {
        var track = ShopRect("Scroll track", shopArt, new Vector2(9, 900), new Vector2(441, -115));
        ShopImage(track, shopRounded, new Color32(10, 57, 143, 180)).raycastTarget = true;
        var thumb = ShopRect("Scroll handle", track, Vector2.zero, Vector2.zero);
        thumb.anchorMin = Vector2.zero;
        thumb.anchorMax = Vector2.one;
        thumb.offsetMin = thumb.offsetMax = Vector2.zero;
        Image graphic = ShopImage(thumb, shopRounded, new Color32(182, 235, 255, 255));
        graphic.raycastTarget = true;
        var scrollbar = track.gameObject.AddComponent<Scrollbar>();
        scrollbar.handleRect = thumb;
        scrollbar.targetGraphic = graphic;
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        scrollbar.transition = Selectable.Transition.None;
        shopScroll.verticalScrollbar = scrollbar;
    }

    void ShopSection(string text, float y)
    {
        var label = ShopLabel(text, iapRect, new Vector2(800, 50), new Vector2(0, y), 38);
        label.rectTransform.anchorMin = label.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        label.color = new Color32(255, 232, 139, 255);
    }

    RectTransform ShopCard(string name, Vector2 size, Vector2 position)
    {
        var rim = ShopRect(name, iapRect, size, position);
        rim.anchorMin = rim.anchorMax = new Vector2(0.5f, 1f);
        ShopImage(rim, shopRounded, new Color32(255, 202, 80, 255));
        var inset = ShopRect("Sky card", rim, size - new Vector2(7, 7), Vector2.zero);
        ShopImage(inset, shopRounded, new Color32(39, 124, 219, 255));
        var highlight = ShopRect("Card highlight", rim, new Vector2(size.x - 20, 6), new Vector2(0, size.y * 0.5f - 9));
        ShopImage(highlight, shopRounded, new Color32(194, 239, 255, 150));
        return rim;
    }

    void MoveShopButton(Button button, Transform parent, Vector2 size, Vector2 position, Color tint)
    {
        ShopPlace((RectTransform)button.transform, parent, size, position);
        foreach (Transform child in button.transform) child.gameObject.SetActive(false);
        var image = button.GetComponent<Image>();
        if (!image) image = button.gameObject.AddComponent<Image>();
        image.sprite = shopRounded;
        image.type = Image.Type.Sliced;
        image.color = tint;
        image.enabled = true;
        image.raycastTarget = true;
        button.gameObject.SetActive(true);
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        var colors = button.colors;
        colors.disabledColor = new Color(0.85f, 0.9f, 1f, 1f);
        colors.pressedColor = new Color(0.8f, 0.9f, 1f, 1f);
        button.colors = colors;
    }

    void MoveShopText(TextMeshProUGUI label, Transform parent, Vector2 size, Vector2 position, float fontSize)
    {
        ShopPlace(label.rectTransform, parent, size, position);
        label.gameObject.SetActive(true);
        label.fontSharedMaterial = shopTextMaterial;
        label.fontSize = fontSize;
        label.enableAutoSizing = false;
        label.color = Color.white;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Truncate;
        label.raycastTarget = false;
        label.margin = Vector4.zero;
    }

    TextMeshProUGUI ShopLabel(string text, Transform parent, Vector2 size, Vector2 position, float fontSize)
    {
        var label = Instantiate(shopFont, parent, false);
        label.name = text;
        MoveShopText(label, parent, size, position, fontSize);
        label.text = text;
        return label;
    }

    Image ShopIcon(string name, Transform parent, Sprite sprite, Vector2 size, Vector2 position)
    {
        Image image = ShopImage(ShopRect(name, parent, size, position), sprite, Color.white);
        image.type = Image.Type.Simple;
        image.preserveAspect = true;
        return image;
    }

    static Image ShopImage(RectTransform rect, Sprite sprite, Color tint)
    {
        Image image = rect.GetComponent<Image>();
        if (!image) image = rect.gameObject.AddComponent<Image>();
        image.enabled = true;
        image.sprite = sprite;
        image.color = tint;
        image.type = sprite && sprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
        image.raycastTarget = false;
        return image;
    }

    static RectTransform ShopRect(string name, Transform parent, Vector2 size, Vector2 position)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        ShopPlace(rect, parent, size, position);
        rect.gameObject.layer = parent.gameObject.layer;
        return rect;
    }

    static void ShopPlace(RectTransform rect, Transform parent, Vector2 size, Vector2 position)
    {
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one * 0.5f;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    void CreateShopRounded()
    {
        shopRoundedTexture = new Texture2D(64, 64, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        var pixels = new Color[64 * 64];
        for (int y = 0; y < 64; y++)
        for (int x = 0; x < 64; x++)
        {
            Vector2 corner = new(Mathf.Max(0, Mathf.Abs(x - 31.5f) - 15f), Mathf.Max(0, Mathf.Abs(y - 31.5f) - 15f));
            pixels[y * 64 + x] = new Color(1, 1, 1, Mathf.Clamp01(16f - corner.magnitude));
        }
        shopRoundedTexture.SetPixels(pixels);
        shopRoundedTexture.Apply(false, true);
        shopRounded = Sprite.Create(shopRoundedTexture, new Rect(0, 0, 64, 64), Vector2.one * 0.5f, 100, 0,
            SpriteMeshType.FullRect, new Vector4(20, 20, 20, 20));
    }

    Sprite ShopSlice(Texture2D texture, Rect region)
    {
        Sprite sprite = Sprite.Create(texture, region, Vector2.one * 0.5f, 100, 0, SpriteMeshType.FullRect);
        shopSlices.Add(sprite);
        return sprite;
    }

    void UpdateShopGold(int amount, bool added)
    {
        if (!shopWallet) return;
        shopWallet.text = amount.ToString("N0");
        shopWallet.fontSize = 38;
        AlignShopGoldAmount(shopWalletCoin, shopWallet, Vector2.zero, 58, 285);
    }

    void LateUpdate() => FitTileSkyShop();

    void FitTileSkyShop()
    {
        if (!shopArt) return;
        var parent = (RectTransform)shopArt.parent;
        float scale = Mathf.Min(parent.rect.width / 1000f, parent.rect.height / 1500f) * 0.96f;
        shopArt.localScale = Vector3.one * Mathf.Max(0.01f, scale);
    }

    void OnDestroy()
    {
        foreach (var sprite in shopSlices) ReleaseShopAsset(sprite);
        ReleaseShopAsset(shopRounded);
        ReleaseShopAsset(shopRoundedTexture);
        ReleaseShopAsset(shopTextMaterial);
    }

    static void ReleaseShopAsset(Object asset)
    {
        if (!asset) return;
        if (Application.isPlaying) Destroy(asset);
        else DestroyImmediate(asset);
    }
}
