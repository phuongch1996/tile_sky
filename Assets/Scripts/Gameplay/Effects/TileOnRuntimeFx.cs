using UnityEngine;

public static class TileOnRuntimeFx
{
    public const string HoveringSortingLayer = "Hovering";
    public const string SlottedSortingLayer = "Slotted";
    public const int TileFxSortingOrder = 4500;
    static Mesh starParticleMesh;

    public static ParticleSystem CreateBurstParticle(
        Transform parent,
        string name,
        Color color,
        int burstCount = 14,
        float duration = 0.45f,
        float startLifetime = 0.32f,
        float minSize = 0.08f,
        float maxSize = 0.18f,
        float startSpeed = 1.2f,
        string sortingLayerName = HoveringSortingLayer,
        int sortingOrder = 100)
    {
        GameObject particleObject = new(name);
        bool wasActive = particleObject.activeSelf;
        particleObject.SetActive(false);

        if (parent != null)
            particleObject.transform.SetParent(parent, false);

        ParticleSystem particle = particleObject.AddComponent<ParticleSystem>();
        ParticleSystemRenderer renderer = particleObject.GetComponent<ParticleSystemRenderer>();

        ParticleSystem.MainModule main = particle.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = duration;
        main.startLifetime = startLifetime;
        main.startSpeed = startSpeed;
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.startColor = new ParticleSystem.MinMaxGradient(color, Color.white);
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = particle.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)burstCount) });

        ParticleSystem.ShapeModule shape = particle.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.18f;

        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingLayerName = sortingLayerName;
        renderer.sortingOrder = sortingOrder;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            Material material = new(shader)
            {
                name = $"{name} Material"
            };
            renderer.sharedMaterial = material;
        }

        particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particleObject.SetActive(wasActive);
        return particle;
    }

    public static Transform VisualRoot(ItemTile tile)
    {
        if (tile == null)
            return null;
        if (tile.sprites != null)
            return tile.sprites;
        if (tile.transform.childCount > 0)
            return tile.transform.GetChild(0);
        return tile.transform;
    }

    public static void SetRendererSorting(GameObject target, string sortingLayerName, int sortingOrder)
    {
        if (target == null)
            return;

        foreach (Renderer renderer in target.GetComponentsInChildren<Renderer>(true))
        {
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = sortingOrder;
        }
    }

    public static Mesh StarParticleMesh()
    {
        if (starParticleMesh != null)
            return starParticleMesh;

        const int points = 5;
        Vector3[] vertices = new Vector3[points * 2 + 1];
        int[] triangles = new int[points * 2 * 3];
        vertices[0] = Vector3.zero;

        for (int i = 0; i < points * 2; i++)
        {
            float radius = i % 2 == 0 ? 0.5f : 0.22f;
            float angle = Mathf.Deg2Rad * (90f + i * 180f / points);
            vertices[i + 1] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
        }

        for (int i = 0; i < points * 2; i++)
        {
            int triangleIndex = i * 3;
            triangles[triangleIndex] = 0;
            triangles[triangleIndex + 1] = i + 1;
            triangles[triangleIndex + 2] = i == points * 2 - 1 ? 1 : i + 2;
        }

        starParticleMesh = new Mesh
        {
            name = "Runtime Gold Star Particle Mesh",
            vertices = vertices,
            triangles = triangles
        };
        starParticleMesh.RecalculateBounds();
        starParticleMesh.RecalculateNormals();
        return starParticleMesh;
    }
}
