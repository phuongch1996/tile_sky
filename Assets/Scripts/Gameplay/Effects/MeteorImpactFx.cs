using DG.Tweening;
using UnityEngine;

public class MeteorImpactFx : MonoBehaviour
{
    const string RealisticMeteorResourcePath = "MeteorImpactRealistic";
    const float RealisticMeteorSourceAngle = -135f;
    const int MeteorTextureSize = 112;
    const int TrailTextureWidth = 288;
    const int TrailTextureHeight = 104;
    const float Match3DurationMultiplier = 7f / 8f;
    const float FallDuration = 0.32f * Match3DurationMultiplier;
    const float ImpactDuration = 0.08f * Match3DurationMultiplier;

    static Sprite meteorSprite;
    static Sprite trailSprite;
    static bool usesRealisticMeteorSprite;

    public static Tween Play(Vector3 targetPosition, Transform parent, string sortingLayerName, int sortingOrder, int sequenceIndex = 0)
    {
        GameObject effectObject = new("Meteor Impact Fx");
        if (parent != null)
            effectObject.transform.SetParent(parent, false);

        MeteorImpactFx effect = effectObject.AddComponent<MeteorImpactFx>();
        return effect.PlayInternal(targetPosition, sortingLayerName, sortingOrder, sequenceIndex);
    }

    Tween PlayInternal(Vector3 targetPosition, string sortingLayerName, int sortingOrder, int sequenceIndex)
    {
        EnsureSprites();

        Vector3 startOffset = GetStartOffset(sequenceIndex);
        transform.position = targetPosition + startOffset;
        Vector2 fallDirection = ((Vector2)(targetPosition - transform.position)).normalized;
        float fallAngle = Mathf.Atan2(fallDirection.y, fallDirection.x) * Mathf.Rad2Deg;

        if (usesRealisticMeteorSprite)
            return PlayRealisticMeteor(targetPosition, fallAngle, sortingLayerName, sortingOrder);

        transform.rotation = Quaternion.Euler(0f, 0f, fallAngle);

        GameObject trailObject = new("Meteor Trail");
        trailObject.transform.SetParent(transform, false);
        trailObject.transform.localPosition = new Vector3(-0.08f, 0f, 0f);
        trailObject.transform.localScale = new Vector3(1.5f, 0.95f, 1f);

        SpriteRenderer trailRenderer = trailObject.AddComponent<SpriteRenderer>();
        trailRenderer.sprite = trailSprite;
        trailRenderer.color = new Color(1f, 0.58f, 0.1f, 0.96f);
        trailRenderer.sortingLayerName = sortingLayerName;
        trailRenderer.sortingOrder = sortingOrder;

        GameObject coreTrailObject = new("Meteor White Heat Trail");
        coreTrailObject.transform.SetParent(transform, false);
        coreTrailObject.transform.localPosition = new Vector3(-0.04f, 0.01f, 0f);
        coreTrailObject.transform.localScale = new Vector3(1.28f, 0.38f, 1f);

        SpriteRenderer coreTrailRenderer = coreTrailObject.AddComponent<SpriteRenderer>();
        coreTrailRenderer.sprite = trailSprite;
        coreTrailRenderer.color = new Color(1f, 0.95f, 0.62f, 0.74f);
        coreTrailRenderer.sortingLayerName = sortingLayerName;
        coreTrailRenderer.sortingOrder = sortingOrder + 1;

        GameObject meteorObject = new("Meteor");
        meteorObject.transform.SetParent(transform, false);
        meteorObject.transform.localScale = new Vector3(0.74f, 0.68f, 1f);
        meteorObject.transform.localRotation = Quaternion.Euler(0f, 0f, -fallAngle);

        SpriteRenderer meteorRenderer = meteorObject.AddComponent<SpriteRenderer>();
        meteorRenderer.sprite = meteorSprite;
        meteorRenderer.color = Color.white;
        meteorRenderer.sortingLayerName = sortingLayerName;
        meteorRenderer.sortingOrder = sortingOrder + 2;

        ParticleSystem impactParticle = TileOnRuntimeFx.CreateBurstParticle(
            transform,
            "Meteor Impact Sparks",
            new Color(1f, 0.48f, 0.08f, 1f),
            36,
            0.5f,
            0.3f,
            0.04f,
            0.14f,
            2.45f,
            sortingLayerName,
            sortingOrder + 3);
        impactParticle.transform.localPosition = Vector3.zero;

        Sequence sequence = DOTween.Sequence()
            .Append(transform.DOMove(targetPosition, FallDuration).SetEase(Ease.InQuad))
            .Join(transform.DOScale(1.08f, FallDuration).SetEase(Ease.OutQuad))
            .Join(trailRenderer.DOFade(0.78f, FallDuration * 0.65f).SetLoops(2, LoopType.Yoyo))
            .Join(coreTrailRenderer.DOFade(0.52f, FallDuration * 0.55f).SetLoops(2, LoopType.Yoyo))
            .Join(meteorObject.transform.DORotate(new Vector3(0f, 0f, -fallAngle - 95f), FallDuration, RotateMode.FastBeyond360).SetEase(Ease.Linear))
            .AppendCallback(() =>
            {
                meteorRenderer.enabled = false;
                trailRenderer.enabled = false;
                coreTrailRenderer.enabled = false;
                impactParticle.Play(true);
            })
            .AppendInterval(ImpactDuration)
            .OnComplete(() => Destroy(gameObject))
            .SetTarget(gameObject);

        return sequence;
    }

