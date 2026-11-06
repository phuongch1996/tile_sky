using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MatchedItemBurstFx : MonoBehaviour
{
    const int DefaultColumns = 6;
    const int DefaultRows = 3;
    const float Match3DurationMultiplier = 7f / 8f;
    const float Duration = 0.78f * Match3DurationMultiplier;
    const float FragmentScaleMultiplier = 1.18f;
    const float ImpactScaleMultiplier = 1.35f;

    readonly List<Sprite> runtimeSprites = new();
    readonly List<Tween> tweens = new();

    public static GameObject Play(ItemTile tile, Transform parent, string sortingLayerName, int sortingOrder)
    {
        if (tile == null || !TryGetSourceRenderer(tile, out SpriteRenderer sourceRenderer))
            return null;

        return Play(sourceRenderer, parent, sortingLayerName, sortingOrder, "Matched Item Burst Fx");
    }

    public static GameObject Play(SpriteRenderer sourceRenderer, Transform parent, string sortingLayerName, int sortingOrder, string effectName = "Sprite Burst Fx")
    {
        if (!IsUsableSource(sourceRenderer))
            return null;

        GameObject effectObject = new(effectName);
        if (parent != null)
            effectObject.transform.SetParent(parent, false);

        effectObject.transform.position = sourceRenderer.transform.position;
        MatchedItemBurstFx effect = effectObject.AddComponent<MatchedItemBurstFx>();
        effect.PlayInternal(sourceRenderer, sortingLayerName, sortingOrder);
        return effectObject;
    }

    void PlayInternal(SpriteRenderer sourceRenderer, string sortingLayerName, int sortingOrder)
    {
        if (!IsUsableSource(sourceRenderer))
            return;

        Sprite sourceSprite = sourceRenderer.sprite;
        Vector3 baseScale = GetFragmentLocalScale(sourceRenderer, sourceSprite);

        Color baseColor = sourceRenderer.color;
        baseColor.a = 1f;
        IReadOnlyList<Sprite> fragments = CreateFragmentSprites(sourceSprite);
        SpawnImpactSprite(sourceSprite, baseScale, baseColor, sortingLayerName, sortingOrder);

        for (int i = 0; i < fragments.Count; i++)
        {
            Sprite fragmentSprite = fragments[i];
            GameObject fragmentObject = new($"Fragment {i + 1}");
            fragmentObject.transform.SetParent(transform, false);

            SpriteRenderer shadowRenderer = CreateFragmentRenderer(
                fragmentObject.transform,
                "Shadow",
                fragmentSprite,
                new Color(0f, 0f, 0f, 0.32f),
                sortingLayerName,
                sortingOrder + i);
            shadowRenderer.transform.localPosition = new Vector3(0.035f, -0.035f, 0f);

            SpriteRenderer renderer = fragmentObject.AddComponent<SpriteRenderer>();
            renderer.sprite = fragmentSprite;
            renderer.color = baseColor;
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = sortingOrder + fragments.Count + i;

            Vector3 startOffset = Vector3.Scale(GetFragmentStartOffset(i, fragments.Count, sourceSprite), baseScale);
            fragmentObject.transform.localPosition = startOffset;
            fragmentObject.transform.localScale = baseScale * FragmentScaleMultiplier;
            fragmentObject.transform.localRotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(-10f, 10f));

            Vector2 direction = startOffset.sqrMagnitude > 0.001f
                ? ((Vector2)startOffset).normalized
                : UnityEngine.Random.insideUnitCircle.normalized;
            if (direction.sqrMagnitude < 0.001f)
                direction = Vector2.up;

            float distance = UnityEngine.Random.Range(0.62f, 1.05f);
            float lift = UnityEngine.Random.Range(0.08f, 0.28f);
            Vector3 endPosition = startOffset + new Vector3(direction.x * distance, direction.y * distance + lift, 0f);

            Sequence sequence = DOTween.Sequence()
                .Join(fragmentObject.transform.DOLocalMove(endPosition, Duration).SetEase(Ease.OutCubic))
                .Join(fragmentObject.transform.DORotate(new Vector3(0f, 0f, UnityEngine.Random.Range(-260f, 260f)), Duration, RotateMode.FastBeyond360).SetEase(Ease.OutQuad))
                .Join(fragmentObject.transform.DOScale(baseScale * UnityEngine.Random.Range(0.7f, 0.95f), Duration).SetEase(Ease.OutQuad))
                .Join(renderer.DOFade(0f, Duration * 0.85f).SetDelay(Duration * 0.22f))
                .Join(shadowRenderer.DOFade(0f, Duration * 0.72f).SetDelay(Duration * 0.22f))
                .SetTarget(fragmentObject);

            tweens.Add(sequence);
        }

        Destroy(gameObject, Duration + 0.05f);
    }

    void SpawnImpactSprite(Sprite sourceSprite, Vector3 baseScale, Color baseColor, string sortingLayerName, int sortingOrder)
    {
        GameObject impactObject = new("Impact Sprite");
        impactObject.transform.SetParent(transform, false);
        impactObject.transform.localScale = baseScale;

        SpriteRenderer impactRenderer = impactObject.AddComponent<SpriteRenderer>();
        impactRenderer.sprite = sourceSprite;
        impactRenderer.color = baseColor;
        impactRenderer.sortingLayerName = sortingLayerName;
        impactRenderer.sortingOrder = sortingOrder + 100;

        Sequence sequence = DOTween.Sequence()
            .Join(impactObject.transform.DOScale(baseScale * ImpactScaleMultiplier, 0.16f * Match3DurationMultiplier).SetEase(Ease.OutQuad))
            .Join(impactRenderer.DOFade(0f, 0.18f * Match3DurationMultiplier).SetEase(Ease.InQuad))
            .SetTarget(impactObject);

        tweens.Add(sequence);
    }

    Vector3 GetFragmentLocalScale(SpriteRenderer sourceRenderer, Sprite sourceSprite)
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

    SpriteRenderer CreateFragmentRenderer(Transform parent, string name, Sprite sprite, Color color, string sortingLayerName, int sortingOrder)
    {
        GameObject rendererObject = new(name);
        rendererObject.transform.SetParent(parent, false);

        SpriteRenderer renderer = rendererObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingLayerName = sortingLayerName;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }

    static bool TryGetSourceRenderer(ItemTile tile, out SpriteRenderer sourceRenderer)
    {
        sourceRenderer = null;
        if (tile == null)
            return false;

        if (tile.IsEventTile && tile.eventTileExtend.EventUp)
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

    static SpriteRenderer FirstSprite(SpriteRenderer[] renderers, bool preferActive, params SpriteRenderer[] ignoredRenderers)
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

            if (fallback == null)
                fallback = renderer;
        }

        return fallback;
    }

    static bool IsUsableSource(SpriteRenderer renderer)
    {
        return renderer != null && renderer.sprite != null;
    }

    static bool IsIgnored(SpriteRenderer renderer, SpriteRenderer[] ignoredRenderers)
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

    IReadOnlyList<Sprite> CreateFragmentSprites(Sprite sourceSprite)
    {
        List<Sprite> fragments = new(DefaultColumns * DefaultRows);

        try
        {
            Rect textureRect = sourceSprite.textureRect;
            int minX = Mathf.RoundToInt(textureRect.xMin);
            int minY = Mathf.RoundToInt(textureRect.yMin);
            int maxX = Mathf.RoundToInt(textureRect.xMax);
            int maxY = Mathf.RoundToInt(textureRect.yMax);

            for (int y = 0; y < DefaultRows; y++)
            {
                for (int x = 0; x < DefaultColumns; x++)
                {
                    int fragmentMinX = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(minX, maxX, (float)x / DefaultColumns)), minX, maxX - 1);
                    int fragmentMaxX = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(minX, maxX, (float)(x + 1) / DefaultColumns)), fragmentMinX + 1, maxX);
                    int fragmentMinY = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(minY, maxY, (float)y / DefaultRows)), minY, maxY - 1);
                    int fragmentMaxY = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(minY, maxY, (float)(y + 1) / DefaultRows)), fragmentMinY + 1, maxY);

                    Rect rect = new(fragmentMinX, fragmentMinY, fragmentMaxX - fragmentMinX, fragmentMaxY - fragmentMinY);
                    Sprite fragment = Sprite.Create(sourceSprite.texture, rect, new Vector2(0.5f, 0.5f), sourceSprite.pixelsPerUnit, 0, SpriteMeshType.FullRect);
                    fragment.name = $"{sourceSprite.name}_match_fragment_{x}_{y}";
                    runtimeSprites.Add(fragment);
                    fragments.Add(fragment);
                }
            }
        }
        catch (Exception ex) when (ex is UnityException || ex is ArgumentException)
        {
            for (int i = 0; i < DefaultColumns * DefaultRows; i++)
                fragments.Add(sourceSprite);
        }

        return fragments;
    }

    Vector3 GetFragmentStartOffset(int index, int count, Sprite sourceSprite)
    {
        if (count != DefaultColumns * DefaultRows)
            return UnityEngine.Random.insideUnitCircle * 0.16f;

        int x = index % DefaultColumns;
        int y = index / DefaultColumns;
        Vector2 spriteSize = sourceSprite.bounds.size;
        float offsetX = ((x + 0.5f) / DefaultColumns - 0.5f) * spriteSize.x;
        float offsetY = ((y + 0.5f) / DefaultRows - 0.5f) * spriteSize.y;
        return new Vector3(offsetX, offsetY, 0f);
    }

    void OnDestroy()
    {
        for (int i = 0; i < tweens.Count; i++)
            tweens[i]?.Kill();

        for (int i = 0; i < runtimeSprites.Count; i++)
        {
            if (runtimeSprites[i] != null)
                Destroy(runtimeSprites[i]);
        }
    }
}
