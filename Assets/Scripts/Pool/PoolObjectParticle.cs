using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class PoolObjectParticle : PoolObject
{
    public ParticleSystem particle;
    public Vector3 scale;
    private void OnValidate()
    {
        particle = GetComponent<ParticleSystem>();
        scale = transform.localScale;
    }
    public override void Inactive()
    {
        base.Inactive();
        particle.Stop();
        transform.localScale = scale;
    }
    public override void Active(float? timeActive = null)
    {
        base.Active(timeActive);
        particle.Play(true);
    }
}
