using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

// Retain the component name/GUID for existing references; the visual is now sky magic.
public class MeteorImpactFx : MonoBehaviour
{
    const float GatherDuration = 0.32f * 7f / 8f / GameController.Match3EffectSpeed;
    const float ReleaseDuration = 0.08f * 7f / 8f / GameController.Match3EffectSpeed;
    readonly List<Tween> tweens = new();
    readonly List<Material> materials = new();
    Material lightMaterial;

    public static Tween Play(Vector3 targetPosition, Transform parent, string sortingLayerName,
        int sortingOrder, int sequenceIndex = 0)
    {
        var go = new GameObject("Sky Match - Starlight Bloom");
        if (parent) go.transform.SetParent(parent, false);
        go.transform.position = targetPosition;
        return go.AddComponent<MeteorImpactFx>().PlayBloom(sortingLayerName, sortingOrder, sequenceIndex);
    }

    Tween PlayBloom(string layer, int order, int index)
    {
        lightMaterial = new Material(Shader.Find("Sprites/Default")) { name = "Sky Match Light" };
        materials.Add(lightMaterial);
        Color sky = new Color(0.35f, 0.91f, 1f);
        Color gold = new Color(1f, 0.88f, 0.4f);
        var cyanRibbon = CreateRibbon("Turquoise light ribbon", sky, layer, order, 0);
        var goldRibbon = CreateRibbon("Golden light ribbon", gold, layer, order + 1, 180);
        foreach (var ribbon in new[] { cyanRibbon, goldRibbon })
        {
            ribbon.transform.localScale = Vector3.one * 1.25f;
            tweens.Add(ribbon.transform.DOScale(0.48f, GatherDuration).SetEase(Ease.InSine));
            tweens.Add(ribbon.transform.DOLocalRotate(new Vector3(0, 0,
                ribbon.transform.localEulerAngles.z + 110), GatherDuration, RotateMode.FastBeyond360).SetEase(Ease.OutSine));
        }

        var star = new GameObject("Wishing star", typeof(MeshFilter), typeof(MeshRenderer));
        star.transform.SetParent(transform, false);
        star.transform.localScale = Vector3.zero;
        star.GetComponent<MeshFilter>().sharedMesh = TileOnRuntimeFx.StarParticleMesh();
        var starRenderer = star.GetComponent<MeshRenderer>();
        var starMaterial = new Material(lightMaterial) { color = gold };
        materials.Add(starMaterial);
        starRenderer.sharedMaterial = starMaterial;
        starRenderer.sortingLayerName = layer;
        starRenderer.sortingOrder = order + 3;
        tweens.Add(star.transform.DOScale(0.55f, GatherDuration).SetEase(Ease.OutBack));
        tweens.Add(star.transform.DOLocalRotate(new Vector3(0, 0, 35 + index * 12), GatherDuration));

        var dust = CreateSkyDust("Rising stardust", sky, 14, layer, order + 4);
        var glints = CreateSkyDust("Golden wishes", gold, 7, layer, order + 5);
        var halo = CreateRibbon("Soft expanding halo", sky, layer, order + 2, 0, true);
        halo.gameObject.SetActive(false);

        Sequence sequence = DOTween.Sequence()
            .AppendInterval(GatherDuration)
            .AppendCallback(() =>
            {
                cyanRibbon.enabled = goldRibbon.enabled = false;
                dust.Play(true);
                glints.Play(true);
                halo.gameObject.SetActive(true);
                halo.transform.localScale = Vector3.one * 0.4f;
                tweens.Add(halo.transform.DOScale(1.45f, 0.38f / GameController.Match3EffectSpeed).SetEase(Ease.OutCubic));
                tweens.Add(DOTween.To(() => 0.65f, alpha => SetRibbonAlpha(halo, sky, alpha), 0, 0.38f / GameController.Match3EffectSpeed));
                tweens.Add(star.transform.DOScale(0, 0.18f / GameController.Match3EffectSpeed).SetEase(Ease.InSine));
            })
            .AppendInterval(ReleaseDuration)
            // Clear the tile now; the dust finishes independently.
            .OnComplete(() => Destroy(gameObject, 0.85f / GameController.Match3EffectSpeed))
            .SetTarget(gameObject);
        tweens.Add(sequence);
        return sequence;
    }

    LineRenderer CreateRibbon(string objectName, Color color, string layer, int order, float angle, bool fullRing = false)
    {
        var go = new GameObject(objectName, typeof(LineRenderer));
        go.transform.SetParent(transform, false);
        go.transform.localRotation = Quaternion.Euler(0, 0, angle);
        var line = go.GetComponent<LineRenderer>();
        line.sharedMaterial = lightMaterial;
        line.useWorldSpace = false;
        line.positionCount = 40;
        line.loop = fullRing;
        line.numCapVertices = 4;
        line.widthMultiplier = fullRing ? 0.045f : 0.065f;
        line.widthCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        line.sortingLayerName = layer;
        line.sortingOrder = order;
        SetRibbonAlpha(line, color, 0.8f);
        for (int i = 0; i < line.positionCount; i++)
        {
            float phase = i / (float)(fullRing ? line.positionCount : line.positionCount - 1);
            float radians = phase * Mathf.PI * (fullRing ? 2 : 1.45f);
            line.SetPosition(i, new Vector3(Mathf.Cos(radians) * 0.75f, Mathf.Sin(radians) * 0.5f, 0));
        }
        return line;
    }

    static void SetRibbonAlpha(LineRenderer line, Color color, float alpha)
    {
        var gradient = new Gradient();
        gradient.SetKeys(new[] { new GradientColorKey(color, 0), new GradientColorKey(Color.white, 0.5f),
            new GradientColorKey(color, 1) }, new[] { new GradientAlphaKey(0, 0),
            new GradientAlphaKey(alpha, 0.5f), new GradientAlphaKey(0, 1) });
        line.colorGradient = gradient;
    }

    ParticleSystem CreateSkyDust(string objectName, Color color, int count, string layer, int order)
    {
        var dust = TileOnRuntimeFx.CreateBurstParticle(transform, objectName, color, count,
            0.65f, 0.7f, 0.07f, 0.18f, 0.65f, layer, order);
        var renderer = dust.GetComponent<ParticleSystemRenderer>();
        materials.Add(renderer.sharedMaterial);
        renderer.renderMode = ParticleSystemRenderMode.Mesh;
        renderer.mesh = TileOnRuntimeFx.StarParticleMesh();
        var main = dust.main;
        main.simulationSpeed = GameController.Match3EffectSpeed;
        main.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
        main.gravityModifier = -0.12f;
        var velocity = dust.velocityOverLifetime;
        velocity.enabled = true;
        velocity.y = 0.65f;
        var fade = dust.colorOverLifetime;
        fade.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(color, 1) },
            new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, 0.12f), new GradientAlphaKey(0, 1) });
        fade.color = gradient;
        var size = dust.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, 0.3f),
            new Keyframe(0.2f, 1), new Keyframe(1, 0)));
        return dust;
    }

    void OnDestroy()
    {
        foreach (var tween in tweens) tween?.Kill();
        foreach (var material in materials)
            if (material) Destroy(material);
    }
}
