using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PoolObjectSpriteRenderer : PoolObject
{
    public SpriteRenderer spriteRenderer;
    public Color color;
    public Vector3 scale;
    public Vector3 euler;
    private void OnValidate()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        color = spriteRenderer.color;
        scale = transform.localScale;
        euler = transform.localEulerAngles;
    }
    public override void Inactive()
    {
        base.Inactive();
        spriteRenderer.color = color;
        transform.localScale = scale;
        transform.localEulerAngles = euler;
    }
    public override void Active(float? timeActive = null)
    {
        base.Active(timeActive);
    }
}
