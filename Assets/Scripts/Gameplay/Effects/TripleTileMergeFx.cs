using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TripleTileMergeFx : MonoBehaviour
{
    const string SortingLayer = TileOnRuntimeFx.HoveringSortingLayer;
    const int SortingOrder = TileOnRuntimeFx.TileFxSortingOrder;
    const float TimeMultiplier = 1f / GameController.Match3EffectSpeed;
    public const float Duration = 1.2f * TimeMultiplier;
    const float HighlightDuration = 0.12f * TimeMultiplier;
    const float HopStagger = 0.16f * TimeMultiplier;
    readonly List<TileState> tilesInFlight = new();
    readonly List<GameObject> visuals = new();
    Sequence sequence;
    Material starMaterial;
    Material highlightMaterial;

    sealed class TileState
    {
        public ItemTile tile;
        public Transform visual;
        public Vector3 position;
        public Vector3 scale;
        public Quaternion rotation;
        public readonly List<SpriteRenderer> highlights = new();
        public RendererState[] renderers;
    }

    struct RendererState
    {
        public SpriteRenderer renderer;
        public int layer;
        public int order;
    }

    public Tween Play(IReadOnlyList<ItemTile> tiles, Action onComplete = null)
    {
        ResetEffect(true);
        if (tiles == null || tiles.Count == 0) return null;
        if (!starMaterial) starMaterial = new Material(Shader.Find("Sprites/Default"));
        if (!highlightMaterial)
        {
            var shader = Resources.Load<Shader>("TileSkyFx/HintAdditiveGlow");
            if (shader) highlightMaterial = new Material(shader);
        }

        sequence = DOTween.Sequence();
        var illuminate = DOTween.Sequence();
        foreach (var tile in tiles)
        {
            if (!tile) continue;
            var visual = TileOnRuntimeFx.VisualRoot(tile);
            tile.transform.DOKill();
            visual.DOKill();
            var state = new TileState
            {
                tile = tile, visual = visual, position = tile.transform.localPosition,
                scale = visual.localScale, rotation = visual.localRotation
            };
            var renderers = tile.GetComponentsInChildren<SpriteRenderer>(true);
            state.renderers = new RendererState[renderers.Length];
            int baseOrder = tile.bg ? tile.bg.sortingOrder : 0;
            for (int i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                state.renderers[i] = new RendererState
                {
                    renderer = renderer, layer = renderer.sortingLayerID, order = renderer.sortingOrder
                };
                renderer.sortingLayerName = SortingLayer;
                renderer.sortingOrder = SortingOrder + tilesInFlight.Count * 30 + renderer.sortingOrder - baseOrder;
            }
            // Put the halo behind the tile; additive copies brighten its actual artwork.
            AddHighlight(state, tile.bg, false, 1.16f);
            if (highlightMaterial)
            {
                AddHighlight(state, tile.item, true, 1);
            }
            tilesInFlight.Add(state);
            illuminate.Join(visual.DOScale(state.scale * 1.1f, HighlightDuration).SetEase(Ease.OutSine));
            foreach (var highlight in state.highlights)
                illuminate.Join(highlight.DOFade(highlight.sharedMaterial == highlightMaterial ? 0.3f : 0.7f,
                    HighlightDuration).SetEase(Ease.OutSine));
        }
        if (tilesInFlight.Count == 0)
        {
            ResetEffect(true);
            return null;
        }

        // Keep the hops and their particles on the same match-effect timeline.
        sequence.Append(illuminate);
        for (int i = 0; i < tilesInFlight.Count; i++)
        {
            var state = tilesInFlight[i];
            int order = SortingOrder + i * 30 + 15;
            float startY = state.tile.transform.position.y;
            float height = state.tile.bg ? Mathf.Clamp(state.tile.bg.bounds.size.y * 1.1f, 0.8f, 1.6f) : 1.2f;
            var trail = CreateLightTrail(state.tile.transform, order);
            var ring = CreatePopRing(order);
            var hop = DOTween.Sequence();
            hop.Append(state.visual.DOScale(Vector3.Scale(state.scale, new Vector3(1.18f, 0.85f, 1)), 0.04f * TimeMultiplier));
            hop.AppendCallback(() =>
            {
                trail.emitting = true;
                SpawnStars(state.tile.transform.position, order, 10);
            });
            hop.Append(state.tile.transform.DOMoveY(startY + height, 0.22f * TimeMultiplier).SetEase(Ease.OutCubic));
            hop.Join(state.visual.DOScale(state.scale * 1.18f, 0.14f * TimeMultiplier).SetEase(Ease.OutBack));
            hop.Join(state.visual.DOLocalRotateQuaternion(state.rotation * Quaternion.Euler(0, 0, i % 2 == 0 ? 9 : -9), 0.22f * TimeMultiplier));
            hop.AppendCallback(() =>
            {
                SpawnStars(state.tile.transform.position, order, 28);
                ring.transform.position = state.tile.transform.position;
                ring.gameObject.SetActive(true);
            });
            hop.Append(state.tile.transform.DOMoveY(startY + height + 0.25f, 0.14f * TimeMultiplier).SetEase(Ease.OutSine));
            hop.Join(state.visual.DOScale(0, 0.14f * TimeMultiplier).SetEase(Ease.InSine));
            hop.Join(ring.transform.DOScale(1.6f, 0.14f * TimeMultiplier).SetEase(Ease.OutQuad));
            hop.Join(DOTween.To(() => 0.9f, alpha =>
            {
                var color = new Color(0.65f, 0.95f, 1, alpha);
                ring.startColor = ring.endColor = color;
            }, 0, 0.14f * TimeMultiplier));
            foreach (var highlight in state.highlights)
                hop.Join(highlight.DOFade(0, 0.14f * TimeMultiplier));
            hop.AppendCallback(() => trail.emitting = false);
            sequence.Insert(HighlightDuration + i * HopStagger, hop);
        }
        // Include the star fade in the fixed total duration.
        sequence.AppendInterval(Mathf.Max(0, Duration - sequence.Duration()));
        sequence.OnComplete(() =>
        {
            sequence = null;
            ResetEffect(false);
            onComplete?.Invoke();
        });
        return sequence;
    }

    TrailRenderer CreateLightTrail(Transform tile, int order)
    {
        var go = new GameObject("Hint turquoise light trail", typeof(TrailRenderer));
        go.transform.SetParent(tile, false);
        var trail = go.GetComponent<TrailRenderer>();
        trail.sharedMaterial = starMaterial;
        trail.time = 0.15f * TimeMultiplier;
        trail.minVertexDistance = 0.02f;
        trail.startWidth = 0.24f;
        trail.endWidth = 0;
        trail.startColor = new Color(0.7f, 1, 1, 0.75f);
        trail.endColor = new Color(0.6f, 0.65f, 1, 0);
        trail.sortingLayerName = SortingLayer;
        trail.sortingOrder = order - 1;
        trail.emitting = false;
        visuals.Add(go);
        return trail;
    }

    LineRenderer CreatePopRing(int order)
    {
        var go = new GameObject("Hint sparkle halo", typeof(LineRenderer));
        go.transform.SetParent(transform, false);
        go.transform.localScale = Vector3.one * 0.4f;
        var ring = go.GetComponent<LineRenderer>();
        ring.sharedMaterial = starMaterial;
        ring.useWorldSpace = false;
        ring.loop = true;
        ring.positionCount = 40;
        ring.widthMultiplier = 0.045f;
        ring.sortingLayerName = SortingLayer;
        ring.sortingOrder = order;
        for (int i = 0; i < ring.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2 / ring.positionCount;
            ring.SetPosition(i, new Vector3(Mathf.Cos(angle) * 0.55f, Mathf.Sin(angle) * 0.4f, 0));
        }
        go.SetActive(false);
        visuals.Add(go);
        return ring;
    }

    void AddHighlight(TileState state, SpriteRenderer source, bool additive, float scale)
    {
        if (!source || !source.sprite || !source.enabled || !source.gameObject.activeInHierarchy) return;
        var go = new GameObject(additive ? "Hint artwork light" : "Hint outer halo", typeof(SpriteRenderer));
        go.transform.SetParent(source.transform, false);
        go.transform.localScale = Vector3.one * scale;
        var renderer = go.GetComponent<SpriteRenderer>();
        renderer.sprite = source.sprite;
        renderer.drawMode = source.drawMode;
        renderer.size = source.size;
        renderer.flipX = source.flipX;
        renderer.flipY = source.flipY;
        renderer.color = additive ? new Color(1, 0.97f, 0.86f, 0) : new Color(0.65f, 0.95f, 1, 0);
        if (additive) renderer.sharedMaterial = highlightMaterial;
        renderer.sortingLayerName = SortingLayer;
        renderer.sortingOrder = source.sortingOrder + (additive ? 1 : -2);
        state.highlights.Add(renderer);
        visuals.Add(go);
    }

    void SpawnStars(Vector3 position, int order, int count)
    {
        var dust = TileOnRuntimeFx.CreateBurstParticle(transform, "Hint rising stars",
            new Color(1, 0.9f, 0.4f), count, 0.42f, 0.4f, 0.1f, 0.27f,
            count > 10 ? 1.65f : 0.85f, SortingLayer, order);
        dust.transform.position = position;
        var renderer = dust.GetComponent<ParticleSystemRenderer>();
        var createdMaterial = renderer.sharedMaterial;
        if (!starMaterial) starMaterial = createdMaterial;
        else
        {
            renderer.sharedMaterial = starMaterial;
            if (createdMaterial) Destroy(createdMaterial);
        }
        renderer.renderMode = ParticleSystemRenderMode.Mesh;
        renderer.mesh = TileOnRuntimeFx.StarParticleMesh();
        var main = dust.main;
        main.simulationSpeed = GameController.Match3EffectSpeed;
        main.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
        var velocity = dust.velocityOverLifetime;
        velocity.enabled = true;
        velocity.y = 0.6f;
        var colors = dust.colorOverLifetime;
        colors.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0),
            new GradientColorKey(new Color(0.5f, 0.92f, 1), 1) },
            new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) });
        colors.color = gradient;
        var size = dust.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.EaseInOut(0, 1, 1, 0));
        visuals.Add(dust.gameObject);
        dust.Play(true);
        Destroy(dust.gameObject, 0.5f * TimeMultiplier);
    }

    void ResetEffect(bool restoreTiles)
    {
        sequence?.Kill();
        sequence = null;
        foreach (var state in tilesInFlight)
        {
            if (!state.tile) continue;
            state.tile.transform.localPosition = state.position;
            state.visual.localRotation = state.rotation;
            if (restoreTiles) state.visual.localScale = state.scale;
            foreach (var saved in state.renderers)
            {
                if (!saved.renderer) continue;
                saved.renderer.sortingLayerID = saved.layer;
                saved.renderer.sortingOrder = saved.order;
            }
        }
        tilesInFlight.Clear();
        foreach (var go in visuals)
            if (go) Destroy(go);
        visuals.Clear();
    }

    void OnDisable() => ResetEffect(true);
    void OnDestroy()
    {
        ResetEffect(true);
        if (starMaterial) Destroy(starMaterial);
        if (highlightMaterial) Destroy(highlightMaterial);
    }
}
