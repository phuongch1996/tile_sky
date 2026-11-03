using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class PopupBattlePass : Popup
{
    public BattlePassCard bpCardTemplate;
    public Transform bpCardParent;
    public List<BattlePassCard> bpCardList;
    public Image bpFill;
    //public Slider bpSlider;
    public Button btClose;
    public Slider progressSlider;
    public TextMeshProUGUI txProgress;
    public TextMeshProUGUI txProgressGoal;
    public TextMeshProUGUI txTimer;
    public CodelessIAPButton iapButtonBattlePass;
    public TextMeshProUGUI txBattlePassPrice;
    RectTransform heavenlyHeader;
    RectTransform heavenlyFrame;
    RectTransform heavenlyControls;
    RectTransform rewardScroll;
    TextMeshProUGUI headerGold;
    RectTransform headerGoldRect;
    RectTransform homeGoldRect;
    Vector2 headerParentSize;
    static Sprite roundedHeaderSprite;
    static Sprite heavenlyBackground;

    void EnsureHeavenlyBackground()
    {
        if (!bg) return;
        if (!heavenlyBackground)
        {
            // The full-screen sky is independent of the foreground frame and art.
            var texture = new Texture2D(1, 256, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < 256; y++)
                texture.SetPixel(0, y, Color.Lerp(new Color32(190, 233, 255, 255),
                    new Color32(0, 136, 249, 255), y / 255f));
            texture.Apply(false, true);
            heavenlyBackground = Sprite.Create(texture, new Rect(0, 0, 1, 256), Vector2.one * 0.5f);
            heavenlyBackground.name = "Battle Pass full screen background";
        }
        isBGChange = false;
        foreach (var canvasBackground in new[] {
            HomeScript.instance ? HomeScript.instance.canvasBG : null,
            GameScript.instance ? GameScript.instance.canvasBG : null })
        {
            if (!canvasBackground) continue;
            canvasBackground.bgImages?.Remove(bg);
            canvasBackground.images?.Remove(bg);
        }
        bg.rectTransform.SetParent(panel.transform, false);
        bg.rectTransform.SetAsFirstSibling();
        bg.sprite = heavenlyBackground;
        bg.type = Image.Type.Simple;
        bg.preserveAspect = false;
        bg.fillCenter = true;
        bg.color = Color.white;
        bg.enabled = true;
        FitHeavenlyBackground();
    }

    void FitHeavenlyBackground()
    {
        if (!bg || !heavenlyBackground || bg.sprite != heavenlyBackground) return;
        var rect = bg.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = Vector2.one * 0.5f;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        bg.pixelsPerUnitMultiplier = 1;
    }

    public void EnsureHeavenlyHeader(TopBarScript topBarOverride = null)
    {
        EnsureHeavenlyBackground();
        if (heavenlyHeader) return;
        var artwork = Resources.Load<Sprite>("HeavenlyBattlePass/HeaderCutout");
        var back = Resources.Load<Sprite>("HeavenlyBattlePass/Back");
        var activate = Resources.Load<Sprite>("HeavenlyBattlePass/Activate");
        var topBar = topBarOverride ? topBarOverride : HomeScript.instance ? HomeScript.instance.topBarScript :
            (GameScript.instance ? GameScript.instance.topBarScript : null);
        if (!artwork || !back || !activate || !topBar) return;

        var overlay = btClose.transform.parent;
        var oldHeader = progressSlider.transform.parent.gameObject;
        var oldTimer = txTimer.transform.parent.gameObject;
        // Open builds the header before base.Open activates the popup hierarchy.
        var scroll = bpCardParent.GetComponentInParent<ScrollRect>(true);
        if (!scroll) return;
        rewardScroll = scroll.GetComponent<RectTransform>();
        float headerHeight = 1440f * artwork.rect.height / artwork.rect.width;
        heavenlyHeader = HeaderRect("Heavenly Battle Pass", overlay, new Vector2(1440, headerHeight), Vector2.zero);
        heavenlyHeader.anchorMin = heavenlyHeader.anchorMax = new Vector2(0.5f, 1);
        heavenlyHeader.pivot = new Vector2(0.5f, 1);
        var headerArt = heavenlyHeader.gameObject.AddComponent<Image>();
        headerArt.sprite = artwork;
        headerArt.preserveAspect = true;
        headerArt.raycastTarget = false;

        heavenlyFrame = HeaderRect("Battle Pass reward frame", overlay, Vector2.zero, Vector2.zero);
        AddHeaderPill(heavenlyFrame, new Color32(186, 250, 255, 255));
        var frameInset = HeaderRect("Blue frame interior", heavenlyFrame, Vector2.zero, Vector2.zero);
        frameInset.anchorMin = Vector2.zero;
        frameInset.anchorMax = Vector2.one;
        frameInset.offsetMin = Vector2.one * 7;
        frameInset.offsetMax = -Vector2.one * 7;
        AddHeaderPill(frameInset, new Color32(27, 151, 239, 255));
        heavenlyControls = HeaderRect("Battle Pass progress and timer", heavenlyFrame, new Vector2(1440, 280), Vector2.zero);
        heavenlyControls.anchorMin = heavenlyControls.anchorMax = new Vector2(0.5f, 1);
        heavenlyControls.pivot = new Vector2(0.5f, 1);

        // Remove the old viewport overflow, which could cover the controls above it.
        rewardScroll.SetParent(heavenlyFrame, false);
        var scrollImage = scroll.GetComponent<Image>();
        if (scrollImage) scrollImage.color = Color.clear;
        scroll.viewport.anchorMin = Vector2.zero;
        scroll.viewport.anchorMax = Vector2.one;
        scroll.viewport.offsetMin = scroll.viewport.offsetMax = Vector2.zero;
        var mask = scroll.viewport.GetComponent<Mask>();
        if (mask) mask.showMaskGraphic = false;
        scroll.horizontal = false;
        if (scroll.horizontalScrollbar) scroll.horizontalScrollbar.gameObject.SetActive(false);
        if (scroll.verticalScrollbar) scroll.verticalScrollbar.gameObject.SetActive(false);
        scroll.horizontalScrollbar = scroll.verticalScrollbar = null;
        var rewardLayout = bpCardParent.GetComponent<VerticalLayoutGroup>();
        if (rewardLayout)
        {
            rewardLayout.padding.top = 12;
            rewardLayout.padding.bottom = 40;
        }

        // Match GameScene's bthome in safe-area coordinates, independently of header scaling.
        btClose.transform.SetParent(overlay, false);
        PlaceHeaderRect((RectTransform)btClose.transform, new Vector2(125, 150), new Vector2(110, 100));
        btClose.transform.SetAsLastSibling();
        foreach (Transform child in btClose.transform) child.gameObject.SetActive(false);
        btClose.image.sprite = back;
        btClose.image.color = Color.white;
        btClose.image.enabled = true;
        btClose.image.type = Image.Type.Simple;
        btClose.image.preserveAspect = true;

        // Clone Home's actual counter so both its frame and coin use the same art.
        var counter = Instantiate(topBar.goldsCounter, overlay, false);
        counter.name = "Home gold counter";
        counter.SetActive(true);
        homeGoldRect = (RectTransform)topBar.goldsCounter.transform;
        headerGoldRect = (RectTransform)counter.transform;
        AlignHomeGoldCounter();
        foreach (var button in counter.GetComponentsInChildren<Button>(true))
            button.gameObject.SetActive(false);
        foreach (var graphic in counter.GetComponentsInChildren<Graphic>(true))
            graphic.raycastTarget = false;
        headerGold = counter.GetComponentInChildren<TextMeshProUGUI>(true);
        if (headerGold) headerGold.text = GameConfig.GetGold().ToString();
        // The popup supplies its own copy; keep the external TopBar below this popup.
        showTopBar = false;

        var bar = HeaderRect("Progress", heavenlyControls, new Vector2(670, 84), new Vector2(460, 105));
        AddHeaderPill(bar, new Color32(202, 250, 255, 255));
        var track = HeaderRect("Blue track", bar, new Vector2(656, 70), new Vector2(335, 42));
        AddHeaderPill(track, new Color32(0, 65, 161, 255));
        var fillArea = HeaderRect("Fill area", track, new Vector2(636, 48), new Vector2(328, 35));
        var fill = HeaderRect("Green fill", fillArea, new Vector2(636, 48), new Vector2(318, 24));
        AddHeaderPill(fill, new Color32(101, 245, 15, 255));
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = Vector2.one;
        fill.offsetMin = fill.offsetMax = Vector2.zero;
        progressSlider = bar.gameObject.AddComponent<Slider>();
        progressSlider.fillRect = fill;
        progressSlider.direction = Slider.Direction.LeftToRight;
        progressSlider.interactable = false;
        progressSlider.transition = Selectable.Transition.None;
        txProgress.transform.SetParent(heavenlyControls, false);
        PlaceHeaderRect(txProgress.rectTransform, new Vector2(580, 95), new Vector2(460, 105));
        StyleHeaderText(txProgress, 66, new Color32(0, 45, 131, 255));

        var diamond = HeaderRect("Goal diamond", heavenlyControls, new Vector2(106, 106), new Vector2(800, 105));
        diamond.localRotation = Quaternion.Euler(0, 0, 45);
        diamond.gameObject.AddComponent<Image>().color = new Color32(255, 219, 38, 255);
        var green = HeaderRect("Green inset", diamond, new Vector2(84, 84), new Vector2(53, 53));
        green.gameObject.AddComponent<Image>().color = new Color32(94, 226, 28, 255);
        foreach (var graphic in diamond.GetComponentsInChildren<Graphic>()) graphic.raycastTarget = false;
        txProgressGoal.transform.SetParent(heavenlyControls, false);
        PlaceHeaderRect(txProgressGoal.rectTransform, new Vector2(110, 100), new Vector2(800, 105));
        StyleHeaderText(txProgressGoal, 62, new Color32(0, 45, 131, 255));

        var purchaseRect = (RectTransform)iapButtonBattlePass.transform;
        purchaseRect.SetParent(heavenlyControls, false);
        PlaceHeaderRect(purchaseRect, new Vector2(460, 180), new Vector2(1165, 105));
        txBattlePassPrice.transform.SetParent(purchaseRect, false);
        foreach (Transform child in purchaseRect)
            if (child != txBattlePassPrice.transform) child.gameObject.SetActive(false);
        var purchaseImage = iapButtonBattlePass.button.image;
        purchaseImage.sprite = activate;
        purchaseImage.type = Image.Type.Simple;
        purchaseImage.color = Color.white;
        purchaseImage.preserveAspect = true;
        PlaceHeaderRect(txBattlePassPrice.rectTransform, new Vector2(380, 60), new Vector2(230, 113));
        StyleHeaderText(txBattlePassPrice, 58, new Color32(102, 30, 7, 255));
        var activateLabel = Instantiate(txBattlePassPrice, purchaseRect, false);
        activateLabel.name = "Activate label";
        activateLabel.text = "Activate";
        PlaceHeaderRect(activateLabel.rectTransform, new Vector2(380, 60), new Vector2(230, 57));
        StyleHeaderText(activateLabel, 54, new Color32(102, 30, 7, 255));

        txTimer.transform.SetParent(heavenlyControls, false);
        PlaceHeaderRect(txTimer.rectTransform, new Vector2(850, 65), new Vector2(720, 235));
        StyleHeaderText(txTimer, 42, new Color32(0, 65, 161, 255));
        oldHeader.SetActive(false);
        oldTimer.SetActive(false);
        UpdateHeavenlyHeader();
    }

    static RectTransform HeaderRect(string objectName, Transform parent, Vector2 size, Vector2 position)
    {
        var rect = new GameObject(objectName, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.gameObject.layer = parent.gameObject.layer;
        rect.SetParent(parent, false);
        PlaceHeaderRect(rect, size, position);
        return rect;
    }

    static void PlaceHeaderRect(RectTransform rect, Vector2 size, Vector2 position)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.sizeDelta = size;
        rect.anchoredPosition = new Vector2(position.x, -position.y);
    }

    static void StyleHeaderText(TextMeshProUGUI text, float fontSize, Color outline)
    {
        text.color = Color.white;
        text.fontSize = text.fontSizeMax = fontSize;
        text.fontSizeMin = fontSize * 0.55f;
        text.enableAutoSizing = true;
        text.alignment = TextAlignmentOptions.Center;
        text.outlineColor = outline;
        text.outlineWidth = 0.2f;
        text.raycastTarget = false;
    }

    static void AddHeaderPill(RectTransform rect, Color color)
    {
        if (!roundedHeaderSprite)
        {
            var texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            texture.name = "Battle Pass rounded UI mask";
            texture.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color[32 * 32];
            for (int y = 0; y < 32; y++)
                for (int x = 0; x < 32; x++)
                    pixels[y * 32 + x] = new Color(1, 1, 1,
                        Mathf.Clamp01(16f - Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f))));
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            roundedHeaderSprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), Vector2.one * 0.5f,
                100, 0, SpriteMeshType.FullRect, new Vector4(15, 15, 15, 15));
        }
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = roundedHeaderSprite;
        image.type = Image.Type.Sliced;
        image.color = color;
        image.raycastTarget = false;
    }

    void UpdateHeavenlyHeader()
    {
        if (!heavenlyHeader) return;
        if (headerGold) headerGold.SetText("{0}", GameConfig.GetGold());
        var parentSize = ((RectTransform)heavenlyHeader.parent).rect.size;
        if (parentSize == headerParentSize) return;
        headerParentSize = parentSize;
        if (parentSize.x <= 0 || parentSize.y <= 0) return;
        float headerHeight = heavenlyHeader.sizeDelta.y;
        float scale = Mathf.Min(parentSize.x / 1440f, parentSize.y * 0.38f / headerHeight);
        heavenlyHeader.localScale = Vector3.one * scale;
        heavenlyHeader.anchoredPosition = Vector2.zero;
        float frameTop = headerHeight * scale + 12f;
        heavenlyFrame.anchorMin = Vector2.zero;
        heavenlyFrame.anchorMax = Vector2.one;
        heavenlyFrame.offsetMin = new Vector2(24, 24);
        heavenlyFrame.offsetMax = new Vector2(-24, -frameTop);
        float controlsScale = Mathf.Max(0.01f, (parentSize.x - 48f) / 1440f);
        heavenlyControls.localScale = Vector3.one * controlsScale;
        heavenlyControls.anchoredPosition = new Vector2(0, -12);
        rewardScroll.anchorMin = Vector2.zero;
        rewardScroll.anchorMax = Vector2.one;
        rewardScroll.offsetMin = new Vector2(18, 18);
        rewardScroll.offsetMax = new Vector2(-18, -280f * controlsScale - 28f);
    }

    void LateUpdate()
    {
        FitHeavenlyBackground();
        UpdateHeavenlyHeader();
        // SafeArea and Canvas layout can update after Open or when the screen size changes.
        if (heavenlyHeader) AlignHomeGoldCounter();
    }

    void AlignHomeGoldCounter()
    {
        if (!homeGoldRect || !headerGoldRect) return;
        var parent = (RectTransform)headerGoldRect.parent;
        var sourceCanvas = homeGoldRect.GetComponentInParent<Canvas>(true);
        var targetCanvas = parent.GetComponentInParent<Canvas>(true);
        if (sourceCanvas) sourceCanvas = sourceCanvas.rootCanvas;
        if (targetCanvas) targetCanvas = targetCanvas.rootCanvas;
        var sourceCamera = sourceCanvas && sourceCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? sourceCanvas.worldCamera : null;
        var targetCamera = targetCanvas && targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? targetCanvas.worldCamera : null;
        // Convert the complete counter bounds so the frame, icon and text retain Home's size.
        var lowerLeftScreen = RectTransformUtility.WorldToScreenPoint(sourceCamera,
            homeGoldRect.TransformPoint(homeGoldRect.rect.min));
        var upperRightScreen = RectTransformUtility.WorldToScreenPoint(sourceCamera,
            homeGoldRect.TransformPoint(homeGoldRect.rect.max));
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, lowerLeftScreen, targetCamera, out var lowerLeft) ||
            !RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, upperRightScreen, targetCamera, out var upperRight))
            return;
        var size = upperRight - lowerLeft;
        var sourceSize = homeGoldRect.rect.size;
        if (sourceSize.x <= 0 || sourceSize.y <= 0) return;
        headerGoldRect.anchorMin = headerGoldRect.anchorMax = Vector2.one * 0.5f;
        headerGoldRect.pivot = homeGoldRect.pivot;
        headerGoldRect.sizeDelta = sourceSize;
        headerGoldRect.localRotation = Quaternion.identity;
        headerGoldRect.localScale = new Vector3(size.x / sourceSize.x, size.y / sourceSize.y, 1);
        headerGoldRect.localPosition = lowerLeft + Vector2.Scale(size, homeGoldRect.pivot);
    }

    bool isInit;
    private void Awake0()
    {
        iapButtonBattlePass.productId = IAPManager.tilespace_battlepass;
        iapButtonBattlePass.button.onClick.AddListener(() => IAPManager.instance.OnIAPClick());
        txBattlePassPrice.text = GameConfig.GetLocalizedPrice(IAPManager.tilespace_battlepass);
    }
    public override void SetupTweenCallback()
    {
        SetupTweenCallback(() =>
        {
            btClose.onClick.AddListener(() => Close());
            for (int i = 0; i < bpCardList.Count; i++)
                SetupButton(bpCardList[i], i);
        });
    }
    public override void Open()
    {
        EnsureHeavenlyHeader();
        btClose.onClick.RemoveAllListeners();
        foreach (var card in bpCardList)
        {
            card.btFreeClaim.onClick.RemoveAllListeners();
            card.btAdsClaim.onClick.RemoveAllListeners();
            card.btVipClaim.onClick.RemoveAllListeners();
        }
        if (!isInit)
            SetupBPCards();
        BPProgress();
        base.Open();
    }
    //int tfmIndex;
    void Tfm(int tfmIndex)
    {
        var content = (RectTransform)bpCardParent;
        var scroll = bpCardParent.GetComponentInParent<ScrollRect>(true);
        if (!scroll || bpCardList.Count == 0) return;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        var currentCard = (RectTransform)bpCardList[Mathf.Clamp(tfmIndex - 1, 0, bpCardList.Count - 1)].transform;
        float center = -currentCard.anchoredPosition.y;
        float maxOffset = Mathf.Max(0, content.rect.height - scroll.viewport.rect.height);
        float offset = Mathf.Clamp(center - scroll.viewport.rect.height * 0.5f, 0, maxOffset);
        scroll.StopMovement();
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, offset);
    }
    private void Update()
    {
        UpdateHeavenlyHeader();
        UpdateTimerText();
    }
    private void UpdateTimerText()
    {
        if (!BattlePassManager.instance || !TimeManager.instance) return;
        TimeSpan ts = BattlePassManager.instance.TimeLeftBattlePass();
        if (ts < TimeSpan.Zero) ts = TimeSpan.Zero;
        if (ts.TotalDays >= 1)
            txTimer.text = $"ENDS IN {Mathf.Floor((float)ts.TotalDays)}d {ts:%h}h {ts:%m}m";
        else
            txTimer.text = $"ENDS IN {ts:%h}h {ts:%m}m";
    }
    void SetupBPCards()
    {
        Awake0();
        bpCardList = new();
        for (int i = 0; i < BattlePassManager.battlePass.Count; i++)
        {
            BattlePassCard card = i == 0 ? bpCardTemplate : Instantiate(bpCardTemplate, bpCardParent);
            card.SetupCard(BattlePassManager.battlePass[i]);
            bpCardList.Add(card);
        }
        isInit = true;
    }
    public void BPProgress()
    {
        iapButtonBattlePass.gameObject.SetActive(!BattlePassManager.IsBattlePassVIP());
        var bpData = BattlePassManager.instance.bpData;
        int winCount = BattlePassManager.GetBPWinCount();
        var (index, winFill) = BattlePassManager.GetBPWinFill(winCount);
        int tfmIndex = index;
        Debug.Log($"index{index} winFill{winFill}");
        bpFill.fillAmount = (index - 1 + winFill) / (bpCardList.Count - 1);
        //bpSlider.minValue = 0;
        //bpSlider.maxValue = bpCardList.Count - 1;
        //bpSlider.value = index - 1 + winFill;
        int fillMax = BattlePassManager.battlePass[index < BattlePassManager.battlePass.Count ? index : BattlePassManager.battlePass.Count - 1].winAmount;
        txProgress.text = $"{winCount}/{fillMax}";
        txProgressGoal.text = $"{fillMax}";
        progressSlider.minValue = 0;
        progressSlider.maxValue = fillMax;
        progressSlider.value = winCount;
        for (int i = 0; i < bpCardList.Count; i++)
        {
            if (BattlePassManager.battlePass[i].winAmount <= winCount)
            {
                bpCardList[i].notAvail.SetActive(false);
                if (bpData.freeClaimed.Contains(i))
                    bpCardList[i].SetupFree(2);
                else
                    bpCardList[i].SetupFree(1);

                if (bpData.adsClaimed.Contains(i))
                    bpCardList[i].SetupAds(2);
                else
                    bpCardList[i].SetupAds(1);

                if (bpData.vipClaimed.Contains(i))
                    bpCardList[i].SetupVip(2);
                else
                    bpCardList[i].SetupVip(1);
            }
            else
            {
                bpCardList[i].notAvail.SetActive(true);
                bpCardList[i].SetupFree(0);
                bpCardList[i].SetupAds(0);
                bpCardList[i].SetupVip(0);
            }
            bpCardList[i].numCurrentGO.SetActive(index > 0 ? (i == index - 1) : i == 0);
        }
        Tfm(tfmIndex);
    }
    void SetupButton(BattlePassCard bpCard, int index)
    {
        bpCard.btFreeClaim.onClick.AddListener(() =>
        {
            BattlePassManager.instance.bpData.ClaimFree(index);
            BattlePassManager.instance.UpdateData();
            ClaimPrize(bpCard.freePrizeGO, BattlePassManager.battlePass[index].bpPrizes[0]);
        });
        bpCard.btAdsClaim.onClick.AddListener(() =>
        {
            adsIndex = index;
            GameConfig.ShowRewardedAd(8);
        });
        bpCard.btVipClaim.onClick.AddListener(() =>
        {
            BattlePassManager.instance.bpData.ClaimVip(index);
            BattlePassManager.instance.UpdateData();
            ClaimPrize(bpCard.vipPrizeGO, BattlePassManager.battlePass[index].bpPrizes[2]);
        });
    }
    int adsIndex;
    public void AdsClaim()
    {
        BattlePassManager.instance.bpData.ClaimAds(adsIndex);
        BattlePassManager.instance.UpdateData();
        ClaimPrize(bpCardList[adsIndex].adsPrizeGO, BattlePassManager.battlePass[adsIndex].bpPrizes[1]);
    }
    public void ClaimPrize(GameObject prizeGO, BPPrizes bpPrizes)
    {
        if (bpPrizes.prizeTypes.Contains(4))
            GameConfig.AddGold(bpPrizes.prizeAmount);
        else
            GameConfig.AddPowerUps(bpPrizes.prizeAmount, bpPrizes.prizeTypes);
        BPProgress();
        var go = Instantiate(prizeGO, prizeGO.transform.parent);
        //go.transform.position = prizeGO.transform.position;
        go.transform.DOBlendableLocalMoveBy(new Vector3(0, 100), 1).OnStart(() =>
        {
            MaskableGraphic[] graphics = go.GetComponentsInChildren<MaskableGraphic>();
            foreach (var graphic in graphics)
            {
                graphic.DOFade(0, 1 - Time.deltaTime);
            }
        }).OnComplete(() => { Destroy(go); });
    }
}
