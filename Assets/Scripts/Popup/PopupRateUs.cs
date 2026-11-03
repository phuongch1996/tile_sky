using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class PopupRateUs : Popup
{
    public Button btClose;
    public Button btPanel;
    public Button[] btStars;
    public Button btRate;
    public GameObject[] stars;
    public GameObject goldsGO;
    public TextMeshProUGUI txRateDesc;
    const string ratingKey = "rating";
    public override void SetupTweenCallback()
    {
        ApplyTileSkyLayout();
        SetupTweenCallback(() =>
        {
            btClose.onClick.AddListener(() => Close());
            btPanel.onClick.AddListener(() => Close());
            btRate.onClick.AddListener(() => Rate());
            for (int i = 0; i < 5; i++)
            {
                StarsButton(btStars[i], i);
            }
        });
    }
    void SetStars(int rate)
    {
        this.rate = rate;
        for (int i = 0; i < 5; i++)
        {
            stars[i].SetActive(i < rate);
        }
    }
    int rate = 4;
    public static bool HasRating()
    {
        return PlayerPrefs.GetInt(ratingKey, 0) != 0;
    }
    public static void SetRating(int rate)
    {
        PlayerPrefs.SetInt(ratingKey, rate);
    }
    public bool WinOpen()
    {
#if UNITY_IOS
        return false;
#endif
        //if (PlayerPrefs.GetInt(ratingKey, 0) != 0)
        if (HasRating())
            return false;
        showTopBar = false;
        Open();
        return true;
    }
    public override void Open()
    {
        ApplyTileSkyLayout();
        btClose.interactable = true;
        btPanel.interactable = true;
        btRate.interactable = true;
        btClose.onClick.RemoveAllListeners();
        btPanel.onClick.RemoveAllListeners();
        btRate.onClick.RemoveAllListeners();
        foreach (var bt in btStars)
        {
            bt.interactable = true;
            bt.onClick.RemoveAllListeners();
        }
        //goldsGO.SetActive(PlayerPrefs.GetInt(ratingKey, 0) == 0);
        goldsGO.SetActive(!HasRating());
        txRateDesc.text = "Are you enjoying Tile Space?\nLeave us a 5 stars rating";
        SetStars(4);
        base.Open();
    }
    public override void Close()
    {
        if (HomeScript.instance)
            HomeScript.instance.settingsPopup.ToggleRate();
        if (GameScript.instance)
            GameScript.instance.settingsPopup.ToggleRate();
        base.Close();
    }
    bool isRate;
    void Rate()
    {
        btClose.interactable = false;
        btPanel.interactable = false;
        btRate.interactable = false;
        foreach (var bt in btStars)
            bt.interactable = false;
        if (rate >= 4)
        {
            isRate = true;
            GameConfig.SetDisableCommonAd(true);
            Application.OpenURL("https://play.google.com/store/apps/details?id=com.vik.tilespace&hl=vi");
            return;
        }
        DoneRate();
    }
    void DoneRate()
    {
        //if (PlayerPrefs.GetInt(ratingKey, 0) == 0)
        if (!HasRating())
        {
            //txRateDesc.text = "Thank you so much\nfor your rating!";
            ToastScript.instance.Toast(6);
            //PlayerPrefs.SetInt(ratingKey, rate);
            SetRating(rate);
            if (HomeScript.instance)
                HomeScript.instance.TopBarUpdateGolds(goldsGO.transform, 300, 75);
            if (GameScript.instance)
                GameScript.instance.TopBarUpdateGolds(goldsGO.transform, 300, 75);
            Invoke(nameof(Close), 1);
        }
        else
            Close();
    }
    private void OnApplicationPause(bool pause)
    {
        if (!pause && isRate)
        {
            isRate = false;
            GameConfig.SetDisableCommonAd(false);
            DoneRate();
        }
    }
    void StarsButton(Button bt, int star)
    {
        bt.onClick.AddListener(() => SetStars(star + 1));
    }
}
