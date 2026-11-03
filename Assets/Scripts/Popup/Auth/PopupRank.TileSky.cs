using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class PopupRank
{
    RectTransform tileSkyRankRoot;
    readonly Image[] tileSkyTabImages = new Image[3];

    public void EnsureTileSkyLeaderboard()
    {
        if (tileSkyRankRoot || !TileSkyRankArt.Ready) return;
        var safeArea = btClose.transform.parent;
        while (safeArea.parent && safeArea.name != "SafeAreaView" && safeArea != panel.transform)
            safeArea = safeArea.parent;
        tileSkyRankRoot = TileSkyRankArt.Rect("Tile Sky Leaderboard", safeArea, Vector2.zero, Vector2.zero);
        TileSkyRankArt.Stretch(tileSkyRankRoot, Vector2.zero, Vector2.zero);

        // Replace the old destination-themed background with a sky gradient.
        if (bg)
        {
            bg.sprite = TileSkyRankArt.Sky;
            bg.type = Image.Type.Simple;
            bg.color = Color.white;
            bg.preserveAspect = false;
        }
        var title = TileSkyRankArt.Rect("Cloud title", tileSkyRankRoot, new Vector2(680, 235), new Vector2(0, -135));
        title.anchorMin = title.anchorMax = new Vector2(0.5f, 1);
        TileSkyRankArt.Image(title, TileSkyRankArt.Header);
        var titleText = TileSkyRankArt.Label("LEADERBOARD", txBtDailyRank, title,
            new Vector2(540, 85), new Vector2(0, -8), 58);
        titleText.outlineWidth = 0.22f;

        var toggles = new[] { toggleDailyRank, toggleAllTimeRank, toggleDailyChallengeRank };
        var tabGroup = tileSkyRankRoot.gameObject.AddComponent<ToggleGroup>();
        tabGroup.allowSwitchOff = false;
        var labels = new[] { txBtDailyRank, txBtAllTimeRank, txBtDailyChallengeRank };
        var names = new[] { "DAILY", "ALL TIME", "CHALLENGE" };
        for (int i = 0; i < toggles.Length; i++)
        {
            var toggle = toggles[i];
            toggle.group = tabGroup;
            toggle.toggleTransition = Toggle.ToggleTransition.None;
            var rect = (RectTransform)toggle.transform;
            TileSkyRankArt.Place(rect, tileSkyRankRoot, new Vector2(300, 100), Vector2.zero);
            foreach (Transform child in rect) child.gameObject.SetActive(false);
            TileSkyRankArt.Place(labels[i].rectTransform, rect, new Vector2(280, 75), Vector2.zero);
            labels[i].gameObject.SetActive(true);
            labels[i].text = names[i];
            TileSkyRankArt.TextStyle(labels[i], 40);
            var image = toggle.GetComponent<Image>();
            if (!image) image = toggle.gameObject.AddComponent<Image>();
            image.enabled = true;
            image.sprite = TileSkyRankArt.Tab;
            image.type = Image.Type.Simple;
            toggle.targetGraphic = image;
            toggle.graphic = null;
            toggle.transition = Selectable.Transition.None;
            tileSkyTabImages[i] = image;
        }

        dailyTab = BuildTileSkyTab(dailyParent, currentUserDaily, noDailyGO, "Daily rankings");
        allTimeTab = BuildTileSkyTab(allTimeParent, currentUserAllTime, noAllTimeGO, "All time rankings");
        dailyChallengeTab = BuildTileSkyTab(dailyChallengeParent, currentUserDailyChallenge, noDailyChallengeGO, "Challenge rankings");
        foreach (var status in new[] { loadingGO, loadFailedGO })
        {
            TileSkyRankArt.Place((RectTransform)status.transform, tileSkyRankRoot,
                new Vector2(750, 140), new Vector2(0, 0));
        }
        TileSkyRankArt.Place(txTimeLeft.rectTransform, dailyChallengeTab.transform, new Vector2(400, 60), new Vector2(0, -380));
        txTimeLeft.rectTransform.anchorMin = txTimeLeft.rectTransform.anchorMax = new Vector2(0.5f, 1);
        TileSkyRankArt.TextStyle(txTimeLeft, 34);
        TileSkyRankArt.Place((RectTransform)btInfo.transform, dailyChallengeTab.transform, new Vector2(70, 70), new Vector2(400, -380));
        btInfo.GetComponent<RectTransform>().anchorMin = btInfo.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1);
        TileSkyRankArt.Place((RectTransform)btPrizePreview.transform, dailyChallengeTab.transform, new Vector2(70, 70), new Vector2(-400, -380));
        btPrizePreview.GetComponent<RectTransform>().anchorMin = btPrizePreview.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1);
        prizeReviewGO.transform.SetParent(dailyChallengeTab.transform, false);
        TileSkyRankArt.Place((RectTransform)btEditPlayer.transform, tileSkyRankRoot, new Vector2(72, 72), new Vector2(-95, 175));
        btEditPlayer.GetComponent<RectTransform>().anchorMin = btEditPlayer.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0);

        TileSkyRankArt.Place((RectTransform)btClose.transform, tileSkyRankRoot, new Vector2(150, 150), new Vector2(135, -135));
        btClose.GetComponent<RectTransform>().anchorMin = btClose.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
        foreach (Transform child in btClose.transform) child.gameObject.SetActive(false);
        btClose.image.sprite = Resources.Load<Sprite>("HeavenlyBattlePass/Back");
        btClose.image.type = Image.Type.Simple;
        btClose.image.preserveAspect = true;
        btClose.image.color = Color.white;
        btClose.image.enabled = true;
        btClose.transition = Selectable.Transition.None;
        FitTileSkyBackButton();

        // All live controls have moved. Hide the former title, podiums and panels.
        foreach (Transform child in safeArea)
            if (child != tileSkyRankRoot && (!bg || child != bg.transform)) child.gameObject.SetActive(false);
        LayoutTileSkyTabs();
        RefreshTileSkyTabs();
    }

    GameObject BuildTileSkyTab(Transform content, RankCard currentUser, GameObject empty, string name)
    {
        var scroll = content.GetComponentInParent<ScrollRect>(true);
        var tab = TileSkyRankArt.Rect(name, tileSkyRankRoot, Vector2.zero, Vector2.zero);
        TileSkyRankArt.Stretch(tab, Vector2.zero, Vector2.zero);
        tab.SetAsFirstSibling();
        var border = TileSkyRankArt.Rect("Cyan frame", tab, Vector2.zero, Vector2.zero);
        float panelTop = currentUser == currentUserDailyChallenge ? 425f : 355f;
        TileSkyRankArt.Stretch(border, new Vector2(30, 95), new Vector2(-30, -panelTop));
        TileSkyRankArt.Image(border, TileSkyRankArt.Rounded).color = new Color32(109, 242, 255, 255);
        var inset = TileSkyRankArt.Rect("Blue panel", border, Vector2.zero, Vector2.zero);
        TileSkyRankArt.Stretch(inset, new Vector2(7, 7), new Vector2(-7, -7));
        TileSkyRankArt.Image(inset, TileSkyRankArt.Rounded).color = new Color32(8, 64, 151, 245);
        var scrollRect = (RectTransform)scroll.transform;
        scrollRect.SetParent(tab, false);
        scrollRect.localScale = Vector3.one;
        TileSkyRankArt.Stretch(scrollRect, new Vector2(45, 275), new Vector2(-45, -panelTop - 14));
        var scrollImage = scroll.GetComponent<Image>();
        if (scrollImage) scrollImage.color = Color.clear;
        if (scroll.horizontalScrollbar) scroll.horizontalScrollbar.gameObject.SetActive(false);
        if (scroll.verticalScrollbar) scroll.verticalScrollbar.gameObject.SetActive(false);
        scroll.horizontalScrollbar = scroll.verticalScrollbar = null;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        // The old viewport sits inside a purple Image with its own top inset.
        // Move it directly under the scroll rect so no legacy padding or art remains.
        scroll.viewport.SetParent(scrollRect, false);
        TileSkyRankArt.Stretch(scroll.viewport, Vector2.zero, Vector2.zero);
        foreach (Transform child in scrollRect)
            if (child != scroll.viewport && child != empty.transform && child != currentUser.transform)
                child.gameObject.SetActive(false);
        var viewportMask = scroll.viewport.GetComponent<Mask>();
        if (viewportMask) viewportMask.showMaskGraphic = false;
        else
        {
            var viewportImage = scroll.viewport.GetComponent<Image>();
            if (viewportImage) viewportImage.color = Color.clear;
        }
        content.localScale = Vector3.one;
        var contentRect = (RectTransform)content;
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = Vector2.one;
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, contentRect.sizeDelta.y);
        var layout = content.GetComponent<VerticalLayoutGroup>();
        if (layout)
        {
            layout.padding = new RectOffset(8, 8, 6, 6);
            layout.spacing = 12;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }
        currentUser.ApplyTileSkyStyle(true);
        var userRect = (RectTransform)currentUser.transform;
        userRect.SetParent(tab, false);
        currentUser.gameObject.SetActive(true);
        userRect.anchorMin = new Vector2(0, 0);
        userRect.anchorMax = new Vector2(1, 0);
        userRect.pivot = Vector2.one * 0.5f;
        userRect.anchoredPosition = new Vector2(0, 175);
        userRect.sizeDelta = new Vector2(-90, 155);
        TileSkyRankArt.Place((RectTransform)empty.transform, tab, new Vector2(800, 150), Vector2.zero);
        return tab.gameObject;
    }

    void LayoutTileSkyTabs()
    {
        if (!tileSkyRankRoot) return;
        var toggles = new[] { toggleDailyRank, toggleAllTimeRank, toggleDailyChallengeRank };
        int count = toggleDailyChallengeRank.gameObject.activeSelf ? 3 : 2;
        for (int i = 0; i < count; i++)
        {
            var rect = (RectTransform)toggles[i].transform;
            rect.anchorMin = rect.anchorMax = new Vector2((i + 0.5f) / count, 1);
            rect.anchoredPosition = new Vector2(0, -295);
        }
    }

    void RefreshTileSkyTabs()
    {
        if (!tileSkyRankRoot) return;
        var toggles = new[] { toggleDailyRank, toggleAllTimeRank, toggleDailyChallengeRank };
        for (int i = 0; i < toggles.Length; i++)
            tileSkyTabImages[i].color = toggles[i].isOn ? Color.white : new Color32(45, 92, 177, 255);
    }

    void FitTileSkyBackButton()
    {
        if (!tileSkyRankRoot) return;
        // Compare visible artwork, accounting for both sprites' transparent margins.
        float settingsWidth = HomeScript.instance
            ? ((RectTransform)HomeScript.instance.btSettings.transform).sizeDelta.x
            : 149.78f * 208f / 183f;
        float backSize = settingsWidth * (181f / 256f) / (1036f / 1254f);
        var backRect = (RectTransform)btClose.transform;
        backRect.sizeDelta = Vector2.one * backSize;
        if (!HomeScript.instance) return;

        // Home and this popup have separate safe-area hierarchies. Match screen
        // coordinates instead of copying an anchored offset between those parents.
        var settings = (RectTransform)HomeScript.instance.btSettings.transform;
        var homeCanvas = settings.GetComponentInParent<Canvas>().rootCanvas;
        var popupCanvas = tileSkyRankRoot.GetComponentInParent<Canvas>().rootCanvas;
        var homeCamera = homeCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : homeCanvas.worldCamera;
        var popupCamera = popupCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : popupCanvas.worldCamera;
        var center = RectTransformUtility.WorldToScreenPoint(homeCamera, settings.TransformPoint(settings.rect.center));
        var edge = RectTransformUtility.WorldToScreenPoint(homeCamera,
            settings.TransformPoint(settings.rect.center + Vector2.right * settings.rect.width * 0.5f));
        float visibleHalfWidth = Vector2.Distance(center, edge) * (181f / 256f);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(tileSkyRankRoot, center, popupCamera, out var localCenter);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(tileSkyRankRoot,
            center + Vector2.right * visibleHalfWidth, popupCamera, out var localEdge);
        backRect.anchorMin = backRect.anchorMax = Vector2.one * 0.5f;
        backRect.anchoredPosition = localCenter;
        backRect.sizeDelta = Vector2.one * (Vector2.Distance(localCenter, localEdge) * 2f / (1036f / 1254f));
    }
}

