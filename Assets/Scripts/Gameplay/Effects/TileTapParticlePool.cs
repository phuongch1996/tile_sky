using UnityEngine;

public class TileTapParticlePool : MonoBehaviour
{
    public static TileTapParticlePool instance;

    [SerializeField] private ParticleSystem particlePrefab;
    [SerializeField] private int poolSize = 8;
    [SerializeField] private Vector3 positionOffset = new(0f, 0f, -0.2f);

    private ParticleSystem[] pool;
    private int nextIndex;

    private void Awake()
    {
        instance = this;
        RebuildPool();
    }

    public void SetParticlePrefab(ParticleSystem prefab)
    {
        if (prefab == null)
            return;

        if (particlePrefab == prefab && pool != null && pool.Length > 0)
            return;

        particlePrefab = prefab;
        RebuildPool();
    }

    public void PlayAt(Vector3 worldPosition)
    {
        if (pool == null || pool.Length == 0)
            RebuildPool();

        ParticleSystem particle = GetAvailableParticle();
        particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        TileOnRuntimeFx.SetRendererSorting(particle.gameObject, TileOnRuntimeFx.HoveringSortingLayer, TileOnRuntimeFx.TileFxSortingOrder);
        particle.transform.position = worldPosition + positionOffset;
        particle.Play(true);
    }

    private void RebuildPool()
    {
        ClearPool();
        poolSize = Mathf.Max(1, poolSize);
        pool = new ParticleSystem[poolSize];

        for (int i = 0; i < poolSize; i++)
            pool[i] = CreateParticle(i);
    }

    private ParticleSystem CreateParticle(int index)
    {
        ParticleSystem particle = particlePrefab != null
            ? Instantiate(particlePrefab, transform)
            : TileOnRuntimeFx.CreateBurstParticle(
                transform,
                $"Tile Tap Particle {index}",
                new Color(1f, 0.9f, 0.05f, 1f),
                48,
                0.6f,
                0.55f,
                0.24f,
                0.48f,
                3.15f,
                TileOnRuntimeFx.HoveringSortingLayer,
                TileOnRuntimeFx.TileFxSortingOrder);

        particle.name = $"Tile Tap Particle {index}";
        ConfigureGoldStarBurst(particle);
        TileOnRuntimeFx.SetRendererSorting(particle.gameObject, TileOnRuntimeFx.HoveringSortingLayer, TileOnRuntimeFx.TileFxSortingOrder);

        ParticleSystem.MainModule main = particle.main;
        main.playOnAwake = false;
        main.loop = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        return particle;
    }

    private void ConfigureGoldStarBurst(ParticleSystem particle)
    {
        ParticleSystem.MainModule main = particle.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 0.6f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.38f, 0.65f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2.3f, 4.15f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.28f, 0.55f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 1f, 0.28f, 1f),
            new Color(1f, 0.86f, 0.02f, 1f));
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.05f;

        ParticleSystem.EmissionModule emission = particle.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, 26),
            new ParticleSystem.Burst(0.04f, 15),
            new ParticleSystem.Burst(0.09f, 9)
        });

        ParticleSystem.ShapeModule shape = particle.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.3f;
        shape.radiusThickness = 0.62f;

        ParticleSystem.VelocityOverLifetimeModule velocity = particle.velocityOverLifetime;
        velocity.enabled = false;
        velocity.space = ParticleSystemSimulationSpace.Local;
        // Unity requires all orbital axes to share a curve mode, even zero axes.
        velocity.orbitalX = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocity.orbitalY = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocity.orbitalZ = new ParticleSystem.MinMaxCurve(-0.75f, 0.75f);
        velocity.enabled = true;

        ParticleSystem.RotationOverLifetimeModule rotation = particle.rotationOverLifetime;
        rotation.enabled = true;
        rotation.z = new ParticleSystem.MinMaxCurve(-18f, 18f);

        ParticleSystem.SizeOverLifetimeModule size = particle.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.42f),
                new Keyframe(0.12f, 1.38f),
                new Keyframe(1f, 0f)));

        ParticleSystem.ColorOverLifetimeModule color = particle.colorOverLifetime;
        color.enabled = true;
        Gradient gradient = new();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(1f, 1f, 0.75f), 0f),
                new GradientColorKey(new Color(1f, 0.92f, 0.05f), 0.55f),
                new GradientColorKey(new Color(1f, 0.72f, 0.02f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.06f),
                new GradientAlphaKey(0f, 1f)
            });
        color.color = gradient;

        ParticleSystem.TrailModule trails = particle.trails;
        trails.enabled = true;
        trails.mode = ParticleSystemTrailMode.PerParticle;
        trails.ratio = 0.85f;
        trails.lifetime = 0.25f;
        trails.widthOverTrail = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(1f, 0f)));
        trails.colorOverTrail = gradient;

        ParticleSystemRenderer renderer = particle.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Mesh;
        renderer.mesh = TileOnRuntimeFx.StarParticleMesh();
        renderer.sortingLayerName = TileOnRuntimeFx.HoveringSortingLayer;
        renderer.sortingOrder = TileOnRuntimeFx.TileFxSortingOrder;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            Material material = new(shader)
            {
                name = "Gold Star Tap Particle Material"
            };
            material.color = Color.white;
            renderer.sharedMaterial = material;
            renderer.trailMaterial = material;
        }
    }

    private void ClearPool()
    {
        if (pool == null)
            return;

        foreach (ParticleSystem particle in pool)
        {
            if (particle != null)
                Destroy(particle.gameObject);
        }

        pool = null;
        nextIndex = 0;
    }

    private ParticleSystem GetAvailableParticle()
    {
        for (int i = 0; i < pool.Length; i++)
        {
            int index = (nextIndex + i) % pool.Length;
            if (!pool[index].IsAlive(true))
            {
                nextIndex = (index + 1) % pool.Length;
                return pool[index];
            }
        }

        ParticleSystem particle = pool[nextIndex];
        nextIndex = (nextIndex + 1) % pool.Length;
        return particle;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
