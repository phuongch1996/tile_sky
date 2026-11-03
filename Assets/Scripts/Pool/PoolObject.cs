using UnityEngine;

public class PoolObject : MonoBehaviour
{
    [HideInInspector] public Transform poolTfm;
    public ObjPoolerStatus PoolerStatus { get; set; }
    public virtual void Inactive()
    {
        CancelInvoke();
        gameObject.SetActive(false);
        PoolerStatus = ObjPoolerStatus.Inactive;
    }
    public virtual void Active(float? timeActive = null)
    {
        gameObject.SetActive(true);
        PoolerStatus = ObjPoolerStatus.Active;
        if (timeActive.HasValue)
        {
            Invoke(nameof(ReleaseObj), timeActive.Value);
        }
    }
    void ReleaseObj()
    {
        PoolManager.instance.ReleaseObject(this);
    }
}

public enum ObjPoolerStatus
{
    DontKnow,
    Active,
    Inactive,
}
