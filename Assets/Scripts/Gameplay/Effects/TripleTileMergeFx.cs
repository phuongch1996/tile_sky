using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TripleTileMergeFx : MonoBehaviour
{
    const int BlackHoleTextureSize = 192;
    const int FragmentSortingOffset = 80;
    const int BlackHoleBaseSortingOrder = 40;
    const int HintTileSortingOrder = 90;
    const int MinFragmentsPerTile = 56;
    const int MaxFragmentsPerTileExclusive = 85;
    const float FragmentBurstRangeMultiplier = 4f;
    const float Match3DurationMultiplier = 7f / 8f;

    [SerializeField] private Transform mergeContainer;
    [SerializeField] private ParticleSystem boomParticle;
    [SerializeField] private ParticleSystem boomParticlePrefab;
    [SerializeField] private float glowScale = 1.3f;
    [SerializeField] private float glowDuration = 0.15f * Match3DurationMultiplier;
    [SerializeField] private float popScale = 1.15f;
    [SerializeField] private float popDuration = 0.12f * Match3DurationMultiplier;
    [SerializeField] private float blackHoleAppearDuration = 0.32f * Match3DurationMultiplier;
    [SerializeField] private float spiralDuration = 0.95f * Match3DurationMultiplier;
    [SerializeField] private float blackHoleBreakDuration = 0.72f * Match3DurationMultiplier;
    [SerializeField] private float turns = 2.55f;
    [SerializeField] private float blackHoleWorldSize = 2.45f;
    [SerializeField, Range(0f, 1f)] private float vanishStart = 0.52f;
    [SerializeField] private Ease spiralEase = Ease.InCubic;

    private Sequence sequence;
    private Transform blackHoleRoot;
    private SpriteRenderer blackHoleCore;
    private SpriteRenderer blackHoleRingA;
    private SpriteRenderer blackHoleRingB;
    private Sprite blackHoleCoreSprite;
    private Sprite blackHoleRingSprite;
    private readonly List<GameObject> runtimeObjects = new();
    private readonly List<Sprite> runtimeSprites = new();
    private readonly List<Tween> runtimeTweens = new();

    private void Awake()
    {
        EnsureReferences();
    }

    public void SetBoomParticlePrefab(ParticleSystem prefab)
    {
        if (prefab == null)
            return;

        if (boomParticlePrefab == prefab && boomParticle != null)
            return;

        boomParticlePrefab = prefab;
        if (boomParticle != null)
        {
            Destroy(boomParticle.gameObject);
            boomParticle = null;
        }

        EnsureReferences();
    }

    public Tween Play(
        IReadOnlyList<ItemTile> tiles,
        Action onBoom = null,
        bool vanishDuringSpiral = true,
        bool playBoomParticle = true)
    {
        if (tiles == null || tiles.Count == 0)
            return null;

        EnsureReferences();
        sequence?.Kill();
        ClearRuntimeObjects();

        transform.position = GetScreenCenterPosition();
        mergeContainer.localPosition = Vector3.zero;
        PrepareBlackHole();

        Dictionary<ItemTile, SpriteRenderer> sourceRenderers = GetSourceRenderers(tiles);

        foreach (ItemTile tile in tiles)
        {
            if (tile == null)
                continue;

            Transform visual = TileOnRuntimeFx.VisualRoot(tile);
            tile.transform.DOKill();
            visual?.DOKill();
            tile.transform.SetParent(mergeContainer, true);
            tile.SetSortingLayer(TileOnRuntimeFx.HoveringSortingLayer);
            tile.SetSortingOrder(HintTileSortingOrder);
        }

        sequence = DOTween.Sequence();
        sequence.Append(PlayBlackHoleAppear());

        Sequence glowSequence = DOTween.Sequence();
        Dictionary<ItemTile, Vector3> originalScales = new();

        foreach (ItemTile tile in tiles)
        {
            Transform visual = TileOnRuntimeFx.VisualRoot(tile);
            if (visual == null)
                continue;

            Vector3 originalScale = visual.localScale;
            originalScales[tile] = originalScale;
            glowSequence.Join(visual.DOScale(originalScale * glowScale, glowDuration).SetEase(Ease.OutQuad));
        }

        sequence.Append(glowSequence);

        Sequence popSequence = DOTween.Sequence();
        foreach (ItemTile tile in tiles)
        {
            Transform visual = TileOnRuntimeFx.VisualRoot(tile);
            if (visual == null || !originalScales.ContainsKey(tile))
                continue;

            popSequence.Join(visual.DOScale(originalScales[tile] * popScale, popDuration).SetEase(Ease.OutBack));
        }

        sequence.Append(popSequence);

        Sequence spiralSequence = DOTween.Sequence();
        foreach (ItemTile tile in tiles)
        {
            Transform tileTransform = tile.transform;
            Transform visualTransform = TileOnRuntimeFx.VisualRoot(tile);
            if (visualTransform == null || !originalScales.ContainsKey(tile))
                continue;

            Vector3 startPosition = tileTransform.localPosition;
            Vector3 enlargedScale = originalScales[tile] * popScale;
            Quaternion startRotation = visualTransform.localRotation;
            float startRadius = new Vector2(startPosition.x, startPosition.y).magnitude;
            float startAngle = Mathf.Atan2(startPosition.y, startPosition.x) * Mathf.Rad2Deg;

            Tween spiralTween = DOTween.To(
                () => 0f,
                progress =>
                {
                    float angle = (startAngle + 360f * turns * progress) * Mathf.Deg2Rad;
                    float radius = Mathf.Lerp(startRadius, 0f, progress);

                    tileTransform.localPosition = new Vector3(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(angle) * radius,
                        Mathf.Lerp(startPosition.z, 0f, progress));

                    visualTransform.localRotation = startRotation * Quaternion.Euler(0f, 0f, -720f * progress);

                    if (vanishDuringSpiral)
                    {
                        float vanishProgress = Mathf.InverseLerp(vanishStart, 1f, progress);
                        visualTransform.localScale = Vector3.Lerp(enlargedScale, Vector3.zero, vanishProgress);
                    }
                    else
                    {
                        visualTransform.localScale = enlargedScale;
                    }
                },
                1f,
                spiralDuration)
                .SetEase(spiralEase)
                .SetTarget(tileTransform);

            spiralSequence.Join(spiralTween);
        }

        spiralSequence.Join(blackHoleCore.transform.DOLocalRotate(new Vector3(0f, 0f, -220f), spiralDuration, RotateMode.FastBeyond360).SetEase(Ease.Linear));
        spiralSequence.Join(blackHoleRingA.transform.DOLocalRotate(new Vector3(0f, 0f, 560f), spiralDuration, RotateMode.FastBeyond360).SetEase(Ease.Linear));
        spiralSequence.Join(blackHoleRingB.transform.DOLocalRotate(new Vector3(0f, 0f, -720f), spiralDuration, RotateMode.FastBeyond360).SetEase(Ease.Linear));
        spiralSequence.Join(blackHoleRoot.DOPunchScale(Vector3.one * 0.22f, spiralDuration, 4, 0.35f));
        sequence.Append(spiralSequence);
        sequence.AppendCallback(() =>
        {
            if (playBoomParticle && boomParticle != null)
            {
                boomParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                boomParticle.transform.localPosition = Vector3.zero;
                boomParticle.Play(true);
            }

            onBoom?.Invoke();
            SpawnTileFragments(sourceRenderers);
        });
        sequence.Append(PlayBlackHoleBreak());

        sequence.OnComplete(() =>
        {
            sequence = null;
            ClearRuntimeObjects();
        });
        return sequence;
    }

    private Vector3 GetScreenCenterPosition()
    {
        Camera cam = Camera.main;
        if (cam == null)
            return transform.position;

        float zDistance = Mathf.Abs(cam.transform.position.z - transform.position.z);
        Vector3 center = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, zDistance));
        center.z = transform.position.z;
        return center;
    }

    private void EnsureReferences()
    {
        if (mergeContainer == null)
        {
            GameObject container = new("Triple Tile Merge Container");
            container.transform.SetParent(transform, false);
            mergeContainer = container.transform;
        }

        if (boomParticle == null)
        {
            boomParticle = boomParticlePrefab != null
                ? Instantiate(boomParticlePrefab, transform)
                : TileOnRuntimeFx.CreateBurstParticle(
                    transform,
                    "Triple Tile Merge Boom",
                    new Color(1f, 0.74f, 0.22f, 1f),
                    28,
                    0.55f,
                    0.32f,
                    0.08f,
                    0.22f,
                    1.65f,
                    TileOnRuntimeFx.HoveringSortingLayer,
                    TileOnRuntimeFx.TileFxSortingOrder);
        }

        boomParticle.transform.SetParent(transform, false);
        boomParticle.transform.localPosition = Vector3.zero;
        TileOnRuntimeFx.SetRendererSorting(boomParticle.gameObject, TileOnRuntimeFx.HoveringSortingLayer, TileOnRuntimeFx.TileFxSortingOrder);
    }

    private Sequence PlayBlackHoleAppear()
    {
        Sequence appear = DOTween.Sequence();
        blackHoleRoot.localScale = Vector3.zero;
        SetBlackHoleAlpha(0f);

        appear.Join(blackHoleRoot.DOScale(Vector3.one * blackHoleWorldSize, blackHoleAppearDuration).SetEase(Ease.OutBack));
        appear.Join(DOTween.To(() => 0f, SetBlackHoleAlpha, 1f, blackHoleAppearDuration));
        appear.Join(blackHoleRingA.transform.DOLocalRotate(new Vector3(0f, 0f, -180f), blackHoleAppearDuration, RotateMode.FastBeyond360));
        appear.Join(blackHoleRingB.transform.DOLocalRotate(new Vector3(0f, 0f, 220f), blackHoleAppearDuration, RotateMode.FastBeyond360));
        return appear;
    }

    private Sequence PlayBlackHoleBreak()
    {
        Sequence breakSequence = DOTween.Sequence();
        breakSequence.Join(blackHoleRoot.DOScale(Vector3.one * blackHoleWorldSize * 0.28f, blackHoleBreakDuration).SetEase(Ease.InBack));
        breakSequence.Join(DOTween.To(() => 1f, SetBlackHoleAlpha, 0f, blackHoleBreakDuration).SetEase(Ease.InQuad));
        breakSequence.Join(blackHoleRingA.transform.DOLocalRotate(new Vector3(0f, 0f, -520f), blackHoleBreakDuration, RotateMode.FastBeyond360).SetEase(Ease.InCubic));
        breakSequence.Join(blackHoleRingB.transform.DOLocalRotate(new Vector3(0f, 0f, 640f), blackHoleBreakDuration, RotateMode.FastBeyond360).SetEase(Ease.InCubic));
        return breakSequence;
    }

    private void PrepareBlackHole()
    {
        if (blackHoleRoot == null)
        {
            GameObject root = new("Hint Black Hole");
            root.transform.SetParent(transform, false);
            blackHoleRoot = root.transform;
        }

        blackHoleCoreSprite ??= CreateRadialSprite("Hint Black Hole Core", Color.black, new Color(0.02f, 0.01f, 0.16f, 1f), true);
        blackHoleRingSprite ??= CreateRadialSprite("Hint Black Hole Ring", new Color(1f, 0.82f, 0.42f, 1f), new Color(0.9f, 0.12f, 0.02f, 0.72f), false);

        blackHoleCore = EnsureSpriteRenderer(blackHoleRoot, ref blackHoleCore, "Core", blackHoleCoreSprite, new Color(0f, 0f, 0.01f, 1f), 0.52f, BlackHoleBaseSortingOrder + 2);
        blackHoleRingA = EnsureSpriteRenderer(blackHoleRoot, ref blackHoleRingA, "Accretion Ring A", blackHoleRingSprite, new Color(1f, 0.92f, 0.68f, 1f), 1.38f, BlackHoleBaseSortingOrder + 1);
        blackHoleRingB = EnsureSpriteRenderer(blackHoleRoot, ref blackHoleRingB, "Accretion Ring B", blackHoleRingSprite, new Color(1f, 0.24f, 0.04f, 0.8f), 1.72f, BlackHoleBaseSortingOrder);
        blackHoleRingB.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);

        blackHoleRoot.gameObject.SetActive(true);
    }

    private SpriteRenderer EnsureSpriteRenderer(Transform parent, ref SpriteRenderer renderer, string name, Sprite sprite, Color color, float scale, int sortingOrder)
    {
        if (renderer == null)
        {
            GameObject rendererObject = new(name);
            rendererObject.transform.SetParent(parent, false);
            renderer = rendererObject.AddComponent<SpriteRenderer>();
        }

        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingLayerName = TileOnRuntimeFx.HoveringSortingLayer;
        renderer.sortingOrder = sortingOrder;
        renderer.transform.localPosition = Vector3.zero;
        renderer.transform.localScale = Vector3.one * scale;
        return renderer;
    }

    private Sprite CreateRadialSprite(string spriteName, Color innerColor, Color outerColor, bool solidCenter)
    {
        Texture2D texture = new(BlackHoleTextureSize, BlackHoleTextureSize, TextureFormat.RGBA32, false)
        {
            name = $"{spriteName} Texture",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        Vector2 center = Vector2.one * ((BlackHoleTextureSize - 1) * 0.5f);
        for (int y = 0; y < BlackHoleTextureSize; y++)
        {
            for (int x = 0; x < BlackHoleTextureSize; x++)
            {
                Vector2 offset = new((x - center.x) / (BlackHoleTextureSize * 0.5f), (y - center.y) / (BlackHoleTextureSize * 0.5f));
                float distance = offset.magnitude;
                float angle = Mathf.Atan2(offset.y, offset.x);
                float alpha;
                Color color;

                if (solidCenter)
                {
                    float eventHorizon = Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(0.26f, 0.36f, distance));
                    float lensGlow = Mathf.Exp(-Mathf.Pow((distance - 0.38f) * 13f, 2f)) * 0.42f;
                    alpha = Mathf.Clamp01(eventHorizon + lensGlow);
                    color = Color.Lerp(innerColor, outerColor, lensGlow);
                    color = Color.Lerp(color, Color.black, eventHorizon);
                }
                else
                {
                    float disk = Mathf.Exp(-Mathf.Pow((distance - 0.47f) * 7.2f, 2f));
                    float outerHaze = Mathf.Exp(-Mathf.Pow((distance - 0.68f) * 4.4f, 2f)) * 0.42f;
                    float spiral = 0.5f + 0.5f * Mathf.Sin(angle * 3.2f + distance * 28f);
                    float turbulence = Mathf.PerlinNoise(x * 0.045f, y * 0.045f);
                    float heat = Mathf.Clamp01(disk * (0.74f + spiral * 0.34f + turbulence * 0.28f));
                    alpha = Mathf.Clamp01(disk * 0.96f + outerHaze);
                    Color darkOrange = new(0.38f, 0.04f, 0f, 1f);
                    Color hotOrange = new(1f, 0.34f, 0.02f, 1f);
                    Color whiteHot = new(1f, 0.92f, 0.66f, 1f);
                    color = Color.Lerp(darkOrange, hotOrange, heat);
                    color = Color.Lerp(color, whiteHot, Mathf.Clamp01((heat - 0.58f) * 2.4f));
                    color = Color.Lerp(color, outerColor, Mathf.Clamp01(distance - 0.64f));
                }

                color.a *= alpha;
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, BlackHoleTextureSize, BlackHoleTextureSize), new Vector2(0.5f, 0.5f), 64f);
        sprite.name = spriteName;
        return sprite;
    }

    private void SetBlackHoleAlpha(float alpha)
    {
        SetRendererAlpha(blackHoleCore, alpha);
        SetRendererAlpha(blackHoleRingA, alpha);
        SetRendererAlpha(blackHoleRingB, alpha * 0.95f);
    }

    private void SetRendererAlpha(SpriteRenderer renderer, float alpha)
    {
        if (renderer == null)
            return;

        Color color = renderer.color;
        color.a = alpha;
        renderer.color = color;
    }

    private Dictionary<ItemTile, SpriteRenderer> GetSourceRenderers(IReadOnlyList<ItemTile> tiles)
    {
        Dictionary<ItemTile, SpriteRenderer> sourceRenderers = new();
        foreach (ItemTile tile in tiles)
        {
            if (TryGetSourceRenderer(tile, out SpriteRenderer renderer))
                sourceRenderers[tile] = renderer;
        }

        return sourceRenderers;
    }

    private void SpawnTileFragments(Dictionary<ItemTile, SpriteRenderer> sourceRenderers)
    {
        if (sourceRenderers == null || sourceRenderers.Count == 0)
            return;

        int sortingOrder = TileOnRuntimeFx.TileFxSortingOrder + FragmentSortingOffset;
        foreach (KeyValuePair<ItemTile, SpriteRenderer> pair in sourceRenderers)
        {
            SpawnFragmentsForRenderer(pair.Value, sortingOrder);
            sortingOrder += 30;
        }
    }

    private void SpawnFragmentsForRenderer(SpriteRenderer sourceRenderer, int sortingOrder)
    {
        if (sourceRenderer == null || sourceRenderer.sprite == null)
            return;

        Sprite sourceSprite = sourceRenderer.sprite;
        Vector3 baseScale = GetFragmentScale(sourceRenderer, sourceSprite);
        Color color = sourceRenderer.color;
        color.a = 1f;
        List<Sprite> fragments = CreateVariableFragmentSprites(sourceSprite);

        for (int i = 0; i < fragments.Count; i++)
        {
            GameObject fragmentObject = new($"Hint Item Fragment {i + 1}");
            fragmentObject.transform.SetParent(transform, false);
            fragmentObject.transform.localPosition = Vector3.zero;
            fragmentObject.transform.localRotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f));

            SpriteRenderer renderer = fragmentObject.AddComponent<SpriteRenderer>();
            renderer.sprite = fragments[i];
            renderer.color = color;
            renderer.sortingLayerName = TileOnRuntimeFx.HoveringSortingLayer;
            renderer.sortingOrder = sortingOrder + i;

            float sizeMultiplier = UnityEngine.Random.Range(0.32f, 1.28f);
            fragmentObject.transform.localScale = baseScale * sizeMultiplier;

            Vector2 direction = UnityEngine.Random.insideUnitCircle.normalized;
            if (direction.sqrMagnitude < 0.001f)
                direction = Vector2.up;

            Vector3 endPosition = new(
                direction.x * UnityEngine.Random.Range(0.45f, 1.65f) * FragmentBurstRangeMultiplier,
                direction.y * UnityEngine.Random.Range(0.45f, 1.65f) * FragmentBurstRangeMultiplier,
                0f);
            float duration = UnityEngine.Random.Range(0.78f, 1.18f) * Match3DurationMultiplier;
            Sequence fragmentSequence = DOTween.Sequence()
                .Join(fragmentObject.transform.DOLocalMove(endPosition, duration).SetEase(Ease.OutCubic))
                .Join(fragmentObject.transform.DOScale(baseScale * UnityEngine.Random.Range(0.08f, 0.42f), duration).SetEase(Ease.InQuad))
                .Join(fragmentObject.transform.DOLocalRotate(new Vector3(0f, 0f, UnityEngine.Random.Range(-520f, 520f)), duration, RotateMode.FastBeyond360))
                .Join(renderer.DOFade(0f, duration * 0.72f).SetDelay(duration * 0.18f))
                .SetTarget(fragmentObject);

            runtimeTweens.Add(fragmentSequence);
            runtimeObjects.Add(fragmentObject);
        }
    }

    private Vector3 GetFragmentScale(SpriteRenderer sourceRenderer, Sprite sourceSprite)
    {
        Vector2 spriteSize = sourceSprite.bounds.size;
        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
            return Vector3.one;

        Bounds renderedBounds = sourceRenderer.bounds;
        Vector3 effectScale = transform.lossyScale;
        float parentScaleX = Mathf.Approximately(effectScale.x, 0f) ? 1f : Mathf.Abs(effectScale.x);
        float parentScaleY = Mathf.Approximately(effectScale.y, 0f) ? 1f : Mathf.Abs(effectScale.y);

        return new Vector3(
            Mathf.Abs(renderedBounds.size.x) / spriteSize.x / parentScaleX,
            Mathf.Abs(renderedBounds.size.y) / spriteSize.y / parentScaleY,
            1f);
    }

    private List<Sprite> CreateVariableFragmentSprites(Sprite sourceSprite)
    {
        List<Sprite> fragments = new();

        try
        {
            Rect textureRect = sourceSprite.textureRect;
            int minX = Mathf.RoundToInt(textureRect.xMin);
            int minY = Mathf.RoundToInt(textureRect.yMin);
            int maxX = Mathf.RoundToInt(textureRect.xMax);
            int maxY = Mathf.RoundToInt(textureRect.yMax);
            int fragmentCount = UnityEngine.Random.Range(MinFragmentsPerTile, MaxFragmentsPerTileExclusive);

            for (int i = 0; i < fragmentCount; i++)
            {
                float widthRatio = UnityEngine.Random.Range(0.12f, 0.34f);
                float heightRatio = UnityEngine.Random.Range(0.12f, 0.38f);
                int width = Mathf.Max(4, Mathf.RoundToInt(textureRect.width * widthRatio));
                int height = Mathf.Max(4, Mathf.RoundToInt(textureRect.height * heightRatio));
                int x = UnityEngine.Random.Range(minX, Mathf.Max(minX + 1, maxX - width));
                int y = UnityEngine.Random.Range(minY, Mathf.Max(minY + 1, maxY - height));
                Rect rect = new(x, y, Mathf.Min(width, maxX - x), Mathf.Min(height, maxY - y));

                Sprite fragment = Sprite.Create(sourceSprite.texture, rect, new Vector2(0.5f, 0.5f), sourceSprite.pixelsPerUnit, 0, SpriteMeshType.FullRect);
                fragment.name = $"{sourceSprite.name}_hint_fragment_{i}";
                runtimeSprites.Add(fragment);
                fragments.Add(fragment);
            }
        }
        catch (Exception ex) when (ex is UnityException || ex is ArgumentException)
        {
            for (int i = 0; i < MinFragmentsPerTile; i++)
                fragments.Add(sourceSprite);
        }

        return fragments;
    }

    private bool TryGetSourceRenderer(ItemTile tile, out SpriteRenderer sourceRenderer)
    {
        sourceRenderer = null;
        if (tile == null)
            return false;

        if (tile.IsEventTile)
        {
            sourceRenderer = FirstSprite(tile.eventTileExtend.goldPieces, false);
            if (sourceRenderer != null)
                return true;
        }

        if (tile.TryGetComponent(out GoldTileVariant goldTile) &&
            IsUsableSource(goldTile.gold) &&
            goldTile.gold.gameObject.activeInHierarchy)
        {
            sourceRenderer = goldTile.gold;
            return true;
        }

        if (IsUsableSource(tile.item))
        {
            sourceRenderer = tile.item;
            return true;
        }

        sourceRenderer = FirstSprite(tile.GetComponentsInChildren<SpriteRenderer>(true), true, tile.bg, tile.shadow);
        return sourceRenderer != null;
    }

    private SpriteRenderer FirstSprite(SpriteRenderer[] renderers, bool preferActive, params SpriteRenderer[] ignoredRenderers)
    {
        if (renderers == null)
            return null;

        SpriteRenderer fallback = null;
        for (int i = 0; i < renderers.Length; i++)
        {
            SpriteRenderer renderer = renderers[i];
            if (!IsUsableSource(renderer) || IsIgnored(renderer, ignoredRenderers))
                continue;

            if (!preferActive || renderer.gameObject.activeInHierarchy)
                return renderer;

            fallback ??= renderer;
        }

        return fallback;
    }

    private bool IsUsableSource(SpriteRenderer renderer)
    {
        return renderer != null && renderer.sprite != null;
    }

    private bool IsIgnored(SpriteRenderer renderer, SpriteRenderer[] ignoredRenderers)
    {
        if (ignoredRenderers == null)
            return false;

        for (int i = 0; i < ignoredRenderers.Length; i++)
        {
            if (renderer == ignoredRenderers[i])
                return true;
        }

        return false;
    }

    private void ClearRuntimeObjects()
    {
        for (int i = 0; i < runtimeTweens.Count; i++)
            runtimeTweens[i]?.Kill(false);
        runtimeTweens.Clear();

        for (int i = 0; i < runtimeObjects.Count; i++)
        {
            if (runtimeObjects[i] != null)
                Destroy(runtimeObjects[i]);
        }
        runtimeObjects.Clear();

        for (int i = 0; i < runtimeSprites.Count; i++)
        {
            if (runtimeSprites[i] != null)
                Destroy(runtimeSprites[i]);
        }
        runtimeSprites.Clear();

        if (blackHoleRoot != null)
            blackHoleRoot.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        sequence?.Kill();
        sequence = null;
        ClearRuntimeObjects();
    }
}
