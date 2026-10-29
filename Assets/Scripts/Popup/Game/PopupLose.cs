using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupLose : Popup
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
                imgCounterFill.DOFillAmount(0, 5).OnUpdate(() =>
                {
                    //txTimeCounter.text = Mathf.RoundToInt((1 - imgCounterFill.fillAmount) * 5).ToString();
                    txTimeCounter.text = Mathf.RoundToInt(imgCounterFill.fillAmount * 5).ToString();
                    float y = Mathf.Cos(Mathf.Deg2Rad * imgCounterFill.fillAmount * 360);
                    float x = Mathf.Sin(Mathf.Deg2Rad * imgCounterFill.fillAmount * 360);
                    fxTfm.anchoredPosition = new Vector2(-x * 270, y * 270);
                }).OnComplete(() =>
                {
                    Restart();
                });
            }
        });
    }
    public override void Open()
    {
        btReviveGold.onClick.RemoveAllListeners();
        btReviveAds.onClick.RemoveAllListeners();
        btRestart.onClick.RemoveAllListeners();
        btRestartBlank.onClick.RemoveAllListeners();
        fxTfm.anchoredPosition = new Vector2(0, 270);
        //btHome.onClick.RemoveAllListeners();
        AudioManager.instance.PlaySFXOneShot(AudioManager.SFX.LOSE);
        if (!GameConfig.isChallengeLevel)
        {
            FirebaseManager.instance.LogLevelFail(GameConfig.level, GameConfig.GetActionDeltaTime(GameConfig.startLevelKey), GameConfig.loseCount + 1);
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
            imgCounterFill.fillAmount = 1;
            txTimeCounter.text = "5";
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
}
