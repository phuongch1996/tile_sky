/// <summary>Retains prefab/pool references without creating claimable Quest rewards.</summary>
public class QuestObject : PoolObjectUI
{
    public override void Active(float? timeActive = null)
    {
        Inactive();
    }
}