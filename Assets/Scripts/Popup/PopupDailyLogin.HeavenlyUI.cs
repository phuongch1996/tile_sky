using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class PopupDailyLogin
{
    RectTransform heavenlyDailyRoot;
    static Sprite dailyRoundedMask;

    void EnsureHeavenlyDailyUI()
    {
        if (heavenlyDailyRoot) return;
        var atlas = Resources.Load<Texture2D>("HeavenlyDaily/Panel");
        var boosterAtlas = Resources.Load<Texture2D>("HeavenlyDaily/Boosters");
        var check = Resources.Load<Sprite>("HeavenlyDaily/Check");
        if (!atlas || !boosterAtlas || !check || !UnityAuthentication.instance) return;

        // Keep the existing background Image, SafeAreaView and background-switching logic.
        var oldBox = box;
        var animationRoot = DailyRect("Daily Reward animation", oldBox.parent, Vector2.zero, Vector2.zero);
        animationRoot.anchorMin = Vector2.zero;
        animationRoot.anchorMax = Vector2.one;
        animationRoot.offsetMin = animationRoot.offsetMax = Vector2.zero;
        heavenlyDailyRoot = DailyRect("Heavenly Daily Reward", animationRoot,
            new Vector2(1024, 2048), Vector2.zero);
        heavenlyDailyRoot.anchorMin = heavenlyDailyRoot.anchorMax = new Vector2(0.5f, 0.5f);
        heavenlyDailyRoot.anchoredPosition = Vector2.zero;
        var panelArt = DailyRect("Panel art", heavenlyDailyRoot, new Vector2(1024, 1689.6f), new Vector2(512, 844.8f));
        var panelImage = panelArt.gameObject.AddComponent<Image>();
        // Separate the action button from the panel, so existing claim/ad visibility still applies.
        panelImage.sprite = DailySlice(atlas, new Rect(0, 0.175f, 1, 0.825f));
        panelImage.raycastTarget = false;
        var actionArt = DailySlice(atlas, new Rect(0.18f, 0.05f, 0.64f, 0.12f));

        txNextReward.transform.SetParent(heavenlyDailyRoot, false);
        PlaceDailyRect(txNextReward.rectTransform, new Vector2(390, 106), new Vector2(557, 554));
        DailyTextStyle(txNextReward, 40, new Color32(40, 28, 115, 255));
        txNextReward.richText = true;
        nextRewardGO = txNextReward.gameObject;
        PrepareDailyButton(btClose, new Vector2(125, 125), new Vector2(938, 418), null, null);
        PrepareDailyButton(btClaim, new Vector2(655.36f, 245.76f), new Vector2(512, 1822.72f), actionArt, "Claim");
        PrepareFreeDailyClaim(atlas);
        PrepareDailyButton(btAds, new Vector2(655.36f, 245.76f), new Vector2(512, 1822.72f), actionArt, "Reclaim");

        var boosters = new Sprite[3];
        for (int i = 0; i < boosters.Length; i++)
            boosters[i] = DailySlice(boosterAtlas, new Rect(i / 3f, 0, 1 / 3f, 1));

        for (int i = 0; i < dailyObs.Length; i++)
        {
            var day = dailyObs[i];
            var root = (RectTransform)day.transform;
            var fontTemplate = day.txDay;
            root.SetParent(heavenlyDailyRoot, false);
            bool special = i == 7;
            var size = new Vector2(special ? 588 : 284, 280);
            var center = special ? new Vector2(671, 1430) : new Vector2(208 + (i % 3) * 307, 794 + (i / 3) * 316);
            PlaceDailyRect(root, size, center);
            foreach (Transform child in root) child.gameObject.SetActive(false);
            var hitImage = root.GetComponent<Image>();
            if (hitImage) { hitImage.color = Color.clear; hitImage.raycastTarget = true; hitImage.enabled = true; }
            day.btDaily.transition = Selectable.Transition.None;

            var shade = DailyRect("Claimed card shade", root, size, size * 0.5f);
            day.heavenlyShade = shade.gameObject.AddComponent<Image>();
            day.heavenlyShade.sprite = DailyRoundedMask();
            day.heavenlyShade.type = Image.Type.Sliced;
            day.heavenlyShade.color = new Color32(69, 90, 130, 190);
            day.heavenlyShade.raycastTarget = false;

            var prizes = DailyRect("Live rewards", root, size, size * 0.5f);
            day.prizeGO = prizes.gameObject;
            day.heavenlyPrizeGroup = prizes.gameObject.AddComponent<CanvasGroup>();
            var gold = DailyRect("Coins", prizes, new Vector2(200, 145), new Vector2(size.x * 0.5f, 135));
            day.goldImg = gold.gameObject.AddComponent<Image>();
            day.goldImg.preserveAspect = true;
            day.goldImg.raycastTarget = false;
            day.txGoldAm = DailyLabel(fontTemplate, "Gold amount", gold, new Vector2(230, 80), new Vector2(100, 154), 62);

            var boosterGroup = DailyRect("Three boosters", prizes, size, size * 0.5f);
            day.boosterGO = boosterGroup.gameObject;
            for (int b = 0; b < 3; b++)
            {
                var iconRect = DailyRect("Booster " + b, boosterGroup, new Vector2(138, 138),
                    new Vector2(size.x * 0.5f + (b - 1) * 155, 158));
                var icon = iconRect.gameObject.AddComponent<Image>();
                icon.sprite = boosters[b];
                icon.preserveAspect = true;
                icon.raycastTarget = false;
                var count = DailyLabel(fontTemplate, "Booster count", iconRect,
                    new Vector2(100, 72), new Vector2(69, 124), 58);
                count.text = dailyLoginPrize[i].boosterAm.ToString();
                if (b == 0) day.txBoosterAm = count;
            }

            var badgeShade = DailyRect("Claimed badge shade", root, new Vector2(92, 92), new Vector2(38, 32));
            day.heavenlyBadgeShade = badgeShade.gameObject.AddComponent<Image>();
            day.heavenlyBadgeShade.sprite = DailyRoundedMask();
            day.heavenlyBadgeShade.color = new Color32(69, 90, 130, 190);
            day.heavenlyBadgeShade.raycastTarget = false;
            day.txDay = DailyLabel(fontTemplate, "Day number", root, new Vector2(85, 85), new Vector2(38, 32), 59);
            var tick = DailyRect("Claimed tick", root, new Vector2(180, 160), size * 0.5f);
            var tickImage = tick.gameObject.AddComponent<Image>();
            tickImage.sprite = check;
            tickImage.preserveAspect = true;
            tickImage.raycastTarget = false;
            day.heavenlyTick = tick.gameObject;
            day.heavenlyTick.SetActive(false);
            day.InitPrize(dailyLoginPrize[i]);
        }
        oldBox.gameObject.SetActive(false);
        // Animate a wrapper so the existing box animation does not overwrite the fitted UI scale.
        box = animationRoot;
        FitHeavenlyDailyUI();
    }

    void PrepareDailyButton(Button button, Vector2 size, Vector2 center, Sprite sprite, string label)
    {
        // The original action labels use Free Royalty, matching Battle Pass Claim.
        // The timer uses a different font and must not be the action-label template.
        var labelTemplate = button.GetComponentInChildren<TextMeshProUGUI>(true);
        button.transform.SetParent(heavenlyDailyRoot, false);
        PlaceDailyRect((RectTransform)button.transform, size, center);
        foreach (Transform child in button.transform) child.gameObject.SetActive(false);
        button.transition = Selectable.Transition.None;
        var image = button.GetComponent<Image>();
        if (!image) image = button.gameObject.AddComponent<Image>();
        button.targetGraphic = image;
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.color = sprite ? Color.white : Color.clear;
        image.enabled = image.raycastTarget = true;
        if (label != null)
        {
            var text = DailyLabel(labelTemplate ? labelTemplate : txNextReward, "Action label", button.transform,
                new Vector2(400, 100), size * 0.5f, 64);
            if (labelTemplate)
                text.fontSharedMaterial = labelTemplate.fontSharedMaterial;
            text.fontStyle = FontStyles.Normal;
            text.margin = Vector4.zero;
            text.alignment = TextAlignmentOptions.Midline;
            // Reclaim keeps the baked video icon; center its text in the remaining space.
            var rect = text.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            // Center on the green button face, above the artwork's lower shadow/padding.
            rect.anchoredPosition = new Vector2(button == btAds ? size.x * 0.09f : 0, size.y * 0.06f);
            text.text = label;
            text.outlineColor = new Color32(14, 62, 33, 255);
        }
    }

    void PrepareFreeDailyClaim(Texture2D atlas)
    {
        // The video icon is baked into the left half of the action artwork.
        // Mirror the clean right half to keep the same button style for a free claim.
        var cleanHalf = DailySlice(atlas, new Rect(0.5f, 0.05f, 0.32f, 0.12f));
        var root = (RectTransform)btClaim.transform;
        var size = root.sizeDelta;
        btClaim.GetComponent<Image>().color = Color.clear;
        for (int i = 0; i < 2; i++)
        {
            var half = DailyRect("Free claim background " + i, root,
                new Vector2(size.x * 0.5f, size.y),
                new Vector2(size.x * (i == 0 ? 0.25f : 0.75f), size.y * 0.5f));
            if (i == 0) half.localScale = new Vector3(-1, 1, 1);
            var image = half.gameObject.AddComponent<Image>();
            image.sprite = cleanHalf;
            image.raycastTarget = false;
            half.SetAsFirstSibling();
        }
    }

    void FitHeavenlyDailyUI()
    {
        if (!heavenlyDailyRoot) return;
        var parent = (RectTransform)heavenlyDailyRoot.parent;
        float scale = Mathf.Min(parent.rect.width / 1024f, parent.rect.height / 2048f) * 0.98f;
        heavenlyDailyRoot.localScale = Vector3.one * scale;
    }

    static Sprite DailySlice(Texture2D atlas, Rect area)
    {
        return Sprite.Create(atlas, new Rect(area.x * atlas.width, area.y * atlas.height,
            area.width * atlas.width, area.height * atlas.height), Vector2.one * 0.5f, 100, 0, SpriteMeshType.FullRect);
    }

    static RectTransform DailyRect(string objectName, Transform parent, Vector2 size, Vector2 center)
    {
        var rect = new GameObject(objectName, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.gameObject.layer = parent.gameObject.layer;
        rect.SetParent(parent, false);
        PlaceDailyRect(rect, size, center);
        return rect;
    }

    static void PlaceDailyRect(RectTransform rect, Vector2 size, Vector2 center)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
        rect.pivot = Vector2.one * 0.5f;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.sizeDelta = size;
        rect.anchoredPosition = new Vector2(center.x, -center.y);
    }

    static TextMeshProUGUI DailyLabel(TextMeshProUGUI template, string objectName, Transform parent,
        Vector2 size, Vector2 center, float fontSize)
    {
        var label = DailyRect(objectName, parent, size, center).gameObject.AddComponent<TextMeshProUGUI>();
        label.font = template.font;
        DailyTextStyle(label, fontSize, new Color32(54, 34, 135, 255));
        return label;
    }

    static void DailyTextStyle(TextMeshProUGUI label, float fontSize, Color outline)
    {
        label.color = Color.white;
        label.fontSize = label.fontSizeMax = fontSize;
        label.fontSizeMin = fontSize * 0.6f;
        label.enableAutoSizing = true;
        label.alignment = TextAlignmentOptions.Center;
        label.outlineColor = outline;
        label.outlineWidth = 0.2f;
        label.raycastTarget = false;
    }

    static Sprite DailyRoundedMask()
    {
        if (dailyRoundedMask) return dailyRoundedMask;
        var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        var pixels = new Color[64 * 64];
        for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
                pixels[y * 64 + x] = new Color(1, 1, 1,
                    Mathf.Clamp01(32 - Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f))));
        texture.SetPixels(pixels);
        texture.Apply(false, false);
        dailyRoundedMask = Sprite.Create(texture, new Rect(0, 0, 64, 64), Vector2.one * 0.5f,
            100, 0, SpriteMeshType.FullRect, new Vector4(31, 31, 31, 31));
        return dailyRoundedMask;
    }
}
