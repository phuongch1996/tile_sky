using UnityEngine;

public class PoolObjectUI : PoolObject
{
    public Vector3 scale;
    public virtual void OnValidate()
    {
        scale = transform.localScale;
    }
    public override void Inactive()
    {
        base.Inactive();
    }
    public override void Active(float? timeActive = null)
    {
        transform.localScale = scale;
        base.Active(timeActive);
    }
}
