using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class PopupFreeGold
{
    RectTransform freeGoldArt;
    Sprite freeGoldCapsule;
    Texture2D freeGoldCapsuleTexture;
    Material freeGoldTextMaterial;
    readonly List<Sprite> freeGoldButtonSprites = new List<Sprite>();

    void EnsureHeavenlyFreeGoldUI()
    {
        if (freeGoldArt) return;
        var sprite = Resources.Load<Sprite>("HeavenlyFreeGold/PanelClean-v2");
        var buttons = Resources.Load<Texture2D>("HeavenlyFreeGold/Buttons");
        if (!sprite || !buttons) return;

        var oldBox = box;
        var wrapper = FreeGoldRect("Free Gold animation", oldBox.parent, Vector2.zero, Vector2.zero);
        wrapper.anchorMin = Vector2.zero;
        wrapper.anchorMax = Vector2.one;
        wrapper.offsetMin = wrapper.offsetMax = Vector2.zero;
        freeGoldArt = FreeGoldRect("Heavenly Free Gold", wrapper, new Vector2(1168, 1360), Vector2.zero);
        var panelArt = freeGoldArt.gameObject.AddComponent<Image>();
        panelArt.sprite = sprite;
        // Block clicks inside the panel from reaching the outside-dismiss button.
        panelArt.raycastTarget = true;

        MoveFreeGoldButton(btClose, new Vector2(150, 155), new Vector2(480, 545),
            FreeGoldButtonSprite(buttons, new Rect(402, 62, 446, 436)));
        MoveFreeGoldButton(btWatchAds, new Vector2(850, 205), new Vector2(0, -415),
            FreeGoldButtonSprite(buttons, new Rect(8, 647, 1238, 322)));

        // Live progress layers, independent of the decorative panel bitmap.
        FreeGoldBarLayer("Bar shadow", new Vector2(778, 86), new Vector2(0, -252), new Color32(35, 79, 172, 255));
        FreeGoldBarLayer("Bar white rim", new Vector2(770, 80), new Vector2(0, -246), Color.white);
        FreeGoldBarLayer("Bar gold rim", new Vector2(758, 70), new Vector2(0, -246), new Color32(255, 185, 0, 255));
        FreeGoldBarLayer("Bar inner rim", new Vector2(746, 58), new Vector2(0, -246), Color.white);
        FreeGoldBarLayer("Bar track", new Vector2(734, 49), new Vector2(0, -246), new Color32(0, 115, 40, 255));

        fillImage.transform.SetParent(freeGoldArt, false);
        foreach (Transform child in fillImage.transform) child.gameObject.SetActive(false);
        PlaceFreeGoldRect(fillImage.rectTransform, new Vector2(728, 43), new Vector2(0, -244));
        fillImage.gameObject.SetActive(true);
        fillImage.color = Color.white;
        fillImage.raycastTarget = false;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = 0;

        txFill.transform.SetParent(freeGoldArt, false);
        PlaceFreeGoldRect(txFill.rectTransform, new Vector2(250, 90), new Vector2(0, -244));
        txFill.gameObject.SetActive(true);
        freeGoldTextMaterial = new Material(txFill.fontSharedMaterial);
        txFill.fontMaterial = freeGoldTextMaterial;
        txFill.fontSize = 72;
        txFill.enableAutoSizing = false;
        txFill.color = Color.white;
        txFill.outlineColor = new Color32(25, 52, 145, 255);
        txFill.outlineWidth = 0.25f;
        txFill.alignment = TextAlignmentOptions.Center;
        txFill.raycastTarget = false;

        goldsTfm.SetParent(freeGoldArt, false);
        goldsTfm.localPosition = new Vector3(0, 235, 0);
        // Only the coin-flight origin is retained from the old illustration.
        foreach (var graphic in goldsTfm.GetComponentsInChildren<Graphic>(true)) graphic.enabled = false;
        oldBox.gameObject.SetActive(false);
        box = wrapper;
        FitFreeGoldUI();
    }

    Sprite FreeGoldButtonSprite(Texture2D atlas, Rect pixelRegion)
    {
        var sprite = Sprite.Create(atlas, pixelRegion, Vector2.one * 0.5f, 100, 0, SpriteMeshType.FullRect);
        freeGoldButtonSprites.Add(sprite);
        return sprite;
    }

    void MoveFreeGoldButton(Button button, Vector2 size, Vector2 position, Sprite sprite)
    {
        button.transform.SetParent(freeGoldArt, false);
        PlaceFreeGoldRect((RectTransform)button.transform, size, position);
        foreach (Transform child in button.transform) child.gameObject.SetActive(false);
        var image = button.GetComponent<Image>();
        if (!image) image = button.gameObject.AddComponent<Image>();
        image.enabled = true;
        // Real transparent button art moves over a continuous, uncut panel.
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.preserveAspect = true;
        image.color = Color.white;
        image.raycastTarget = true;
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        var animation = button.GetComponent<ButtonAnim>();
        if (!animation) animation = button.gameObject.AddComponent<ButtonAnim>();
        animation.tfm = button.transform;
        animation.bt = button;
        animation.scale = 1;
        animation.enabled = true;
        button.gameObject.SetActive(true);
    }

    void LateUpdate() => FitFreeGoldUI();

    void FitFreeGoldUI()
    {
        if (!freeGoldArt) return;
        var parent = (RectTransform)freeGoldArt.parent;
        float scale = Mathf.Min(parent.rect.width / 1168f, parent.rect.height / 1360f) * 0.94f;
        freeGoldArt.localScale = Vector3.one * scale;
    }

    Image FreeGoldBarLayer(string label, Vector2 size, Vector2 position, Color color)
    {
        if (!freeGoldCapsule)
        {
            freeGoldCapsuleTexture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            freeGoldCapsuleTexture.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < 64; y++)
                for (int x = 0; x < 64; x++)
                {
                    float alpha = Mathf.Clamp01(32 - Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(32, 32)));
                    freeGoldCapsuleTexture.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
            freeGoldCapsuleTexture.Apply();
            freeGoldCapsule = Sprite.Create(freeGoldCapsuleTexture, new Rect(0, 0, 64, 64), Vector2.one * 0.5f,
                100, 0, SpriteMeshType.FullRect, new Vector4(32, 32, 32, 32));
        }
        var image = FreeGoldRect(label, freeGoldArt, size, position).gameObject.AddComponent<Image>();
        image.sprite = freeGoldCapsule;
        image.type = Image.Type.Sliced;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    static RectTransform FreeGoldRect(string label, Transform parent, Vector2 size, Vector2 position)
    {
        var rect = new GameObject(label, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.gameObject.layer = parent.gameObject.layer;
        rect.SetParent(parent, false);
        PlaceFreeGoldRect(rect, size, position);
        return rect;
    }

    static void PlaceFreeGoldRect(RectTransform rect, Vector2 size, Vector2 position)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one * 0.5f;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    void OnDestroy()
    {
        ReleaseFreeGoldAsset(freeGoldCapsule);
        ReleaseFreeGoldAsset(freeGoldCapsuleTexture);
        ReleaseFreeGoldAsset(freeGoldTextMaterial);
        foreach (var sprite in freeGoldButtonSprites) ReleaseFreeGoldAsset(sprite);
    }

    static void ReleaseFreeGoldAsset(Object asset)
    {
        if (!asset) return;
        if (Application.isPlaying) Destroy(asset);
        else DestroyImmediate(asset);
    }
}
