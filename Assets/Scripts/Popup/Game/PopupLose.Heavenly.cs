using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class PopupLose
{
    RectTransform heavenlyLose;
    readonly List<Sprite> heavenlySprites = new();

    // Backgrounds and button faces are separate sprites so the complete button can animate.
    void EnsureHeavenlyLose()
    {
        if (heavenlyLose) return;
        Texture2D atlas = Resources.Load<Texture2D>("HeavenlyLose/PopupAtlas");
        Texture2D controls = Resources.Load<Texture2D>("HeavenlyLose/SeparatedControls");
        if (!atlas || !controls) return;

        Transform safeArea = lose1.transform.parent;
        foreach (Transform child in safeArea) child.gameObject.SetActive(false);
        heavenlyLose = LoseRect("Heavenly lose", safeArea, new Vector2(1000, 1500), Vector2.zero);
        var revive = LoseRect("Revive", heavenlyLose, new Vector2(1000, 1500), Vector2.zero);
        var restart = LoseRect("Lose", heavenlyLose, new Vector2(1000, 1000), Vector2.zero);
        var controlsSize = new Vector2(1246, 1263);
        Sprite timer = LoseSlice(controls, new Rect(664, 716, 512, 534), controlsSize);
        Sprite mascot = LoseSlice(atlas, new Rect(650, 60, 580, 500));
        Sprite revivePanel = LoseSlice(controls, new Rect(0, 0, 624, 410), controlsSize);
        Sprite restartPanel = LoseSlice(controls, new Rect(633, 0, 613, 410), controlsSize);
        Sprite watchAd = LoseSlice(controls, new Rect(8, 463, 612, 241), controlsSize);
        Sprite coins = LoseSlice(controls, new Rect(625, 463, 621, 241), controlsSize);
        Sprite restartButton = LoseSlice(controls, new Rect(0, 832, 640, 256), controlsSize);

        var clock = LoseArt("Countdown", revive, timer, new Vector2(560, 584), new Vector2(0, 470));
        // Fill the cyan ring over the dark track, starting at twelve o'clock.
        imgCounterFill.transform.SetParent(clock, false);
        LosePlace(imgCounterFill.rectTransform, new Vector2(520, 520), new Vector2(-4, 12));
        imgCounterFill.type = Image.Type.Filled;
        imgCounterFill.fillMethod = Image.FillMethod.Radial360;
        imgCounterFill.fillOrigin = (int)Image.Origin360.Top;
        imgCounterFill.fillClockwise = true;
        imgCounterFill.fillAmount = 0;
        imgCounterFill.color = Color.white;
        imgCounterFill.raycastTarget = false;
        imgCounterFill.gameObject.SetActive(true);
        // Keep the foreground clouds in front of the moving arc.
        LoseArt("Countdown clouds", clock,
            LoseSlice(controls, new Rect(698, 1075, 444, 175), controlsSize),
            new Vector2(485.625f, 191.406f), new Vector2(0, -196.48f));
        txTimeCounter.transform.SetParent(clock, false);
        LosePlace(txTimeCounter.rectTransform, new Vector2(300, 280), new Vector2(0, 5));
        txTimeCounter.fontSize = 225;
        txTimeCounter.alignment = TextAlignmentOptions.Center;
        txTimeCounter.color = Color.white;
        txTimeCounter.raycastTarget = false;
        txTimeCounter.gameObject.SetActive(true);
        fxTfm.gameObject.SetActive(false);

        LoseArt("Crying star", revive, mascot, new Vector2(450, 388), new Vector2(0, -10));
        var offer = LoseArt("Revive panel", revive, revivePanel, new Vector2(960, 607), new Vector2(0, -470));
        LoseButton(btReviveAds, offer, watchAd, new Vector2(400, 158), new Vector2(-212, -35));
        LoseButton(btReviveGold, offer, coins, new Vector2(400, 158), new Vector2(207, -35));

        restart.localScale = Vector3.one * 0.82f;
        LoseArt("Crying star", restart, mascot, new Vector2(570, 491), new Vector2(0, 235));
        var retry = LoseArt("Restart panel", restart, restartPanel, new Vector2(960, 622), new Vector2(0, -255));
        LoseButton(btRestart, retry, restartButton, new Vector2(730, 292), new Vector2(0, -33));
        // Restart only from the visible button, as in the supplied art.
        btRestartBlank.gameObject.SetActive(false);

        canRevive = lose1 = revive.gameObject;
        noRevive = lose2 = restart.gameObject;
        var shade = panel.GetComponent<Image>();
        if (shade) shade.color = new Color(0, 0, 0, 0.6f);
        FitHeavenlyLose();
    }

    Sprite LoseSlice(Texture2D atlas, Rect source, Vector2 sourceSize = default)
    {
        if (sourceSize == Vector2.zero) sourceSize = new Vector2(1254, 1254);
        // Source coordinates are measured from the top left of the source artboard.
        var rect = new Rect(source.x / sourceSize.x * atlas.width,
            (sourceSize.y - source.y - source.height) / sourceSize.y * atlas.height,
            source.width / sourceSize.x * atlas.width, source.height / sourceSize.y * atlas.height);
        var sprite = Sprite.Create(atlas, rect, new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect);
        heavenlySprites.Add(sprite);
        return sprite;
    }

    static RectTransform LoseRect(string title, Transform parent, Vector2 size, Vector2 position)
    {
        var rect = new GameObject(title, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.gameObject.layer = parent.gameObject.layer;
        rect.SetParent(parent, false);
        LosePlace(rect, size, position);
        return rect;
    }

    static void LosePlace(RectTransform rect, Vector2 size, Vector2 position)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    static RectTransform LoseArt(string title, Transform parent, Sprite sprite, Vector2 size, Vector2 position)
    {
        var rect = LoseRect(title, parent, size, position);
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = false;
        return rect;
    }

    static void LoseButton(Button button, Transform parent, Sprite sprite, Vector2 size, Vector2 position)
    {
        button.transform.SetParent(parent, false);
        LosePlace((RectTransform)button.transform, size, position);
        foreach (Transform child in button.transform) child.gameObject.SetActive(false);
        button.image.sprite = sprite;
        button.image.type = Image.Type.Simple;
        button.image.preserveAspect = true;
        button.image.color = Color.white;
        button.image.raycastTarget = true;
        button.targetGraphic = button.image;
        button.transition = Selectable.Transition.ColorTint;
        var colors = ColorBlock.defaultColorBlock;
        colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1);
        button.colors = colors;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        var press = button.GetComponent<ButtonAnim>();
        if (!press) press = button.gameObject.AddComponent<ButtonAnim>();
        press.tfm = button.transform;
        press.bt = button;
        press.scale = 1;
        press.enabled = true;
        button.gameObject.SetActive(true);
    }

    void FitHeavenlyLose()
    {
        if (!heavenlyLose) return;
        Rect available = ((RectTransform)heavenlyLose.parent).rect;
        float scale = Mathf.Min(available.width / 1100f, available.height / 1800f);
        heavenlyLose.localScale = Vector3.one * Mathf.Max(0.01f, Mathf.Min(0.9f, scale));
        heavenlyLose.anchoredPosition = new Vector2(0, available.height * 0.05f);
    }

    void OnRectTransformDimensionsChange() => FitHeavenlyLose();

    void OnDestroy()
    {
        foreach (var sprite in heavenlySprites)
            if (sprite)
            {
                if (Application.isPlaying) Destroy(sprite);
                else DestroyImmediate(sprite);
            }
    }
}
