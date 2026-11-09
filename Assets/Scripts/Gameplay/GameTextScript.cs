using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameTextScript : MonoBehaviour
{
    // Retain the serialized legacy references for existing scenes and prefabs.
    [HideInInspector] public List<PoolObject> textsGOList;
    [SerializeField] TMP_FontAsset comboFont;
    [SerializeField] string[] comboLabels =
    {
        "Breezy!", "Soaring!", "Brilliant!", "Stellar!", "Sky High!", "Heavenly!"
    };
    [SerializeField] string textSortingLayer = "Hovering";
    [SerializeField] int textSortingOrder = 6000;
    readonly List<ComboEffect> effects = new();
    Material burstMaterial;
    int effectOrder;

    sealed class ComboEffect
    {
        public TextMeshPro text;
        public float elapsed;
        public int tier;
        public float duration;
        public Mesh burst;
        public Vector3[] vertices;
        public Color[] colors;
        public MeshRenderer burstRenderer;
        public float width;
    }
    readonly int timeInterval = 3;
    float timer;
    int index = 0;
    public static int comboCount;
    int combo = 0;
    public static Action<int> OnComboCounter;
    public int ShowText()
    {
        if (index <= 0)
        {
            index = 1;
            timer = timeInterval;
            combo = 0;
            return 0;
        }
        combo += 1;
        comboCount += combo;
        OnComboCounter?.Invoke(comboCount);
        int tier = index <= 4 ? index - 1 : (index % 2 == 0 ? 5 : 4);
        ShowSkyText(tier);
        index++;
        timer = timeInterval;
        return combo;
    }

    void ShowSkyText(int tier)
    {
        ComboEffect effect = effects.Find(item => !item.text.gameObject.activeSelf);
        if (effect == null)
        {
            var go = new GameObject("Sky combo", typeof(TextMeshPro));
            go.transform.SetParent(transform, false);
            var label = go.GetComponent<TextMeshPro>();
            label.font = comboFont != null ? comboFont : TMP_Settings.defaultFontAsset;
            label.fontSize = 7.5f;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.rectTransform.sizeDelta = new Vector2(8f, 1.6f);
            label.raycastTarget = false;
            label.enableVertexGradient = true;
            label.outlineWidth = 0.22f;
            // Existing font materials may contain a colored outline texture.
            if (label.fontMaterial.HasProperty("_OutlineTex"))
                label.fontMaterial.SetTexture("_OutlineTex", Texture2D.whiteTexture);
            effect = new ComboEffect { text = label };
            CreateBurst(effect);
            effects.Add(effect);
        }

        var text = effect.text;
        text.text = comboLabels[tier];
        effect.tier = tier;
        effect.duration = 1.4f + tier * 0.12f;
        ApplyComboColors(text, tier);
        text.renderer.sortingLayerName = textSortingLayer;
        // Bounded ordering keeps long combo runs within Unity's sorting range.
        text.renderer.sortingOrder = textSortingOrder + (effectOrder++ % 100) * 2 + 1;
        effect.burstRenderer.sortingLayerName = textSortingLayer;
        effect.burstRenderer.sortingOrder = text.renderer.sortingOrder - 1;
        effect.burstRenderer.enabled = tier >= 2;
        text.ForceMeshUpdate(true);
        effect.width = Mathf.Max(1.5f, text.preferredWidth * 0.5f);
        effect.elapsed = 0;
        text.alpha = 0;
        text.transform.localPosition = Vector3.zero;
        text.transform.localScale = Vector3.one * 0.65f;
        text.transform.localRotation = Quaternion.identity;
        text.gameObject.SetActive(true);
        AnimateBurst(effect, 0);
    }

    void CreateBurst(ComboEffect effect)
    {
        const int stars = 15;
        const int segments = 48;
        var go = new GameObject("Combo starburst", typeof(MeshFilter), typeof(MeshRenderer));
        go.transform.SetParent(effect.text.transform, false);
        effect.burst = new Mesh { name = "Combo sparkle mesh" };
        effect.burst.MarkDynamic();
        // Eight outline vertices and a center per star, then three rings for a soft halo.
        effect.vertices = new Vector3[stars * 9 + segments * 3];
        effect.colors = new Color[effect.vertices.Length];
        var triangles = new List<int>();
        for (int star = 0; star < stars; star++)
            for (int corner = 0; corner < 8; corner++)
            {
                triangles.Add(star * 9 + 8);
                triangles.Add(star * 9 + corner);
                triangles.Add(star * 9 + (corner + 1) % 8);
            }
        int start = stars * 9;
        for (int segment = 0; segment < segments; segment++)
            for (int band = 0; band < 2; band++)
            {
                int a = start + segment * 3 + band;
                int b = start + (segment + 1) % segments * 3 + band;
                triangles.Add(a); triangles.Add(b); triangles.Add(a + 1);
                triangles.Add(a + 1); triangles.Add(b); triangles.Add(b + 1);
            }
        effect.burst.vertices = effect.vertices;
        effect.burst.triangles = triangles.ToArray();
        go.GetComponent<MeshFilter>().sharedMesh = effect.burst;
        if (!burstMaterial) burstMaterial = new Material(Shader.Find("Sprites/Default"));
        effect.burstRenderer = go.GetComponent<MeshRenderer>();
        effect.burstRenderer.sharedMaterial = burstMaterial;
    }

    static void AnimateBurst(ComboEffect effect, float alpha)
    {
        if (effect.tier < 2) return;
        float age = effect.elapsed;
        int count = effect.tier * 3;
        Color tint = effect.text.colorGradient.bottomLeft;
        for (int star = 0; star < 15; star++)
        {
            float angle = star * Mathf.PI * 2 / count + 0.2f;
            float travel = 1 - Mathf.Exp(-age * 5);
            var center = new Vector3(Mathf.Cos(angle) * (effect.width + 0.3f + travel * 0.5f),
                Mathf.Sin(angle) * (0.45f + travel * (0.4f + effect.tier * 0.06f)), 0);
            float twinkle = 0.65f + 0.35f * Mathf.Sin(age * 13 + star * 2);
            float size = (0.09f + effect.tier * 0.025f) * twinkle;
            Color color = star % 2 == 0 ? new Color(1, 0.95f, 0.55f) : Color.Lerp(tint, Color.white, 0.6f);
            color.a = star < count ? alpha * Mathf.Clamp01(age / 0.12f) : 0;
            effect.vertices[star * 9 + 8] = center;
            effect.colors[star * 9 + 8] = color;
            for (int corner = 0; corner < 8; corner++)
            {
                float direction = corner * Mathf.PI / 4 + age * (star % 2 == 0 ? 0.6f : -0.6f);
                float radius = size * (corner % 2 == 0 ? 1 : 0.23f);
                effect.vertices[star * 9 + corner] = center + new Vector3(Mathf.Cos(direction), Mathf.Sin(direction), 0) * radius;
                effect.colors[star * 9 + corner] = color;
            }
        }
        float expansion = 1 - Mathf.Exp(-age * 7);
        for (int segment = 0; segment < 48; segment++)
            for (int band = 0; band < 3; band++)
            {
                float angle = segment * Mathf.PI * 2 / 48;
                float radius = (0.6f + expansion * 0.65f) * (0.94f + band * 0.06f);
                int vertex = 135 + segment * 3 + band;
                effect.vertices[vertex] = new Vector3(Mathf.Cos(angle) * effect.width * radius,
                    Mathf.Sin(angle) * 0.65f * radius, 0);
                Color color = Color.Lerp(tint, Color.white, 0.45f);
                color.a = band == 1 ? alpha * Mathf.Exp(-age * 2.5f) * (0.35f + effect.tier * 0.09f) : 0;
                effect.colors[vertex] = color;
            }
        effect.burst.vertices = effect.vertices;
        effect.burst.colors = effect.colors;
        effect.burst.RecalculateBounds();
    }

    static void ApplyComboColors(TextMeshPro text, int tier)
    {
        // Give each tier its own vivid highlight, saturated base and dark outline.
        (Color32 top, Color32 bottom, Color32 outline) = tier switch
        {
            0 => (new Color32(157, 255, 222, 255), new Color32(0, 205, 238, 255), new Color32(10, 83, 145, 255)),
            1 => (new Color32(231, 255, 115, 255), new Color32(66, 225, 100, 255), new Color32(15, 103, 96, 255)),
            2 => (new Color32(255, 245, 116, 255), new Color32(255, 153, 35, 255), new Color32(156, 63, 36, 255)),
            3 => (new Color32(255, 178, 245, 255), new Color32(228, 65, 214, 255), new Color32(103, 34, 155, 255)),
            4 => (new Color32(176, 237, 255, 255), new Color32(130, 100, 255, 255), new Color32(65, 36, 148, 255)),
            _ => (new Color32(255, 246, 140, 255), new Color32(255, 102, 153, 255), new Color32(131, 43, 143, 255))
        };
        text.color = Color.white;
        text.outlineColor = outline;
        text.colorGradient = tier == 5
            ? new VertexGradient(top, new Color32(153, 255, 242, 255), bottom, new Color32(176, 119, 255, 255))
            : new VertexGradient(top, top, bottom, bottom);
    }

    void AnimateSkyTexts()
    {
        foreach (var effect in effects)
        {
            var text = effect.text;
            if (!text.gameObject.activeSelf) continue;
            effect.elapsed += Time.deltaTime;
            float age = effect.elapsed;
            if (age >= effect.duration)
            {
                text.gameObject.SetActive(false);
                continue;
            }
            float pop = Mathf.Clamp01(age / (0.24f + effect.tier * 0.015f));
            float peak = 1.08f + effect.tier * 0.045f;
            float scale = pop < 0.7f
                ? Mathf.Lerp(0.65f, peak, Mathf.Sin(pop / 0.7f * Mathf.PI * 0.5f))
                : Mathf.Lerp(peak, 1f, (pop - 0.7f) / 0.3f);
            scale *= 1 + effect.tier * 0.065f;
            if (effect.tier >= 3)
                scale *= 1 + 0.025f * Mathf.Sin(age * 9) * Mathf.Clamp01((age - 0.3f) * 4);
            text.transform.localScale = Vector3.one * scale;
            text.transform.localRotation = Quaternion.Euler(0, 0,
                Mathf.Sin(age * 16) * effect.tier * 0.9f * Mathf.Exp(-age * 4));
            text.transform.localPosition = Vector3.up * (age * (0.45f + effect.tier * 0.035f));
            text.alpha = Mathf.Clamp01(age / 0.1f) * Mathf.Clamp01((effect.duration - age) / 0.4f);
            AnimateBurst(effect, text.alpha);
        }
    }

    void OnDisable()
    {
        foreach (var effect in effects)
            if (effect.text != null) effect.text.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        foreach (var effect in effects)
            if (effect.burst) Destroy(effect.burst);
        if (burstMaterial) Destroy(burstMaterial);
    }
    private void Update()
    {
        AnimateSkyTexts();
        if (!GameConfig.isGameLoaded)
            return;
        if (GameController.instance.gameState != GAME_STATE.PLAYING)
            return;
        if (index != 0 && timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            index = 0;
        }
    }
}
