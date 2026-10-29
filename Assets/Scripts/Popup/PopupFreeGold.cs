using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupFreeGold : Popup
{
    public Image fillImage;
    public TextMeshProUGUI txFill;
    public Button btWatchAds;
    public Button btClose;
    public Button btPanel;
    public Transform goldsTfm;

    public override void SetupTweenCallback()
    {
        SetupTweenCallback(() =>
        {
            btWatchAds.onClick.AddListener(() =>
            {
                if (GameConfig.GetFreeCoins() > 0)
                    GameConfig.ShowRewardedAd(7);
            });
            btClose.onClick.AddListener(() => Close());
            btPanel.onClick.AddListener(() => Close());
        });
    }
    public override void Open()
    {
        btWatchAds.onClick.RemoveAllListeners();
        btClose.onClick.RemoveAllListeners();
        btPanel.onClick.RemoveAllListeners();
        SetupFreeGold();
        base.Open();
    }
    void SetupFreeGold()
    {
        int freeCoins = GameConfig.GetFreeCoins();
        fillImage.fillAmount = (float)freeCoins / GameConfig.freeCoinsMaxDaily;
        txFill.text = $"{freeCoins}/{GameConfig.freeCoinsMaxDaily}";
    }
    //public override void Close()
    //{
    //    base.Close();
    //}
    public void DoneAds()
    {
        if (GameConfig.GetFreeCoins() > 0)
        {
            GameConfig.UseFreeCoins();
            btWatchAds.interactable = false;
            SetupFreeGold();
            HomeScript.instance.TopBarUpdateGolds(goldsTfm, 150, 25, () => { btWatchAds.interactable = true; });
        }
    }
}
