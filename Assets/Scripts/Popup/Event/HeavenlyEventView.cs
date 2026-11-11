using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Shared foreground layout; both popup controllers retain their own event data and claim logic.
public sealed class HeavenlyEventView
{
    RectTransform root;
    RectTransform headerPrizeRoot;
    GameObject[] headerPrizes;
    TextMeshProUGUI headerAmount;
    int headerPrizeMask = -1;
    static Sprite roundedSprite;

    public static HeavenlyEventView Create(PopupComboEvent p) => Build(p, "COMBO EVENT", 0,
        p.eventCardParent, p.eventCards, p.btClose, p.btInfo, p.progressSlider, p.txProgress,
        p.txTimer, p.btCollect, p.btCollectAds, p.prizeGO, p.prizes, p.txPrizeAm);

    public static HeavenlyEventView Create(PopupCollectionEvent p) => Build(p, "COLLECTION EVENT", 1,
        p.eventCardParent, p.eventCards, p.btClose, p.btInfo, p.progressSlider, p.txProgress,
        p.txTimer, p.btCollect, p.btCollectAds, p.prizeGO, p.prizes, p.txPrizeAm);

    static HeavenlyEventView Build(Popup popup, string title, int symbolIndex, Transform cardParent,
        List<EventCard> cards, Button close, Button info, Slider slider, TextMeshProUGUI progress,
        TextMeshProUGUI timer, Button collect, Button collectAds, GameObject prizeGroup,
        GameObject[] prizes, TextMeshProUGUI amount)
    {
        var panel = Resources.Load<Sprite>("HeavenlyEvents/PanelWithoutTimer");
        var timerArt = Resources.Load<Sprite>("HeavenlyEvents/Panel");
        var cardArt = Resources.Load<Sprite>("HeavenlyEvents/RewardCard");
        var symbols = Resources.Load<Texture2D>("HeavenlyEvents/Symbols");
        var padlock = Resources.Load<Sprite>("HeavenlyEvents/Lock");
        var scroll = cardParent.GetComponentInParent<ScrollRect>(true);
        if (!panel || !timerArt || !cardArt || !symbols || !padlock || !scroll) return null;

        var view = new HeavenlyEventView
        {
            headerPrizes = prizes, headerAmount = amount,
            headerPrizeRoot = (RectTransform)prizeGroup.transform
        };
        var oldBox = popup.box;
        var animation = Rect("Event animation", oldBox.parent, Vector2.zero, Vector2.zero);
        Stretch(animation);
        view.root = Rect("Heavenly Event", animation, new Vector2(1024, 1820), Vector2.zero);
        view.root.anchorMin = view.root.anchorMax = Vector2.one * 0.5f;
        view.root.anchoredPosition = Vector2.zero;
        // Extend only the frame downward by 5%; keep header and reward coordinates fixed.
        // Preserve the header and ornamental footer while stretching the empty sky between them.
        const float frameHeight = 1820f * 1.05f;
        var frame = Rect("Extended event frame", view.root,
            new Vector2(1024, frameHeight), new Vector2(512, frameHeight * 0.5f));
        var image = frame.gameObject.AddComponent<Image>();
        image.sprite = Sprite.Create(panel.texture, panel.rect, Vector2.one * 0.5f,
            panel.pixelsPerUnit, 0, SpriteMeshType.FullRect,
            new Vector4(0, panel.rect.height * 0.18f, 0, panel.rect.height * 0.4f));
        image.type = Image.Type.Sliced;
        image.pixelsPerUnitMultiplier = panel.rect.height / 1820f;
        image.raycastTarget = false;

        var heading = Label(progress, "Event title", view.root, new Vector2(720, 115), new Vector2(512, 370), 77);
        heading.text = title;
        // The countdown is separate from the backdrop so both its frame and text
        // can shrink together without squeezing the rest of the event panel.
        float timerScaleX = timerArt.rect.width / 941f;
        float timerScaleY = timerArt.rect.height / 1672f;
        var timerSprite = Sprite.Create(timerArt.texture,
            new UnityEngine.Rect(timerArt.rect.x + 278f * timerScaleX,
                timerArt.rect.y + timerArt.rect.height - 554f * timerScaleY,
                386f * timerScaleX, 121f * timerScaleY), Vector2.one * 0.5f,
            timerArt.pixelsPerUnit, 0, SpriteMeshType.FullRect);
        Graphic("Compact countdown frame", view.root, new Vector2(320, 100),
            new Vector2(512, 510), timerSprite, Color.white);
        Move(timer.rectTransform, view.root, new Vector2(210, 64), new Vector2(562, 510));
        TextStyle(timer, 44, new Color32(0, 44, 133, 255));
        InvisibleButton(close, view.root, new Vector2(115, 115), new Vector2(942, 480));
        InvisibleButton(info, view.root, new Vector2(100, 100), new Vector2(930, 590));
        const float collectHeight = 105f * 1.5f;
        Move((RectTransform)collect.transform, view.root, new Vector2(355, collectHeight), new Vector2(312, 1710));
        Move((RectTransform)collectAds.transform, view.root, new Vector2(355, collectHeight), new Vector2(712, 1710));

        Move(progress.rectTransform, view.root, new Vector2(490, 70), new Vector2(510, 610));
        TextStyle(progress, 49, new Color32(0, 44, 133, 255));
        Move((RectTransform)prizeGroup.transform, view.root, new Vector2(155, 125), new Vector2(800, 608));
        StylePrizes(prizes, amount, (RectTransform)prizeGroup.transform, 155, 125, 47);
        Move((RectTransform)slider.transform, view.root, new Vector2(570, 65), new Vector2(510, 610));
        foreach (Transform child in slider.transform) child.gameObject.SetActive(false);
        var track = slider.GetComponent<Image>();
        if (!track) track = slider.gameObject.AddComponent<Image>();
        track.sprite = Rounded(); track.type = Image.Type.Sliced;
        track.color = new Color32(0, 59, 141, 255); track.raycastTarget = false;
        var border = slider.GetComponent<Outline>();
        if (!border) border = slider.gameObject.AddComponent<Outline>();
        border.effectColor = new Color32(188, 251, 255, 255); border.effectDistance = new Vector2(3, 3);
        var area = Rect("Fill area", slider.transform, new Vector2(550, 43), new Vector2(285, 32.5f));
        var fill = Rect("Live progress", area, Vector2.zero, Vector2.zero);
        Stretch(fill);
        var fillImage = fill.gameObject.AddComponent<Image>();
        fillImage.sprite = Rounded(); fillImage.type = Image.Type.Sliced;
        fillImage.color = new Color32(44, 219, 255, 255); fillImage.raycastTarget = false;
        slider.handleRect = null; slider.fillRect = fill; slider.targetGraphic = track;
        slider.interactable = false; slider.transition = Selectable.Transition.None;
        slider.transform.SetSiblingIndex(1);
        var symbol = Rect("Event symbol", view.root, new Vector2(115, 125), new Vector2(225, 610));
        var symbolImage = symbol.gameObject.AddComponent<Image>();
        symbolImage.sprite = Sprite.Create(symbols,
            new UnityEngine.Rect(symbolIndex * symbols.width / 2f, 0, symbols.width / 2f, symbols.height),
            Vector2.one * 0.5f, 100, 0, SpriteMeshType.FullRect);
        symbolImage.preserveAspect = true; symbolImage.raycastTarget = false;

        Move((RectTransform)scroll.transform, view.root, new Vector2(850, 900), new Vector2(512, 1125));
        scroll.horizontal = false; scroll.vertical = true;
        if (scroll.horizontalScrollbar) scroll.horizontalScrollbar.gameObject.SetActive(false);
        if (scroll.verticalScrollbar) scroll.verticalScrollbar.gameObject.SetActive(false);
        scroll.horizontalScrollbar = scroll.verticalScrollbar = null;
        var scrollImage = scroll.GetComponent<Image>();
        if (scrollImage) scrollImage.color = Color.clear;
        Stretch(scroll.viewport);
        var viewportImage = scroll.viewport.GetComponent<Image>();
        var mask = scroll.viewport.GetComponent<Mask>();
        if (mask)
        {
            if (viewportImage) viewportImage.color = Color.white;
            mask.showMaskGraphic = false;
        }
        else
        {
            if (viewportImage) viewportImage.color = Color.clear;
            if (!scroll.viewport.GetComponent<RectMask2D>())
                scroll.viewport.gameObject.AddComponent<RectMask2D>();
        }
        // Content retains bottom-up progression and the controllers' existing 250-unit scroll offset.
        var content = (RectTransform)cardParent;
        foreach (var layout in content.GetComponents<LayoutGroup>()) layout.enabled = false;
        var fitter = content.GetComponent<ContentSizeFitter>();
        if (fitter) fitter.enabled = false;
        content.anchorMin = new Vector2(0, 0); content.anchorMax = new Vector2(1, 0);
        content.pivot = new Vector2(0.5f, 0);
        content.sizeDelta = new Vector2(0, cards.Count * 250);
        content.anchoredPosition = Vector2.zero;
        for (int i = 0; i < cards.Count; i++) StyleCard(cards[i], i, cards.Count, cardArt, padlock);

        oldBox.gameObject.SetActive(false);
        popup.box = animation;
        view.UpdateLayout();
        return view;
    }

