using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public static class FinalMatchRocketFx
{
    const string RocketResourcePath = "FinalMatchRocket";
    const float RocketSourceAngle = 48f;
    const float FlyDuration = 0.72f;
    const float TileExitDuration = 0.42f;
    const float StartPadding = 2.2f;
    const float EndPadding = 3.2f;
    const float RocketScale = 1.1625f;

    static Sprite rocketSprite;
    static Sprite trailSprite;
    static Sprite coreTrailSprite;

    public static Tween Play(IReadOnlyList<ItemTile> tiles, Transform parent, string sortingLayerName, int sortingOrder)
    {
        List<ItemTile> activeTiles = tiles?
            .Where(tile => tile != null)
            .OrderBy(tile => tile.transform.position.x)
            .ToList();
        if (activeTiles == null || activeTiles.Count == 0)
            return null;

        EnsureSprites();
        Bounds tileBounds = GetTileBounds(activeTiles);
        Vector3 center = tileBounds.center;
        Vector3 start = new(GetCameraLeft(center.z) - StartPadding, center.y, center.z);
        Vector3 end = new(GetCameraRight(center.z) + EndPadding, center.y, center.z);

        GameObject rocketObject = new("Final Match Rocket Fx");
        if (parent != null)
            rocketObject.transform.SetParent(parent, false);
        rocketObject.transform.position = start;
        rocketObject.transform.rotation = Quaternion.Euler(0f, 0f, -RocketSourceAngle);
        rocketObject.transform.localScale = Vector3.one * RocketScale;

        SpriteRenderer rocketRenderer = rocketObject.AddComponent<SpriteRenderer>();
        rocketRenderer.sprite = rocketSprite;
        rocketRenderer.sortingLayerName = sortingLayerName;
        rocketRenderer.sortingOrder = sortingOrder + 40;

        SpriteRenderer trailRenderer = CreateTrailRenderer(rocketObject.transform, "Rocket Trail", trailSprite, new Color(1f, 0.42f, 0.03f, 0.9f), sortingLayerName, sortingOrder + 36);
        trailRenderer.transform.localPosition = new Vector3(-0.98f, -0.08f, 0f);
        trailRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, RocketSourceAngle);
        trailRenderer.transform.localScale = new Vector3(1.3f, 0.62f, 1f);

        SpriteRenderer coreTrailRenderer = CreateTrailRenderer(rocketObject.transform, "Rocket Core Trail", coreTrailSprite, new Color(1f, 0.92f, 0.36f, 0.84f), sortingLayerName, sortingOrder + 37);
        coreTrailRenderer.transform.localPosition = new Vector3(-0.82f, -0.04f, 0f);
        coreTrailRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, RocketSourceAngle);
        coreTrailRenderer.transform.localScale = new Vector3(0.95f, 0.36f, 1f);

        ParticleSystem sparks = TileOnRuntimeFx.CreateBurstParticle(
            parent,
            "Final Match Rocket Sparks",
            new Color(1f, 0.62f, 0.08f, 1f),
            42,
            0.6f,
            0.38f,
            0.06f,
            0.16f,
            2.2f,
            sortingLayerName,
            sortingOrder + 50);
        sparks.transform.position = center;

        Sequence sequence = DOTween.Sequence().SetTarget(rocketObject);
        sequence.Append(rocketObject.transform.DOMove(end, FlyDuration).SetEase(Ease.InOutSine));
        sequence.Join(rocketObject.transform.DOScale(RocketScale * 1.08f, FlyDuration * 0.5f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutSine));
        sequence.Join(trailRenderer.DOFade(0.52f, FlyDuration * 0.12f).SetLoops(6, LoopType.Yoyo));
        sequence.Join(coreTrailRenderer.DOFade(0.42f, FlyDuration * 0.1f).SetLoops(7, LoopType.Yoyo));
        sequence.Insert(FlyDuration * 0.34f, BuildTileExitTween(activeTiles, end, sortingLayerName, sortingOrder + 20));
        sequence.InsertCallback(FlyDuration * 0.38f, () =>
        {
            sparks.transform.position = center;
            sparks.Play(true);
        });
        sequence.OnComplete(() =>
        {
            if (sparks != null)
                Object.Destroy(sparks.gameObject, 0.8f);
            Object.Destroy(rocketObject);
        });

        return sequence;
    }

    static Tween BuildTileExitTween(List<ItemTile> tiles, Vector3 end, string sortingLayerName, int sortingOrder)
    {
        Sequence tileSequence = DOTween.Sequence();
        for (int i = 0; i < tiles.Count; i++)
        {
            ItemTile tile = tiles[i];
            tile.SetSortingLayer(sortingLayerName);
            tile.SetSortingOrder(sortingOrder + i);
            tile.transform.DOKill();
            TileOnRuntimeFx.VisualRoot(tile)?.DOKill();

            Vector3 target = end + new Vector3(-0.55f - i * 0.38f, (i - 1) * 0.18f, 0f);
            tileSequence.Insert(i * 0.035f, tile.transform.DOMove(target, TileExitDuration).SetEase(Ease.InBack));
            tileSequence.Insert(i * 0.035f, tile.transform.DORotate(new Vector3(0f, 0f, -18f + i * 18f), TileExitDuration, RotateMode.FastBeyond360));
            tileSequence.Insert(i * 0.035f, tile.transform.DOScale(0.85f, TileExitDuration).SetEase(Ease.InSine));
        }

        return tileSequence;
    }

    static Bounds GetTileBounds(IReadOnlyList<ItemTile> tiles)
    {
        Bounds bounds = new(tiles[0].transform.position, Vector3.zero);
        for (int i = 1; i < tiles.Count; i++)
            bounds.Encapsulate(tiles[i].transform.position);
        return bounds;
    }

    static float GetCameraLeft(float z)
    {
        Camera cam = Camera.main;
        if (cam == null)
            return -8f;

        float distance = Mathf.Abs(cam.transform.position.z - z);
        return cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, distance)).x;
    }

    static float GetCameraRight(float z)
    {
        Camera cam = Camera.main;
        if (cam == null)
            return 8f;

        float distance = Mathf.Abs(cam.transform.position.z - z);
        return cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, distance)).x;
    }

    static SpriteRenderer CreateTrailRenderer(Transform parent, string name, Sprite sprite, Color color, string sortingLayerName, int sortingOrder)
    {
        GameObject trailObject = new(name);
        trailObject.transform.SetParent(parent, false);

        SpriteRenderer renderer = trailObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingLayerName = sortingLayerName;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }

    static void EnsureSprites()
    {
        if (rocketSprite == null)
        {
            rocketSprite = Resources.Load<Sprite>(RocketResourcePath);
            if (rocketSprite == null)
            {
                Texture2D rocketTexture = Resources.Load<Texture2D>(RocketResourcePath);
                if (rocketTexture != null)
                {
                    rocketSprite = Sprite.Create(
                        rocketTexture,
                        new Rect(0f, 0f, rocketTexture.width, rocketTexture.height),
                        new Vector2(0.5f, 0.5f),
                        100f,
                        0,
                        SpriteMeshType.FullRect);
                }
            }
        }
        if (trailSprite == null)
            trailSprite = CreateTrailSprite("Final Match Rocket Trail", 288, 120, 0.92f);
        if (coreTrailSprite == null)
            coreTrailSprite = CreateTrailSprite("Final Match Rocket Core Trail", 220, 72, 0.72f);
    }

    static Sprite CreateTrailSprite(string name, int width, int height, float alphaMultiplier)
    {
        Texture2D texture = new(width, height, TextureFormat.RGBA32, false)
        {
            name = name,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        Vector2 center = new(width * 0.92f, height * 0.5f);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float forward = Mathf.InverseLerp(0f, width - 1f, x);
                float vertical = Mathf.Abs((y - center.y) / height);
                float widthAtX = Mathf.Lerp(0.02f, 0.28f, Mathf.Pow(forward, 1.4f));
                float body = 1f - Mathf.SmoothStep(widthAtX, widthAtX + 0.12f, vertical);
                float hotCore = 1f - Mathf.SmoothStep(widthAtX * 0.28f, widthAtX * 0.28f + 0.04f, vertical);
                float fade = Mathf.SmoothStep(0.02f, 0.78f, forward);
                float alpha = Mathf.Clamp01((body * 0.72f + hotCore * 0.55f) * fade * alphaMultiplier);
                Color ember = Color.Lerp(new Color(0.78f, 0.04f, 0f, 1f), new Color(1f, 0.54f, 0.04f, 1f), forward);
                Color heat = Color.Lerp(ember, new Color(1f, 0.95f, 0.42f, 1f), hotCore);
                heat.a = alpha;
                texture.SetPixel(x, y, heat);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.92f, 0.5f), 96f, 0, SpriteMeshType.FullRect);
    }
}
