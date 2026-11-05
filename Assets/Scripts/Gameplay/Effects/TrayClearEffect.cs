using System.Collections.Generic;
using UnityEngine;

// Timed geometry and particles rather than a prerecorded Clear animation.
public sealed class TrayClearEffect : MonoBehaviour
{
    public const float Duration = 1.55f;
    private const float Step = 0.085f;
    private const float BreakTime = 0.22f;
    private static Sprite tileSprite;
    private static Sprite[] colorTileSprites;
    private readonly List<Block> blocks = new List<Block>();
    private readonly List<Shard> shards = new List<Shard>();
    private SpriteRenderer title;
    private Vector3 titleOrigin;
    private float titleScale;
    private float spacing;
    private float elapsed;

    private sealed class Block
    {
        public SpriteRenderer face;
        public Vector3 position;
        public Color color;
        public float delay;
    }

    private sealed class Shard
    {
        public SpriteRenderer renderer;
        public Vector3 origin, velocity;
        public Color color;
        public float delay, size, spin;
    }

    public static void PlayImpact(Transform parent, Vector3 worldPosition, float worldSize)
    {
        EnsureSprites();
        var root = new GameObject("Match tile fragments");
        root.transform.SetParent(parent, false);
        root.transform.position = worldPosition;
        var effect = root.AddComponent<TrayClearEffect>();
        effect.spacing = parent.InverseTransformVector(Vector3.right * worldSize).magnitude;
        var random = new System.Random(157);
        for (int i = 0; i < 20; i++)
        {
            float angle = (float)random.NextDouble() * Mathf.PI * 2;
            float speed = (1f + (float)random.NextDouble() * 2.5f) * effect.spacing;
            effect.shards.Add(new Shard {
                renderer = effect.MakeSprite("White tile fragment", tileSprite, 44),
                origin = Vector3.zero,
                velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * speed,
                delay = 0, size = effect.spacing * (0.16f + (float)random.NextDouble() * 0.256f),
                spin = (float)random.NextDouble() * 720 - 360, color = Color.white
            });
        }
        effect.RenderAt(0);
    }

    public void Initialize(IList<Vector3> positions)
    {
        EnsureSprites();
        spacing = positions.Count > 1 ? Vector3.Distance(positions[0], positions[1]) : 1f;
        var colors = new[] {
            new Color(0.94f, 0.20f, 0.9f), new Color(0.04f, 0.7f, 0.98f),
            new Color(0.54f, 0.96f, 0.87f), new Color(0.76f, 0.96f, 0.28f),
            new Color(1f, 0.9f, 0.37f), new Color(1f, 0.96f, 0.58f),
            new Color(1f, 0.76f, 0.02f)
        };
        if (colorTileSprites == null)
        {
            colorTileSprites = new Sprite[colors.Length];
            for (int i = 0; i < colors.Length; i++)
                colorTileSprites[i] = CreateColorTileSprite(colors[i]);
        }
        var random = new System.Random(319);
        for (int i = 0; i < positions.Count; i++)
        {
            var position = positions[i];
            position.z = 0;
            var color = colors[i % colors.Length];
            var block = new Block {
                position = position, color = color, delay = i * Step,
                face = MakeSprite("Color tile", colorTileSprites[i % colors.Length], 41)
            };
            blocks.Add(block);
            block.face.transform.localPosition = position;
            // A 3 x 3 breakup, with additional small sparkles at the impact.
            for (int j = 0; j < 13; j++)
            {
                float x = (j % 3 - 1) * 0.24f;
                float y = (j / 3 - 1) * 0.24f;
                bool sparkle = j >= 9;
                var shard = new Shard {
                    renderer = MakeSprite(sparkle ? "Sparkle" : "Tile fragment", tileSprite, 44),
                    origin = position + new Vector3(x, sparkle ? 0 : y) * spacing,
                    velocity = new Vector3((float)random.NextDouble() * 3f - 2.3f,
                        (float)random.NextDouble() * 2.7f - 0.7f) * spacing,
                    delay = block.delay + BreakTime,
                    size = spacing * (sparkle ? 0.112f : 0.24f + (float)random.NextDouble() * 0.128f),
                    spin = (float)random.NextDouble() * 540f - 270f,
                    color = Color.Lerp(color, Color.white, sparkle ? 0.15f : 0.3f)
                };
                shards.Add(shard);
            }
        }
        titleOrigin = (positions[0] + positions[positions.Count - 1]) * 0.5f;
        titleOrigin.z = -0.1f;
        var titleImage = Resources.Load<Sprite>("ClearTitle");
        if (titleImage == null)
        {
            Debug.LogError("Missing clear title sprite: Resources/ClearTitle.png", this);
            return;
        }
        title = MakeSprite("CLEAR! artwork", titleImage, 61);
        titleScale = spacing * 2.4f / Mathf.Max(0.01f, titleImage.bounds.size.x);
        RenderAt(0);
    }

