using System;
using TMPro;
using UnityEngine.UI;

public class PopupMapPreview : Popup
{
    public TextMeshProUGUI txTitle;
    public TextMeshProUGUI txUnlockAt;
    public TextMeshProUGUI txSetBG;
    public Button btSetBG;
    public Button btWatchAds;
    public Button btNextLevel;
    public Button btCancel;
    int bgIndex;
    int bgType;
    Action callback;
    public void OpenMapPreview(int bgIndex, int bgType, bool unlock, Action callback = null, bool hideTitle = false)
    {
        this.bgIndex = bgIndex;
        this.bgType = bgType;
        this.callback = callback;
        btSetBG.onClick.RemoveAllListeners();
        btWatchAds.onClick.RemoveAllListeners();
        btNextLevel.onClick.RemoveAllListeners();
        btCancel.onClick.RemoveAllListeners();
        btCancel.gameObject.SetActive(HomeScript.instance);
        btNextLevel.gameObject.SetActive(GameScript.instance);
        if (unlock)
        {
            btSetBG.gameObject.SetActive(true);
            btWatchAds.gameObject.SetActive(false);
            txUnlockAt.gameObject.SetActive(false);
        }
        else
        {
            btSetBG.gameObject.SetActive(false);
            btWatchAds.gameObject.SetActive(bgType != 0);
            txUnlockAt.gameObject.SetActive(bgType == 0);
            txUnlockAt.text = $"Unlock at level {GameConfig.GetUnlockBGLevel(bgIndex)}";
        }
        txSetBG.text = (GameConfig.GetBGType() == bgType && GameConfig.GetCurrentBackground() == bgIndex) ? "continue" : "set background";
        if (HomeScript.instance)
        {
            BGSpritesSO bgSpritesSO = HomeScript.instance.canvasBG.bgSpritesSO;
            bg.sprite = bgSpritesSO.GetBGSprite(bgIndex);
            txTitle.text = bgSpritesSO.GetBGTitle(bgIndex);
            HomeScript.instance.canvasBG.OnBGChange?.Invoke();
        }
        if (GameScript.instance)
        {
            BGSpritesSO bgSpritesSO = GameScript.instance.canvasBG.bgSpritesSO;
            bg.sprite = bgSpritesSO.GetBGSprite(bgIndex);
            txTitle.text = bgSpritesSO.GetBGTitle(bgIndex);
            GameScript.instance.canvasBG.OnBGChange?.Invoke();
        }
        txTitle.gameObject.SetActive(!hideTitle);
        Open();
    }
    public override void SetupTweenCallback()
    {
        SetupTweenCallback(() =>
        {
            btCancel.onClick.AddListener(() => Cancel());
            btNextLevel.onClick.AddListener(() => Cancel());
            btSetBG.onClick.AddListener(() => SetBG());
        });
    }
    public override void Open()
    {
        base.Open();
    }
    public override void Close()
    {
        if (callback != null)
        {
            callback.Invoke();
            return;
        }
        base.Close();
    }
    public void SetBG()
    {
        GameConfig.SetBGType(bgType);
        GameConfig.SetCurrentBackground(bgIndex);
        if (HomeScript.instance)
        {
            HomeScript.instance.canvasBG.SetBG();
            HomeScript.instance.mapPopup.SelectMap();
        }
        if (GameScript.instance)
            GameScript.instance.canvasBG.SetBG();
        Close();
    }
    //void WatchAds()
    //{
    //    GameConfig.ShowRewardedAd(9);
    //}
    //public void DoneAds()
    //{
    //    HomeScript.instance.mapPopup.UnlockBG(bgType, bgIndex);
    //    SetBG();
    //}
    public void Cancel()
    {
        //ClosePreview();
        Close();
    }
}
