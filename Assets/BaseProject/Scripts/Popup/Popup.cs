using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum POPUP_TYPE
{
    box, fade, fadelist, none
}
//[Serializable]
//public class MaskableList
//{
//    public List<MaskableGraphic> maskables;
//}
public class Popup : MonoBehaviour
{
    public GameObject panel;
    public Image bg;
    public bool isBGChange;
    public POPUP_TYPE popupType;
    public float speed = 0.35f;
    [Header("Box")]
    public Transform box;
    [Header("Fade")]
    public List<GameObject> fadeIgnore;
    [Header("FadeList")]
    public List<GameObject> maskableList;
    public TweenCallback tweenCallback;
    [Space]
    public bool showTopBar;
    public bool Showing => panel.activeSelf;
    public event Action Closed;
    public virtual void Open()
    {
        if (showTopBar)
        {
            if (HomeScript.instance)
                HomeScript.instance.topBarScript.Top();
            if (GameScript.instance)
                GameScript.instance.topBarScript.Top();
        }
        switch (popupType)
        {
            case POPUP_TYPE.box:
                box.DOScale(0, speed).SetInverted(true).SetEase(Ease.InBack)
                    .OnStart(() => panel.SetActive(true))
                    .OnComplete(tweenCallback);
                break;
            case POPUP_TYPE.fade:
                List<MaskableGraphic> maskable = new(panel.GetComponentsInChildren<MaskableGraphic>().ToList());
                foreach (var ignore in fadeIgnore)
                {
                    maskable.RemoveAll(x => ignore.GetComponentsInChildren<MaskableGraphic>().Contains(x));
                }
                foreach (var mask in maskable)
                {
                    float a = mask.color.a;
                    mask.DOKill();
                    mask.DOFade(0, 0);
                    mask.DOFade(a, speed);
                }
                panel.transform.DOScale(1, speed).OnStart(() => panel.SetActive(true)).OnComplete(tweenCallback);
                break;
            case POPUP_TYPE.fadelist:
                for (int i = 0; i < maskableList.Count; i++)
                {
                    foreach (var mask in maskableList[i].GetComponentsInChildren<MaskableGraphic>())
                    {
                        float a = mask.color.a;
                        mask.DOKill();
                        mask.DOFade(0, 0);
                        mask.DOFade(a, speed).SetDelay(i * speed);
                    }
                }
                panel.transform.DOScale(1, maskableList.Count * speed).OnStart(() => panel.SetActive(true)).OnComplete(tweenCallback);
                break;
            case POPUP_TYPE.none:
                panel.SetActive(true);
                tweenCallback?.Invoke();
                break;
        }
    }
    public virtual void Close()
    {
        bool wasShowing = Showing;
        if (showTopBar)
        {
            if (HomeScript.instance)
                HomeScript.instance.topBarScript.Bottom();
            if (GameScript.instance)
                GameScript.instance.topBarScript.Bottom();
        }
        panel.SetActive(false);
        if (wasShowing)
            Closed?.Invoke();
    }
    public virtual void SetupTweenCallback()
    {

    }
    public void SetupTweenCallback(TweenCallback callback)
    {
        tweenCallback = callback;
    }
}