    Tween PlayRealisticMeteor(Vector3 targetPosition, float fallAngle, string sortingLayerName, int sortingOrder)
    {
        transform.rotation = Quaternion.Euler(0f, 0f, fallAngle - RealisticMeteorSourceAngle);
        transform.localScale = Vector3.one * 0.82f;

        GameObject meteorObject = new("Realistic Meteor");
        meteorObject.transform.SetParent(transform, false);

        SpriteRenderer meteorRenderer = meteorObject.AddComponent<SpriteRenderer>();
        meteorRenderer.sprite = meteorSprite;
        meteorRenderer.color = new Color(1f, 1f, 1f, 0f);
        meteorRenderer.sortingLayerName = sortingLayerName;
        meteorRenderer.sortingOrder = sortingOrder + 2;

        ParticleSystem impactParticle = TileOnRuntimeFx.CreateBurstParticle(
            transform,
            "Meteor Impact Sparks",
            new Color(1f, 0.48f, 0.08f, 1f),
            48,
            0.55f,
            0.34f,
            0.05f,
            0.18f,
            2.85f,
            sortingLayerName,
            sortingOrder + 3);
        impactParticle.transform.localPosition = Vector3.zero;

        Sequence sequence = DOTween.Sequence()
            .Append(transform.DOMove(targetPosition, FallDuration).SetEase(Ease.InSine))
            .Join(transform.DOScale(1.08f, FallDuration).SetEase(Ease.OutSine))
            .Join(meteorRenderer.DOFade(0.98f, 0.06f).SetEase(Ease.OutSine))
            .AppendCallback(() =>
            {
                meteorRenderer.enabled = false;
                impactParticle.Play(true);
            })
            .AppendInterval(ImpactDuration)
            .OnComplete(() => Destroy(gameObject))
            .SetTarget(gameObject);

        return sequence;
    }

    static Vector3 GetStartOffset(int sequenceIndex)
    {
        float horizontal = -2.35f + sequenceIndex * 0.08f;
        return new Vector3(horizontal, 3.85f, 0f);
    }

    static void EnsureSprites()
    {
        if (meteorSprite == null)
        {
            meteorSprite = Resources.Load<Sprite>(RealisticMeteorResourcePath);
            usesRealisticMeteorSprite = meteorSprite != null;
        }

        if (usesRealisticMeteorSprite)
            return;

        if (meteorSprite == null)
            meteorSprite = CreateMeteorSprite();
        if (trailSprite == null)
            trailSprite = CreateTrailSprite();
    }

