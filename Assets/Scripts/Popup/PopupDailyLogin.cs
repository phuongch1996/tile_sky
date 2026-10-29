using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupDailyLogin : Popup
{
    public readonly static List<DailyPrize> dailyLoginPrize = new()
    {
        new DailyPrize(100),
        new DailyPrize(200),
        new DailyPrize(150),
        new DailyPrize(300),
        new DailyPrize(200),
        new DailyPrize(300),
        new DailyPrize(400),
        new DailyPrize(0,1),
    };
    public GameObject nextRewardGO;
    public TextMeshProUGUI txNextReward;
    public Button btClose;
    public Button btClaim;
    public Button btAds;
    public DailyObject[] dailyObs;
    public override void SetupTweenCallback()
    {
        SetupTweenCallback(() =>
        {
            btClose.onClick.AddListener(() => Close());
            btAds.onClick.AddListener(() => GameConfig.ShowRewardedAd(1));
            btClaim.onClick.AddListener(() => ClaimPrize(false));
            foreach (var dailyObs in dailyObs)
                dailyObs.btDaily.onClick.AddListener(() => ClaimPrize(false));
        });
    }
    private void Awake()
    {
        for (int i = 0; i < dailyObs.Length; i++)
        {
            dailyObs[i].InitPrize(dailyLoginPrize[i]);
        }
    }
    public override void Open()
    {
        SetupDailyObs();
        btClose.onClick.RemoveAllListeners();
        btClaim.onClick.RemoveAllListeners();
        btAds.onClick.RemoveAllListeners();
        foreach (var dailyObs in dailyObs)
            dailyObs.btDaily.onClick.RemoveAllListeners();
        base.Open();
    }
    private void Update()
    {
        if (nextRewardGO.activeSelf && GameConfig.GetDailyLoginDate() >= GameConfig.GetCurrentDate())
        {
            TimeSpan ts = TimeManager.instance.TimeToNextDay();
            txNextReward.text = "next reward in\n" + $"{Mathf.Floor((float)ts.TotalHours):00}:{ts:mm}:{ts:ss}";
        }
    }
    void SetupDailyObs()
    {
        int dailyLogin = GameConfig.GetDailyLogin();
        for (int i = 0; i < dailyObs.Length; i++)
        {
            if (i < dailyLogin)
            {
                dailyObs[i].Init(2, $"{i + 1}");
                if (i == dailyObs.Length - 1)
                {
                    btClaim.gameObject.SetActive(false);
                    btAds.gameObject.SetActive(GameConfig.GetDailyLoginDateAds() < GameConfig.GetCurrentDate());
                    nextRewardGO.SetActive(true);
                }
            }
            else if (i == dailyLogin)
            {
                if (GameConfig.GetDailyLoginDate() < GameConfig.GetCurrentDate())
                {
                    dailyObs[i].Init(1, $"{i + 1}");
                    btClaim.gameObject.SetActive(true);
                    btAds.gameObject.SetActive(false);
                    nextRewardGO.SetActive(true);
                    txNextReward.text = "CLAIM NOW!!!";
                }
                else
                {
                    dailyObs[i].Init(0, $"{i + 1}");
                    btClaim.gameObject.SetActive(false);
                    btAds.gameObject.SetActive(GameConfig.GetDailyLoginDateAds() < GameConfig.GetCurrentDate());
                    nextRewardGO.SetActive(true);
                }
            }
            else
            {
                dailyObs[i].Init(0, $"{i + 1}");
            }
        }
    }
    public void ClaimPrize(bool ads)
    {
        int dailyLogin = GameConfig.GetDailyLogin();
        if (!ads)
        {
            GameConfig.SetDailyLogin(dailyLogin + 1);
            GameConfig.SetDailyLoginDate(GameConfig.GetCurrentDate());
            GameConfig.SetDailyLoginTotal();
            QuestManager.QuestComplete(QUEST_TYPE.CLAIM_X_DAILY, 1);
        }
        else
        {
            dailyLogin -= 1;
            GameConfig.SetDailyLoginDateAds(GameConfig.GetCurrentDate());
        }
        SetupDailyObs();
        if (dailyLoginPrize[dailyLogin].goldAm > 0)
            GameConfig.AddGold(dailyLoginPrize[dailyLogin].goldAm);
        if (dailyLoginPrize[dailyLogin].boosterAm > 0)
            GameConfig.AddAllPowerUps(dailyLoginPrize[dailyLogin].boosterAm);
        GameObject go = Instantiate(dailyObs[dailyLogin].prizeGO, dailyObs[dailyLogin].transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.DOBlendableLocalMoveBy(new Vector3(0, 100), 1).OnStart(() =>
        {
            MaskableGraphic[] graphics = go.GetComponentsInChildren<MaskableGraphic>();
            foreach (var graphic in graphics)
            {
                graphic.DOFade(0, 1 - Time.deltaTime);
            }
        }).OnComplete(() => { Destroy(go); });
    }
}

[Serializable]
public class DailyPrize
{
    public int goldAm;
    public int boosterAm;
    public DailyPrize(int goldAm = 0, int boosterAm = 0)
    {
        this.goldAm = goldAm;
        this.boosterAm = boosterAm;
    }
}
