using UnityEngine;
using UnityEngine.UI;

public class MapObject : PoolObjectUI
{
    public Button bt;
    public Image img;
    public GameObject selected;
    public GameObject locked;
    public GameObject lockedAds;
    public GameObject redDot;
    public bool unlock;
    //public bool isLockAds;
    public int bgType;
    public int bgIndex;
    //public void RedDot(bool active)
    //{
    //    redDot.SetActive(active);
    //}
    //public override void Inactive()
    //{
    //    base.Inactive();
    //    //bt.onClick.RemoveAllListeners();
    //}
    //public override void Active(float? timeActive = null)
    //{
    //    base.Active(timeActive);
    //}
    public void Unlock(bool unlock, bool ads = false, bool dot = false)
    {
        this.unlock = unlock;
        locked.SetActive(!this.unlock);
        //lockedAds.SetActive(ads);
        //if (ads)
        //    redDot.SetActive(dot && unlock);
        //else
        //    redDot.SetActive(false);
    }
    public void Select(bool select)
    {
        selected.SetActive(select);
    }
}
