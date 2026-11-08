using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class PopupLose : Popup
{
    public GameObject canRevive;
    public GameObject noRevive;
    public GameObject lose1;
    public Button btReviveGold;
    public Button btReviveAds;
    public TextMeshProUGUI txTimeCounter;
    public Image imgCounterFill;
    public GameObject lose2;
    public Button btRestart;
    public Button btRestartBlank;
    //public Button btHome;
    public RectTransform fxTfm;
    public override void SetupTweenCallback()
    {
        SetupTweenCallback(() =>
        {
            btReviveGold.onClick.AddListener(() => OnClickRevive(false));
            btReviveAds.onClick.AddListener(() => OnClickRevive(true));
            btRestart.onClick.AddListener(() => Restart());
            btRestartBlank.onClick.AddListener(() => Restart());
            //btHome.onClick.AddListener(() => Home());
            if (lose1.activeSelf)
            {
                imgCounterFill.DOFillAmount(1, 5).SetEase(Ease.Linear)
                    .OnUpdate(UpdateReviveCountdown).OnComplete(() =>
                {
                    Restart();
                });
            }
        });
    }

    void UpdateReviveCountdown()
    {
        float progress = imgCounterFill.fillAmount;
        txTimeCounter.text = Mathf.Clamp(Mathf.FloorToInt(progress * 5) + 1, 1, 5).ToString();
        float angle = progress * Mathf.PI * 2;
        fxTfm.anchoredPosition = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * 270;
    }
    public override void Open()
    {
        EnsureHeavenlyLose();
        FitHeavenlyLose();
        imgCounterFill.DOKill();
        btReviveGold.onClick.RemoveAllListeners();
        btReviveAds.onClick.RemoveAllListeners();
        btRestart.onClick.RemoveAllListeners();
        btRestartBlank.onClick.RemoveAllListeners();
        fxTfm.anchoredPosition = new Vector2(0, 270);
        //btHome.onClick.RemoveAllListeners();
        AudioManager.instance.PlaySFXOneShot(AudioManager.SFX.LOSE);
        if (!GameConfig.isChallengeLevel)
        {
            EventManager.instance.StreakLose();
        }
        lose1.SetActive(false);
        canRevive.SetActive(false);
        lose2.SetActive(false);
        noRevive.SetActive(false);
        if (GameConfig.reviveCount < 2)
        {
            showTopBar = true;
            lose1.SetActive(true);
            canRevive.SetActive(true);
            imgCounterFill.fillAmount = 0;
            txTimeCounter.text = "1";
        }
        else
        {
            showTopBar = false;
            if (GameConfig.InterEndLevel())
            {
                GameConfig.ShowInterstitialAd();
            }
            lose2.SetActive(true);
            noRevive.SetActive(true);
        }
        base.Open();
    }
    public void OnClickRevive(bool ads)
    {
        if (ads)
        {
            GameConfig.ShowRewardedAd(5);
        }
        else
        {
            if (GameConfig.UseGold(200))
                Revive();
            else
            {
                ToastScript.instance.Toast(0);
            }
        }
    }
    public void Revive()
    {
        EventManager.instance.StreakRevive();
        imgCounterFill.DOKill();
        GameScript.instance.Revive();
        Close();
    }
    void Restart()
    {
        imgCounterFill.DOKill();
        GameScript.instance.Restart();
    }
    void Home()
    {
        imgCounterFill.DOKill();
        GameScript.instance.Home();
    }

    void OnDisable()
    {
        if (imgCounterFill) imgCounterFill.DOKill();
    }
}