    static void StyleCard(EventCard card, int index, int count, Sprite art, Sprite padlock)
    {
        var root = (RectTransform)card.transform;
        foreach (Transform child in root) child.gameObject.SetActive(false);
        root.anchorMin = root.anchorMax = new Vector2(0.5f, 0);
        root.pivot = new Vector2(0.5f, 0);
        root.sizeDelta = new Vector2(850, 250);
        root.anchoredPosition = new Vector2(0, index * 250);

        card.linkGO = Graphic("Golden path", root, new Vector2(36, 250), new Vector2(205, 0), null,
            new Color32(255, 213, 42, 255)).gameObject;
        card.linkGO.SetActive(index < count - 1);
        card.linkFillGO = Graphic("Completed path", root, new Vector2(16, 250), new Vector2(205, 0), null,
            new Color32(119, 245, 60, 255)).gameObject;
        var diamond = Graphic("Gold milestone rim", root, new Vector2(85, 85), new Vector2(205, 125), null,
            new Color32(255, 211, 29, 255)).rectTransform;
        diamond.localRotation = Quaternion.Euler(0, 0, 45);
        Graphic("Green milestone", diamond, new Vector2(66, 66), new Vector2(42.5f, 42.5f), null,
            new Color32(66, 205, 25, 255));
        card.currentGO = Graphic("Current blue milestone", diamond, new Vector2(66, 66),
            new Vector2(42.5f, 42.5f), null, new Color32(0, 161, 255, 255)).gameObject;
        Move(card.txIndex.rectTransform, root, new Vector2(100, 100), new Vector2(205, 125));
        card.txIndex.gameObject.SetActive(true);
        TextStyle(card.txIndex, 60, new Color32(0, 47, 101, 255));

        Graphic("Cloud reward frame", root, new Vector2(390, 250), new Vector2(515, 125), art, Color.white);
        Move((RectTransform)card.prizesGO.transform, root, new Vector2(245, 175), new Vector2(515, 138));
        card.prizesGO.SetActive(true);
        StylePrizes(card.prizes, card.txPrizeAm, (RectTransform)card.prizesGO.transform, 245, 175, 53);
        card.lockGO = Graphic("Golden padlock", root, new Vector2(77, 90), new Vector2(670, 209), padlock, Color.white).gameObject;
        card.claimableGO = Label(card.txIndex, "Claim available", root,
            new Vector2(180, 45), new Vector2(515, 220), 30).gameObject;
        card.claimableGO.GetComponent<TextMeshProUGUI>().text = "CLAIM";
        var check = Resources.Load<Sprite>("HeavenlyDaily/Check");
        card.claimedGO = Graphic("Claimed", root, new Vector2(95, 95), new Vector2(670, 205), check, Color.white).gameObject;
        card.heavenlyStyle = true;
    }

