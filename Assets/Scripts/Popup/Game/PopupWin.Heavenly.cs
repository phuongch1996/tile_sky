using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class PopupWin
{
    RectTransform heavenlyWin;
    Image levelGoldIcon;
    RectTransform levelGoldReward;
    RectTransform progressGift;
    Sequence goldRewardFlight;
    Sequence giftIdle;
    RectTransform victoryHeader;
    RectTransform victoryEffects;
    readonly List<RectTransform> victoryConfetti = new();
    Sequence victoryEntrance;
    Sprite victoryGlowSprite;
    Texture2D victoryGlowTexture;
    Image victoryGlow;
    int pendingGoldAmount;
    readonly List<Sprite> winSprites = new();
    readonly List<Material> winTextMaterials = new();

    void EnsureHeavenlyWin()
    {
        if (heavenlyWin) return;
        Sprite art = Resources.Load<Sprite>("HeavenlyWin/Header");
        Texture2D controls = Resources.Load<Texture2D>("HeavenlyWin/Buttons");
        if (!art || !controls || !win1GO || !win2GO) return;

        Transform safeArea = win1GO.transform.parent.parent;
        foreach (Transform child in safeArea) child.gameObject.SetActive(false);
        heavenlyWin = WinRect("Heavenly victory", safeArea, new Vector2(960, 1440), Vector2.zero);
        var header = WinRect("Victory header", heavenlyWin, new Vector2(900, 580), new Vector2(0, 410));
        victoryHeader = header;
        var headerImage = header.gameObject.AddComponent<Image>();
        headerImage.sprite = art;
        headerImage.preserveAspect = true;
        headerImage.raycastTarget = false;

        win1GO.transform.SetParent(heavenlyWin, false);
        WinPlace((RectTransform)win1GO.transform, new Vector2(960, 1440), Vector2.zero);
        win2GO.transform.SetParent(heavenlyWin, false);
        WinPlace((RectTransform)win2GO.transform, new Vector2(960, 1440), Vector2.zero);

        // Reveal the gift and progress with the action buttons in Win2.
        progressGO.transform.SetParent(win2GO.transform, false);
        WinPlace((RectTransform)progressGO.transform, new Vector2(170, 210), new Vector2(385, -320));
        var progressBar = (RectTransform)firstSegment.transform.parent;
        progressBar.gameObject.SetActive(false);
        WinPlace(txProgress.rectTransform, new Vector2(170, 60), new Vector2(0, -100));
        WinText(txProgress, 48, new Color32(40, 55, 125, 255));
        StyleHeavenlyGift(progressBar);

        WinButton(btNext, WinButtonSlice(controls, true), new Vector2(580, 205), new Vector2(0, -350));
        WinButton(btHome, WinButtonSlice(controls, false), new Vector2(580, 205), new Vector2(0, -575));
        txNext.gameObject.SetActive(true);
        WinPlace(txNext.rectTransform, new Vector2(520, 150), new Vector2(0, 7));
        WinText(txNext, 80, new Color32(23, 79, 12, 255));

        // Retain the video symbol and show the rolled reward as an icon and quantity.
        var videoIcon = btHome.transform.Find("Image_1");
        if (videoIcon)
        {
            videoIcon.gameObject.SetActive(true);
            WinPlace((RectTransform)videoIcon, new Vector2(115, 105), new Vector2(-185, 5));
        }
        adRewardIcon.gameObject.SetActive(true);
        WinPlace(adRewardIcon.rectTransform, new Vector2(117, 117), new Vector2(-10, 6));
        adRewardIcon.preserveAspect = true;
        txAdReward.gameObject.SetActive(true);
        WinPlace(txAdReward.rectTransform, new Vector2(150, 120), new Vector2(140, 7));
        WinText(txAdReward, 76, new Color32(141, 44, 6, 255));
        txAdReward.alignment = TextAlignmentOptions.Left;

        levelGoldReward = WinRect("Level gold reward", heavenlyWin, new Vector2(340, 150), new Vector2(0, -35));
        var goldIconRect = WinRect("Level gold icon", levelGoldReward, new Vector2(110, 110), new Vector2(-85, 0));
        levelGoldIcon = goldIconRect.gameObject.AddComponent<Image>();
        levelGoldIcon.sprite = streakGoldImg.sprite;
        levelGoldIcon.preserveAspect = true;
        levelGoldIcon.raycastTarget = false;
        txCoinsGain.transform.SetParent(levelGoldReward, false);
        // The original amount label has its own coin decoration; use the single icon above.
        foreach (Transform decoration in txCoinsGain.transform) decoration.gameObject.SetActive(false);
        WinPlace(txCoinsGain.rectTransform, new Vector2(180, 100), new Vector2(65, 0));
        WinText(txCoinsGain, 76, new Color32(111, 62, 4, 255));
        if (bg) bg.gameObject.SetActive(false);
        isBGChange = false;
        var shade = panel.GetComponent<Image>();
        if (shade) shade.color = new Color(0, 0, 0, 0.55f);
        FitHeavenlyWin();
    }

    Sprite WinButtonSlice(Texture2D texture, bool green)
    {
        // Coordinates expressed as fractions of the generated two-row sprite atlas.
        var rect = new Rect(texture.width * 0.11f, texture.height * (green ? 0.51f : 0.01f),
            texture.width * 0.78f, texture.height * 0.48f);
        var sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect);
        winSprites.Add(sprite);
        return sprite;
    }

    void StyleHeavenlyGift(RectTransform bar)
    {
        Texture2D atlas = Resources.Load<Texture2D>("HeavenlyWin/Progress");
        if (!atlas) return;
        Sprite gift = WinProgressSlice(atlas, new Rect(402, 480, 660, 600));
        foreach (Transform child in progressGO.transform)
        {
            if (child == bar || child == txProgress.transform) continue;
            foreach (Transform oldArt in child) oldArt.gameObject.SetActive(false);
            var image = child.GetComponent<Image>();
            if (!image) image = child.gameObject.AddComponent<Image>();
            image.sprite = gift;
            image.type = Image.Type.Simple;
            image.color = Color.white;
            image.preserveAspect = true;
            image.raycastTarget = false;
            WinPlace((RectTransform)child, new Vector2(150, 140), Vector2.zero);
            progressGift = (RectTransform)child;
        }
    }

    void ShowGoldReward(int amount)
    {
        if (!levelGoldReward) return;
        WinPlace(levelGoldReward, new Vector2(340, 150), new Vector2(0, -35));
        levelGoldReward.gameObject.SetActive(true);
        txCoinsGain.gameObject.SetActive(true);
        txCoinsGain.text = amount.ToString();
        levelGoldIcon.sprite = UnityAuthentication.instance.GetGoldSprite(amount);
    }

    void PlayGoldReward(int amount, Action onCollected = null)
    {
        goldRewardFlight?.Kill();
        SettleGoldReward();
        ShowGoldReward(amount);
        pendingGoldAmount = amount;
        var counter = GameScript.instance.topBarScript.txGolds;
        goldRewardFlight = DOTween.Sequence()
            .AppendInterval(0.65f)
            .Append(levelGoldReward.DOMove(counter.transform.position, 0.75f).SetEase(Ease.InQuad))
            .Join(levelGoldReward.DOScale(0.3f, 0.75f).SetEase(Ease.InQuad))
            .OnComplete(() =>
            {
                SettleGoldReward();
                levelGoldReward.gameObject.SetActive(false);
                onCollected?.Invoke();
            });
    }

    void SettleGoldReward()
    {
        if (pendingGoldAmount <= 0) return;
        int amount = pendingGoldAmount;
        pendingGoldAmount = 0;
        GameConfig.AddGold(amount);
    }

    void EnsureVictoryEffects()
    {
        if (victoryEffects) return;
        victoryEffects = WinRect("Victory confetti", heavenlyWin, new Vector2(960, 1440), Vector2.zero);
        // A soft radial light behind the existing header, using code-created UI artwork.
        const int size = 64;
        victoryGlowTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        victoryGlowTexture.wrapMode = TextureWrapMode.Clamp;
        victoryGlowTexture.filterMode = FilterMode.Bilinear;
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float radius = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), Vector2.one * (size * 0.5f)) / (size * 0.5f);
                float alpha = Mathf.Pow(Mathf.Clamp01(1 - radius), 2);
                pixels[y * size + x] = new Color(1, 0.82f, 0.25f, alpha);
            }
        victoryGlowTexture.SetPixels(pixels);
        victoryGlowTexture.Apply(false, true);
        victoryGlowSprite = Sprite.Create(victoryGlowTexture, new Rect(0, 0, size, size), Vector2.one * 0.5f);
        var glow = WinRect("Victory golden burst", heavenlyWin, new Vector2(1120, 1000), new Vector2(0, 410));
        glow.SetAsFirstSibling();
        victoryGlow = glow.gameObject.AddComponent<Image>();
        victoryGlow.sprite = victoryGlowSprite;
        victoryGlow.raycastTarget = false;
        for (int i = 0; i < 56; i++)
        {
            var piece = WinRect("Victory confetti " + i, victoryEffects, Vector2.one, Vector2.zero);
            var image = piece.gameObject.AddComponent<Image>();
            image.raycastTarget = false;
            victoryConfetti.Add(piece);
        }
        victoryEffects.gameObject.SetActive(false);
        glow.gameObject.SetActive(false);
    }

    void OpenVictoryCelebration()
    {
        StopVictoryCelebration();
        EnsureVictoryEffects();
        if (showTopBar)
        {
            if (GameScript.instance) GameScript.instance.topBarScript.Top();
            else if (HomeScript.instance) HomeScript.instance.topBarScript.Top();
        }
        panel.SetActive(true);
        victoryEffects.gameObject.SetActive(true);
        victoryGlow.gameObject.SetActive(true);
        victoryHeader.anchoredPosition = new Vector2(0, 340);
        victoryHeader.localScale = Vector3.one * 0.18f;
        victoryGlow.color = new Color(1, 1, 1, 0);
        victoryGlow.transform.localScale = Vector3.one * 0.3f;
        victoryEntrance = DOTween.Sequence().SetUpdate(true);
        var shade = panel.GetComponent<Image>();
        if (shade)
        {
            shade.color = new Color(0, 0, 0, 0);
            victoryEntrance.Insert(0, shade.DOFade(0.55f, 0.25f));
        }
        victoryEntrance.Insert(0, victoryHeader.DOAnchorPosY(410, 0.48f).SetEase(Ease.OutCubic));
        victoryEntrance.Insert(0, victoryHeader.DOScale(1.12f, 0.32f).SetEase(Ease.OutCubic));
        victoryEntrance.Insert(0.32f, victoryHeader.DOScale(0.97f, 0.12f).SetEase(Ease.InOutSine));
        victoryEntrance.Insert(0.44f, victoryHeader.DOScale(1, 0.16f).SetEase(Ease.OutSine));
        victoryEntrance.Insert(0.12f, victoryGlow.DOFade(1, 0.2f));
        victoryEntrance.Insert(0.12f, victoryGlow.transform.DOScale(1, 0.45f).SetEase(Ease.OutCubic));
        victoryEntrance.Insert(0.4f, victoryGlow.DOFade(0, 1.7f));
        Color[] colors = { new Color32(255, 202, 60, 255), new Color32(255, 88, 122, 255),
            new Color32(89, 218, 255, 255), new Color32(184, 137, 255, 255), Color.white };
        var random = new System.Random(1847);
        for (int i = 0; i < victoryConfetti.Count; i++)
        {
            var piece = victoryConfetti[i];
            var image = piece.GetComponent<Image>();
            bool sparkle = i % 4 == 0;
            piece.sizeDelta = sparkle ? new Vector2(9, 9) : new Vector2(9, 22);
            float side = i % 2 == 0 ? -1 : 1;
            var origin = new Vector2(side * 120, 400);
            var apex = new Vector2(side * (180 + (float)random.NextDouble() * 380),
                450 + (float)random.NextDouble() * 370);
            var landing = apex + new Vector2(side * 80, -650 - (float)random.NextDouble() * 240);
            float start = 0.26f + (float)random.NextDouble() * 0.12f;
            piece.anchoredPosition = origin;
            piece.localScale = Vector3.zero;
            piece.localRotation = Quaternion.identity;
            image.color = colors[i % colors.Length];
            victoryEntrance.Insert(start, piece.DOScale(1, 0.12f));
            victoryEntrance.Insert(start, piece.DOAnchorPos(apex, 0.38f).SetEase(Ease.OutQuad));
            victoryEntrance.Insert(start + 0.38f, piece.DOAnchorPos(landing, 1.65f).SetEase(Ease.InQuad));
            victoryEntrance.Insert(start, piece.DOLocalRotate(new Vector3(0, 0, side * (400 + i * 21)),
                2.03f, RotateMode.FastBeyond360).SetEase(Ease.Linear));
            victoryEntrance.Insert(start + 1.35f, image.DOFade(0, 0.65f));
        }
        // Match the reference's short reveal before the existing reward flow starts.
        victoryEntrance.InsertCallback(0.65f, () => tweenCallback?.Invoke());
        victoryEntrance.OnComplete(() =>
        {
            victoryEffects.gameObject.SetActive(false);
            victoryGlow.gameObject.SetActive(false);
        });
    }

    void StopVictoryCelebration()
    {
        victoryEntrance?.Kill();
        victoryEntrance = null;
        if (victoryEffects) victoryEffects.gameObject.SetActive(false);
        if (victoryGlow) victoryGlow.gameObject.SetActive(false);
        if (victoryHeader)
        {
            victoryHeader.localScale = Vector3.one;
            victoryHeader.anchoredPosition = new Vector2(0, 410);
        }
    }

    void StartGiftIdle()
    {
        StopGiftIdle();
        if (!progressGift || !progressGO.activeInHierarchy) return;
        giftIdle = DOTween.Sequence()
            .Append(progressGift.DOAnchorPosY(6, 1.4f).SetEase(Ease.InOutSine))
            .Join(progressGift.DOLocalRotate(new Vector3(0, 0, 3), 1.4f).SetEase(Ease.InOutSine))
            .Join(progressGift.DOScale(1.03f, 1.4f).SetEase(Ease.InOutSine))
            .SetLoops(-1, LoopType.Yoyo);
    }

    void StopGiftIdle()
    {
        giftIdle?.Kill();
        if (!progressGift) return;
        progressGift.anchoredPosition = Vector2.zero;
        progressGift.localRotation = Quaternion.identity;
        progressGift.localScale = Vector3.one;
    }

    void OnDisable()
    {
        StopVictoryCelebration();
        goldRewardFlight?.Kill();
        // Closing during the flight still grants the earned gold exactly once.
        SettleGoldReward();
        StopGiftIdle();
    }

    Sprite WinProgressSlice(Texture2D atlas, Rect source)
    {
        const float width = 1448, height = 1086;
        var rect = new Rect(source.x / width * atlas.width,
            (height - source.y - source.height) / height * atlas.height,
            source.width / width * atlas.width, source.height / height * atlas.height);
        var sprite = Sprite.Create(atlas, rect, new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect);
        winSprites.Add(sprite);
        return sprite;
    }

    static RectTransform WinRect(string title, Transform parent, Vector2 size, Vector2 position)
    {
        var rect = new GameObject(title, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.gameObject.layer = parent.gameObject.layer;
        rect.SetParent(parent, false);
        WinPlace(rect, size, position);
        return rect;
    }

    static void WinPlace(RectTransform rect, Vector2 size, Vector2 position)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    static void WinButton(Button button, Sprite face, Vector2 size, Vector2 position)
    {
        WinPlace((RectTransform)button.transform, size, position);
        foreach (Transform child in button.transform) child.gameObject.SetActive(false);
        button.image.sprite = face;
        button.image.type = Image.Type.Simple;
        button.image.preserveAspect = false;
        button.image.color = Color.white;
        button.targetGraphic = button.image;
        button.transition = Selectable.Transition.ColorTint;
        var colors = ColorBlock.defaultColorBlock;
        colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1);
        button.colors = colors;
    }

    void WinText(TextMeshProUGUI text, float size, Color32 outline)
    {
        var material = new Material(text.fontSharedMaterial);
        winTextMaterials.Add(material);
        text.fontMaterial = material;
        text.color = Color.white;
        text.outlineColor = outline;
        text.outlineWidth = 0.2f;
        text.fontMaterial.SetTexture("_OutlineTex", Texture2D.whiteTexture);
        text.fontSize = text.fontSizeMax = size;
        text.fontSizeMin = size * 0.55f;
        text.enableAutoSizing = true;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
    }

    void FitHeavenlyWin()
    {
        if (!heavenlyWin) return;
        Rect available = ((RectTransform)heavenlyWin.parent).rect;
        float scale = Mathf.Min(available.width / 1040f, available.height / 1610f);
        heavenlyWin.localScale = Vector3.one * Mathf.Clamp(scale, 0.01f, 1);
    }

    void OnRectTransformDimensionsChange() => FitHeavenlyWin();

    void OnDestroy()
    {
        victoryEntrance?.Kill();
        if (victoryGlowSprite) Destroy(victoryGlowSprite);
        if (victoryGlowTexture) Destroy(victoryGlowTexture);
        goldRewardFlight?.Kill();
        giftIdle?.Kill();
        foreach (var material in winTextMaterials)
            if (material)
            {
                if (Application.isPlaying) Destroy(material);
                else DestroyImmediate(material);
            }
        foreach (var sprite in winSprites)
            if (sprite)
            {
                if (Application.isPlaying) Destroy(sprite);
                else DestroyImmediate(sprite);
            }
    }
}
