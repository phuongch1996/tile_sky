using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class RankCard
{
    bool tileSkyStyled;
    public void ApplyTileSkyStyle(bool pinnedUser = false)
    {
        if (tileSkyStyled || !TileSkyRankArt.Ready) return;
        tileSkyStyled = true;
        foreach (var layout in GetComponentsInChildren<LayoutGroup>(true)) layout.enabled = false;
        foreach (var fitter in GetComponentsInChildren<ContentSizeFitter>(true)) fitter.enabled = false;
        foreach (var fitter in GetComponentsInChildren<AspectRatioFitter>(true)) fitter.enabled = false;
        var root = (RectTransform)transform;
        root.localScale = Vector3.one;
        scale = Vector3.one;
        var element = GetComponent<LayoutElement>();
        if (!element) element = gameObject.AddComponent<LayoutElement>();
        element.minHeight = element.preferredHeight = 155;
        element.flexibleHeight = 0;
        var background = GetComponent<Image>();
        if (!background) background = gameObject.AddComponent<Image>();
        bool user = pinnedUser || name.Contains("User");
        background.sprite = user ? TileSkyRankArt.UserRow : TileSkyRankArt.Row;
        background.type = Image.Type.Simple;
        background.color = Color.white;
        background.raycastTarget = false;
        defaultRankIconSprite = TileSkyRankArt.Rounded;
        defaultRankIconColor = new Color32(12, 64, 150, 255);
        defaultRankIconCached = true;
        if (imgRankIcon)
        {
            imgRankIcon.sprite = defaultRankIconSprite;
            imgRankIcon.color = defaultRankIconColor;
            imgRankIcon.type = Image.Type.Sliced;
        }

        // Flatten the legacy card so its nested space-theme decorations are hidden.
        var graphics = new Graphic[] { imgRankIcon, txRank, imgAvatar, imgFrame, imgGift, txDisplayName, txScore };
        foreach (var graphic in graphics)
            if (graphic) graphic.transform.SetParent(root, false);
        foreach (Transform child in root)
        {
            bool live = false;
            foreach (var graphic in graphics)
                if (graphic && graphic.transform == child) live = true;
            if (!live) child.gameObject.SetActive(false);
        }
        PlaceCardGraphic(imgRankIcon, new Vector2(95, 100), new Vector2(0.075f, 0.5f));
        PlaceCardGraphic(txRank, new Vector2(90, 95), new Vector2(0.075f, 0.5f));
        PlaceCardGraphic(imgAvatar, new Vector2(120, 120), new Vector2(0.21f, 0.5f));
        PlaceCardGraphic(imgFrame, new Vector2(140, 140), new Vector2(0.21f, 0.5f));
        PlaceCardGraphic(txDisplayName, new Vector2(330, 90), new Vector2(0.475f, 0.5f));
        PlaceCardGraphic(txScore, new Vector2(180, 85), new Vector2(0.79f, 0.5f));
        PlaceCardGraphic(imgGift, new Vector2(65, 75), new Vector2(0.925f, 0.5f));
        TileSkyRankArt.TextStyle(txRank, 45);
        TileSkyRankArt.TextStyle(txDisplayName, 36);
        txDisplayName.alignment = TextAlignmentOptions.MidlineLeft;
        TileSkyRankArt.TextStyle(txScore, 42);
        txScore.color = user ? new Color32(68, 42, 9, 255) : new Color32(255, 240, 116, 255);
        // Keep the portrait under its frame and the rank number over its badge.
        imgAvatar.transform.SetSiblingIndex(0);
        imgFrame.transform.SetSiblingIndex(1);
        txRank.transform.SetAsLastSibling();
    }

    void PlaceCardGraphic(Graphic graphic, Vector2 size, Vector2 anchor)
    {
        if (!graphic) return;
        TileSkyRankArt.Place(graphic.rectTransform, transform, size, Vector2.zero);
        graphic.rectTransform.anchorMin = graphic.rectTransform.anchorMax = anchor;
        graphic.gameObject.SetActive(true);
        graphic.raycastTarget = false;
        if (graphic is Image image)
        {
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
        }
    }
}
