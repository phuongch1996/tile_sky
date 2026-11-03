using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class PopupRateUs
{
    // Scene instances may still contain positions driven by the former layout
    // groups. Normalize the live controls before every opening, including after
    // a prefab refresh while the scene is already loaded in the Editor.
    void ApplyTileSkyLayout()
    {
        if (!(box is RectTransform root)) return;

        var oldStarParent = btStars[0].transform.parent;
        var oldDescriptionParent = txRateDesc.transform.parent;
        foreach (var layout in root.GetComponentsInChildren<LayoutGroup>(true))
            layout.enabled = false;
        foreach (var fitter in root.GetComponentsInChildren<ContentSizeFitter>(true))
            fitter.enabled = false;

        PlaceRateRect(root, (RectTransform)root.parent, new Vector2(1000, 733.3333f), Vector2.zero, false);
        for (int i = 0; i < btStars.Length; i++)
        {
            var star = (RectTransform)btStars[i].transform;
            PlaceRateRect(star, root, new Vector2(120, 120), new Vector2((i - 2) * 145, 86.6667f));
            var selected = (RectTransform)stars[i].transform;
            selected.SetParent(star, false);
            selected.anchorMin = Vector2.zero;
            selected.anchorMax = Vector2.one;
            selected.pivot = Vector2.one * 0.5f;
            selected.localScale = Vector3.one;
            selected.localRotation = Quaternion.identity;
            selected.offsetMin = selected.offsetMax = Vector2.zero;
            foreach (var graphic in selected.GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = false;
        }

        PlaceRateRect(txRateDesc.rectTransform, root, new Vector2(800, 90), new Vector2(0, -23.3333f));
        txRateDesc.alignment = TextAlignmentOptions.Center;
        txRateDesc.enableAutoSizing = true;
        txRateDesc.fontSizeMin = 26;
        txRateDesc.fontSizeMax = 42;
        txRateDesc.color = Color.white;
        txRateDesc.raycastTarget = false;
        txRateDesc.margin = Vector4.zero;

        var reward = (RectTransform)goldsGO.transform;
        PlaceRateRect(reward, root, new Vector2(145, 70), new Vector2(-45, -103.3333f));
        var amount = goldsGO.GetComponent<TextMeshProUGUI>();
        if (amount)
        {
            amount.alignment = TextAlignmentOptions.Center;
            amount.enableAutoSizing = true;
            amount.fontSizeMin = 48;
            amount.fontSizeMax = 68;
            amount.color = Color.white;
            amount.raycastTarget = false;
            amount.margin = Vector4.zero;
        }
        var coin = goldsGO.GetComponentInChildren<Image>(true);
        if (coin)
        {
            PlaceRateRect(coin.rectTransform, reward, new Vector2(100, 80), new Vector2(118, 0));
            coin.preserveAspect = true;
            coin.raycastTarget = false;
        }
        PlaceRateRect((RectTransform)btRate.transform, root, new Vector2(450, 115), new Vector2(0, -198.3333f));
        PlaceRateRect((RectTransform)btClose.transform, root, new Vector2(110, 110), new Vector2(410, 250));

        // These legacy containers no longer own any live controls or text.
        if (oldStarParent != root && oldStarParent != transform)
            oldStarParent.gameObject.SetActive(false);
        if (oldDescriptionParent != root && oldDescriptionParent != transform)
            oldDescriptionParent.gameObject.SetActive(false);
    }

    static void PlaceRateRect(RectTransform rect, RectTransform parent, Vector2 size,
        Vector2 position, bool resetScale = true)
    {
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one * 0.5f;
        rect.localRotation = Quaternion.identity;
        if (resetScale) rect.localScale = Vector3.one;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }
}