    private SpriteRenderer MakeSprite(string objectName, Sprite sprite, int order)
    {
        var child = new GameObject(objectName);
        child.transform.SetParent(transform, false);
        var renderer = child.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingLayerName = "Hovering";
        renderer.sortingOrder = order;
        return renderer;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        RenderAt(elapsed);
        if (elapsed >= Duration) Destroy(gameObject);
    }

    // Also allows deterministic frame rendering in the Editor.
    public void RenderAt(float time)
    {
        foreach (var block in blocks)
        {
            float age = time - block.delay;
            block.face.enabled = age >= 0 && age < BreakTime;
            float grow = 1f - Mathf.Pow(1f - Mathf.Clamp01(age / 0.09f), 3f);
            float pop = age < 0.09f ? Mathf.Lerp(0.12f, 1.08f, grow)
                : Mathf.Lerp(1.08f, 1f, Mathf.Clamp01((age - 0.09f) / 0.07f));
            // Taller than wide, with enough height to meet the tray walls.
            block.face.transform.localScale = new Vector3(1.05f, 1.22f, 1f) * spacing * pop;
        }
        foreach (var shard in shards)
        {
            float age = time - shard.delay;
            const float life = 0.48f;
            shard.renderer.enabled = age >= 0 && age < life;
            if (!shard.renderer.enabled) continue;
            shard.renderer.transform.localPosition = shard.origin + shard.velocity * age
                + Vector3.down * spacing * 3.5f * age * age;
            shard.renderer.transform.localRotation = Quaternion.Euler(0, 0, shard.spin * age);
            shard.renderer.transform.localScale = Vector3.one * shard.size * (1f - age / life * 0.65f);
            var color = shard.color;
            color.a = 1f - Mathf.Clamp01((age - 0.2f) / (life - 0.2f));
            shard.renderer.color = color;
        }
        if (title == null) return;
        float textAge = time - 0.22f;
        float appear = Mathf.Clamp01(textAge / 0.12f);
        float disappear = Mathf.Clamp01((time - 1.12f) / 0.35f);
        float bounce = Mathf.Sin(Mathf.Max(0, textAge) * 21f) * Mathf.Exp(-Mathf.Max(0, textAge) * 2.7f);
        float scale = titleScale * appear * (1f + bounce * 0.22f) * (1f - disappear * 0.62f);
        var position = titleOrigin + Vector3.up * spacing * bounce * 0.17f;
        var rotation = Quaternion.Euler(0, 0, bounce * 9f);
        title.transform.localPosition = position;
        title.transform.localRotation = rotation;
        title.transform.localScale = Vector3.one * scale;
        title.color = new Color(1f, 1f, 1f, textAge >= 0 ? 1f - disappear : 0f);
    }

    private static void EnsureSprites()
    {
        if (tileSprite != null) return;
        tileSprite = CreateSprite();
    }

    private static Sprite CreateSprite()
    {
        const int size = 96;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "Clear beveled tile";
        texture.wrapMode = TextureWrapMode.Clamp;
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            var point = new Vector2(Mathf.Abs((x + 0.5f) / size - 0.5f), Mathf.Abs((y + 0.5f) / size - 0.5f));
            var corner = new Vector2(Mathf.Max(0, point.x - 0.31f), Mathf.Max(0, point.y - 0.31f));
            float distance = corner.magnitude - 0.17f;
            float alpha = Mathf.Clamp01(-distance * size);
            float shade = Mathf.Lerp(0.72f, 1f, Mathf.Clamp01((float)y / size * 3f));
            if (distance > -0.035f && y > size * 0.55f) shade = 1.18f;
            pixels[y * size + x] = new Color(shade, shade, shade, alpha);
        }
        texture.SetPixels(pixels);
        texture.Apply(false, true);
        return Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
    }

    private static Sprite CreateColorTileSprite(Color color)
    {
        const int size = 128;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "Clear pastel tile";
        texture.wrapMode = TextureWrapMode.Clamp;
        var pixels = new Color[size * size];
        Color top = Color.Lerp(color, Color.white, 0.78f);
        Color foot = Color.Lerp(color, Color.black, 0.12f);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float height = (y + 0.5f) / size;
            var point = new Vector2(Mathf.Abs((x + 0.5f) / size - 0.5f), Mathf.Abs(height - 0.5f));
            var corner = new Vector2(Mathf.Max(0, point.x - 0.3f), Mathf.Max(0, point.y - 0.3f));
            float distance = corner.magnitude - 0.19f;
            // A saturated lower lip and a soft pastel face, as in the reference.
            float blend = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.08f, 0.92f, height));
            Color pixel = Color.Lerp(color, top, blend);
            if (height < 0.13f)
                pixel = Color.Lerp(foot, color, height / 0.13f);
            float rim = Mathf.Clamp01(1f - Mathf.Abs(distance + 0.018f) / 0.012f);
            pixel = Color.Lerp(pixel, Color.white, rim * 0.24f * Mathf.Clamp01((height - 0.35f) * 2f));
            pixel.a = Mathf.Clamp01(-distance * size);
            pixels[y * size + x] = pixel;
        }
        texture.SetPixels(pixels);
        texture.Apply(false, true);
        return Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
    }

}