static class TileSkyRankArt
{
    static Texture2D atlas;
    static Sprite header, row, userRow, tab, rounded, sky;
    public static bool Ready => atlas || (atlas = Resources.Load<Texture2D>("TileSkyLeaderboard/Controls"));
    public static Sprite Header => header ? header : header = Slice(0, 0.325f);
    public static Sprite Row => row ? row : row = Slice(0.34f, 0.215f);
    public static Sprite UserRow => userRow ? userRow : userRow = Slice(0.57f, 0.22f);
    public static Sprite Tab => tab ? tab : tab = Slice(0.81f, 0.19f);
    static Sprite Slice(float top, float height) => Sprite.Create(atlas,
        new Rect(0, (1 - top - height) * atlas.height, atlas.width, height * atlas.height),
        Vector2.one * 0.5f, 100, 0, SpriteMeshType.FullRect);

    public static Sprite Rounded
    {
        get
        {
            if (rounded) return rounded;
            var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            var pixels = new Color[4096];
            for (int y = 0; y < 64; y++)
                for (int x = 0; x < 64; x++)
                    pixels[y * 64 + x] = new Color(1, 1, 1, Mathf.Clamp01(32 - Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f))));
            texture.SetPixels(pixels);
            texture.Apply();
            rounded = Sprite.Create(texture, new Rect(0, 0, 64, 64), Vector2.one * 0.5f, 100, 0,
                SpriteMeshType.FullRect, new Vector4(31, 31, 31, 31));
            return rounded;
        }
    }

    public static Sprite Sky
    {
        get
        {
            if (sky) return sky;
            var texture = new Texture2D(1, 256, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < 256; y++)
                texture.SetPixel(0, y, Color.Lerp(new Color32(200, 239, 255, 255), new Color32(38, 144, 231, 255), y / 255f));
            texture.Apply();
            return sky = Sprite.Create(texture, new Rect(0, 0, 1, 256), Vector2.one * 0.5f);
        }
    }

    public static RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 position)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.gameObject.layer = parent.gameObject.layer;
        Place(rect, parent, size, position);
        return rect;
    }
    public static void Place(RectTransform rect, Transform parent, Vector2 size, Vector2 position)
    {
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one * 0.5f;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }
    public static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.localScale = Vector3.one;
        rect.offsetMin = min;
        rect.offsetMax = max;
    }
    public static Image Image(RectTransform rect, Sprite sprite)
    {
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.type = sprite == Rounded ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
        image.raycastTarget = false;
        return image;
    }
    public static TextMeshProUGUI Label(string text, TextMeshProUGUI template, Transform parent, Vector2 size, Vector2 position, float fontSize)
    {
        var label = Rect(text, parent, size, position).gameObject.AddComponent<TextMeshProUGUI>();
        label.font = template.font;
        label.text = text;
        TextStyle(label, fontSize);
        return label;
    }
    public static void TextStyle(TextMeshProUGUI label, float fontSize)
    {
        label.color = Color.white;
        label.fontSize = label.fontSizeMax = fontSize;
        label.fontSizeMin = fontSize * 0.6f;
        label.enableAutoSizing = true;
        label.alignment = TextAlignmentOptions.Center;
        label.outlineColor = new Color32(9, 55, 143, 255);
        label.outlineWidth = 0.14f;
        label.raycastTarget = false;
    }
}
