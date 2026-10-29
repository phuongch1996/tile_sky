using DG.Tweening;
using Spine.Unity;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PopupBonus : Popup
{
    public TextMeshProUGUI txGoldsClaim;
    public Button btOpenPresent;
    public GameObject buttonsGO;
    public Button btCollect;
    public Button btCollectAds;
    bool daily;
    bool presentOpened;
    int goldsGain;
    Action callback;
    public void OpenBonus(bool isDailyBonus, Action callback = null)
    {
        btOpenPresent.transform.GetComponentInChildren<SkeletonGraphic>().DOFade(1, 0);
        btOpenPresent.onClick.RemoveAllListeners();
        btCollect.onClick.RemoveAllListeners();
        btCollectAds.onClick.RemoveAllListeners();
        btOpenPresent.interactable = true;
        btCollect.interactable = false;
        btCollectAds.interactable = false;
        goldsGain = 0;
        txGoldsClaim.transform.localScale = Vector3.zero;
        daily = isDailyBonus;
        this.callback = callback;
        foreach (var img in buttonsGO.GetComponentsInChildren<MaskableGraphic>())
        {
            img.DOFade(0, 0);
        }
        buttonsGO.SetActive(false);
        Open();
    }
    public override void Open()
    {
        base.Open();
    }
    public override void Close()
    {
        base.Close();
    }
    public override void SetupTweenCallback()
    {
        SetupTweenCallback(() =>
        {
            if (daily)
                btOpenPresent.onClick.AddListener(() => OpenPresent());
            else
                OpenPresent();
        });
    }
    void OpenPresent()
    {
        if (presentOpened)
            return;
        AudioManager.instance.PlaySFXOneShot(AudioManager.SFX.REWARD);
        presentOpened = true;
        btOpenPresent.interactable = false;
        btCollect.interactable = true;
        btCollectAds.interactable = true;
        btOpenPresent.transform.DOPunchPosition(Vector3.one, 1f);
        btOpenPresent.transform.GetComponentInChildren<SkeletonGraphic>().DOFade(0, 1);
        goldsGain = Random.Range(20, 51);
        txGoldsClaim.text = $"{goldsGain}";
        txGoldsClaim.transform.DOScale(Vector3.one * 2, 1).SetDelay(0.5f).OnComplete(() =>
        {
            if (!daily)
                Collect(false);
            else
            {
                buttonsGO.SetActive(true);
                foreach (var img in buttonsGO.GetComponentsInChildren<MaskableGraphic>())
                {
                    img.DOFade(1, 1);
                }
                buttonsGO.transform.DOScale(1, 1).OnComplete(() =>
                {
                    btCollect.onClick.AddListener(() => Collect(false));
                    btCollectAds.onClick.AddListener(() => GameConfig.ShowRewardedAd(6));
                });
            }
        });
    }
    public void Collect(bool ads)
    {
        if (ads)
        {
            goldsGain *= 3;
            txGoldsClaim.text = $"{goldsGain}";
        }
        btCollect.interactable = false;
        btCollectAds.interactable = false;
        //if (daily)
        //{
        //    //GameConfig.UseDaily
        //}
        if (HomeScript.instance)
            HomeScript.instance.TopBarUpdateGolds(txGoldsClaim.transform, goldsGain, 10, () => DoneCollect());
        if (GameScript.instance)
            GameScript.instance.TopBarUpdateGolds(txGoldsClaim.transform, goldsGain, 10, () => DoneCollect());
    }
    void DoneCollect()
    {
        presentOpened = false;
        Close();
        callback?.Invoke();
    }
}