    static void StylePrizes(GameObject[] prizes, TextMeshProUGUI amount, RectTransform parent,
        float width, float height, float fontSize)
    {
        int visible = 0;
        foreach (var prize in prizes) if (prize.activeSelf) visible++;
        int index = 0;
        foreach (var prize in prizes)
        {
            Move((RectTransform)prize.transform, parent, new Vector2(visible > 1 ? 100 : 145, 115),
                new Vector2(width * 0.5f + (index - (visible - 1) * 0.5f) * 95, height * 0.38f));
            if (prize.activeSelf) index++;
            var image = prize.GetComponent<Image>();
            if (image) { image.preserveAspect = true; image.raycastTarget = false; }
        }
        Move(amount.rectTransform, parent, new Vector2(width, 65), new Vector2(width * 0.5f, height * 0.79f));
        TextStyle(amount, fontSize, new Color32(126, 20, 4, 255));
    }

    public void UpdateLayout()
    {
        if (!root) return;
        int mask = 0;
        for (int i = 0; i < headerPrizes.Length; i++)
            if (headerPrizes[i].activeSelf) mask |= 1 << i;
        if (mask != headerPrizeMask)
        {
            StylePrizes(headerPrizes, headerAmount, headerPrizeRoot, 155, 125, 47);
            headerPrizeMask = mask;
        }
        var parent = (RectTransform)root.parent;
        float scale = Mathf.Min(parent.rect.width / 1024, parent.rect.height / 1820) * 0.98f;
        root.localScale = Vector3.one * scale;
        root.anchoredPosition = Vector2.up * (35f * scale);
    }