    static Sprite CreateMeteorSprite()
    {
        Texture2D texture = new(MeteorTextureSize, MeteorTextureSize, TextureFormat.RGBA32, false)
        {
            name = "Runtime Realistic Meteor Texture",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        Vector2 center = new((MeteorTextureSize - 1) * 0.5f, (MeteorTextureSize - 1) * 0.5f);
        for (int y = 0; y < MeteorTextureSize; y++)
        {
            for (int x = 0; x < MeteorTextureSize; x++)
            {
                Vector2 delta = new(x - center.x, y - center.y);
                float distance = delta.magnitude / (MeteorTextureSize * 0.5f);
                float noise = Mathf.PerlinNoise(x * 0.12f, y * 0.12f);
                float rockMask = 1f - Mathf.SmoothStep(0.5f + noise * 0.08f, 0.72f + noise * 0.08f, distance);
                float flameMask = Mathf.Clamp01(1f - Mathf.SmoothStep(0.58f + noise * 0.08f, 0.96f, distance));

                Color rock = Color.Lerp(new Color(0.12f, 0.08f, 0.06f, 1f), new Color(0.42f, 0.3f, 0.22f, 1f), noise);
                Color ember = Color.Lerp(new Color(0.86f, 0.08f, 0.01f, 1f), new Color(1f, 0.9f, 0.32f, 1f), Mathf.Clamp01((distance - 0.4f) * 3.4f + noise * 0.45f));
                Color color = Color.Lerp(ember, rock, rockMask);
                color.a = Mathf.Max(rockMask, flameMask * 0.92f);

                if (delta.x < -MeteorTextureSize * 0.08f && distance < 0.46f)
                    color = Color.Lerp(color, new Color(1f, 0.82f, 0.28f, 1f), 0.34f);
                if (distance < 0.2f)
                    color = Color.Lerp(color, new Color(0.08f, 0.055f, 0.04f, 1f), 0.55f);

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, MeteorTextureSize, MeteorTextureSize), new Vector2(0.5f, 0.5f), 96f, 0, SpriteMeshType.FullRect);
    }

    static Sprite CreateTrailSprite()
    {
        Texture2D texture = new(TrailTextureWidth, TrailTextureHeight, TextureFormat.RGBA32, false)
        {
            name = "Runtime Meteor Trail Texture",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        Vector2 center = new(TrailTextureWidth * 0.9f, TrailTextureHeight * 0.5f);
        for (int y = 0; y < TrailTextureHeight; y++)
        {
            for (int x = 0; x < TrailTextureWidth; x++)
            {
                float forward = Mathf.InverseLerp(0f, TrailTextureWidth - 1f, x);
                float noise = Mathf.PerlinNoise(x * 0.035f, y * 0.16f);
                float wave = Mathf.Sin(forward * 17f + noise * 2.5f) * (1f - forward) * 0.08f;
                float vertical = Mathf.Abs((y - center.y) / TrailTextureHeight - wave);
                float width = Mathf.Lerp(0.018f, 0.29f, Mathf.Pow(forward, 1.55f));
                float body = 1f - Mathf.SmoothStep(width, width + 0.12f, vertical);
                float hotCore = 1f - Mathf.SmoothStep(width * 0.22f, width * 0.22f + 0.035f, vertical);
                float fade = Mathf.SmoothStep(0.0f, 0.72f, forward);
                float tongue = Mathf.Clamp01(Mathf.PerlinNoise(x * 0.075f, y * 0.07f) * 1.5f - 0.35f);
                float alpha = Mathf.Clamp01((body * 0.7f + hotCore * 0.65f) * fade * (0.7f + tongue * 0.42f));

                Color ember = Color.Lerp(new Color(0.72f, 0.04f, 0.005f, 1f), new Color(1f, 0.38f, 0.02f, 1f), forward);
                Color heat = Color.Lerp(ember, new Color(1f, 0.93f, 0.42f, 1f), Mathf.Clamp01(hotCore + forward * 0.32f));
                Color color = Color.Lerp(ember, heat, Mathf.Clamp01(hotCore * 1.4f + noise * 0.18f));
                color.a = alpha * 0.95f;
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, TrailTextureWidth, TrailTextureHeight), new Vector2(0.9f, 0.5f), 96f, 0, SpriteMeshType.FullRect);
    }
}
