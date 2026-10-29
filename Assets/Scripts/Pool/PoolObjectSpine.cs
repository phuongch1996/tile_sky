using Spine.Unity;
using UnityEngine;

[RequireComponent(typeof(SkeletonAnimation))]
public class PoolObjectSpine : PoolObject
{
    public SkeletonAnimation skeletonAnimation;
    public string animName;
    public bool loop;
    public Vector3 scale;
    private void OnValidate()
    {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        animName = skeletonAnimation.AnimationName;
        loop = skeletonAnimation.loop;
        scale = transform.localScale;
    }
    public override void Inactive()
    {
        base.Inactive();
        transform.localScale = scale;
    }
    public override void Active(float? timeActive = null)
    {
        base.Active(timeActive);

        skeletonAnimation.AnimationState.SetAnimation(0, animName, loop);
    }
}