    static RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 center)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.gameObject.layer = parent.gameObject.layer;
        Move(rect, parent, size, center);
        return rect;
    }

    static void Move(RectTransform rect, Transform parent, Vector2 size, Vector2 center)
    {
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = Vector2.one * 0.5f;
        rect.sizeDelta = size; rect.anchoredPosition = new Vector2(center.x, -center.y);
        rect.localScale = Vector3.one; rect.localRotation = Quaternion.identity;
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    static Image Graphic(string name, Transform parent, Vector2 size, Vector2 center, Sprite sprite, Color color)
    {
        var image = Rect(name, parent, size, center).gameObject.AddComponent<Image>();
        image.sprite = sprite; image.color = color; image.preserveAspect = sprite;
        image.raycastTarget = false;
        return image;
    }

    static void InvisibleButton(Button button, Transform parent, Vector2 size, Vector2 center)
    {
        Move((RectTransform)button.transform, parent, size, center);
        foreach (Transform child in button.transform) child.gameObject.SetActive(false);
        button.image.color = Color.clear; button.image.enabled = true;
        button.image.raycastTarget = true; button.transition = Selectable.Transition.None;
    }

    static TextMeshProUGUI Label(TextMeshProUGUI template, string name, Transform parent, Vector2 size,
        Vector2 center, float fontSize)
    {
        var label = Rect(name, parent, size, center).gameObject.AddComponent<TextMeshProUGUI>();
        label.font = template.font; TextStyle(label, fontSize, new Color32(0, 30, 153, 255));
        return label;
    }

    static void TextStyle(TextMeshProUGUI label, float size, Color outline)
    {
        label.color = Color.white; label.fontSize = label.fontSizeMax = size;
        label.fontSizeMin = size * 0.55f; label.enableAutoSizing = true;
        label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false;
        label.outlineColor = outline; label.outlineWidth = 0.18f;
    }

    static Sprite Rounded()
    {
        if (roundedSprite) return roundedSprite;
        var texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        var pixels = new Color[1024];
        for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++)
            pixels[y * 32 + x] = new Color(1, 1, 1,
                Mathf.Clamp01(16 - Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f))));
        texture.SetPixels(pixels); texture.Apply();
        roundedSprite = Sprite.Create(texture, new UnityEngine.Rect(0, 0, 32, 32), Vector2.one * 0.5f,
            100, 0, SpriteMeshType.FullRect, new Vector4(15, 15, 15, 15));
        return roundedSprite;
    }
}
