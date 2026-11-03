using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.UI;

public partial class PopupRank : Popup
{
    public Button btClose;
    public Button btEditPlayer;
    public GameObject loadingGO;
    public GameObject loadFailedGO;
    public RankCard rankCardTemplate;
    public RankCard rankCardUserTemplate;

    [Header("daily")]
    public Toggle toggleDailyRank;
    public TextMeshProUGUI txBtDailyRank;
    public GameObject dailyTab;
    //public RankCard[] top3Daily;
    public Image[] top3DailyImgs;
    public RankCard currentUserDaily;
    public Transform dailyParent;
    public GameObject noDailyGO;
    [Header("alltime")]
    public Toggle toggleAllTimeRank;
    public TextMeshProUGUI txBtAllTimeRank;
    public GameObject allTimeTab;
    //public RankCard[] top3AllTime;
    public Image[] top3AllTimeImgs;
    public RankCard currentUserAllTime;
    public Transform allTimeParent;
    public GameObject noAllTimeGO;
    [Header("dailyChallenge")]
    public Toggle toggleDailyChallengeRank;
    public TextMeshProUGUI txBtDailyChallengeRank;
    public GameObject dailyChallengeTab;
    //public RankCard[] top3DailyChallenge;
    public Image[] top3DailyChallengeImgs;
    public RankCard currentUserDailyChallenge;
    public Transform dailyChallengeParent;
    public GameObject noDailyChallengeGO;
    [Space]
    public TextMeshProUGUI txTimeLeft;
    public Button btPrizePreview;
    public GameObject prizeReviewGO;
    public Image goldImage;
    public GameObject boosterGO;
    public GameObject[] boosterPrizes;
    public TextMeshProUGUI txGoldAm;
    public TextMeshProUGUI txBoosterAm;
    [Space]
    public Button btInfo;
    List<RankCard> userRankCards;
    bool hasChallengePrize;

    void BindRankTabHandlers()
    {
        toggleDailyRank.onValueChanged.RemoveListener(OnDailyTabSelected);
        toggleAllTimeRank.onValueChanged.RemoveListener(OnAllTimeTabSelected);
        toggleDailyChallengeRank.onValueChanged.RemoveListener(OnChallengeTabSelected);
        toggleDailyRank.onValueChanged.AddListener(OnDailyTabSelected);
        toggleAllTimeRank.onValueChanged.AddListener(OnAllTimeTabSelected);
        toggleDailyChallengeRank.onValueChanged.AddListener(OnChallengeTabSelected);
    }

    void OnDailyTabSelected(bool selected) { if (selected) ChangeTab(); }
    void OnAllTimeTabSelected(bool selected) { if (selected) ChangeTab(); }
    void OnChallengeTabSelected(bool selected) { if (selected) ChangeTab(); }

    public override void SetupTweenCallback()
    {
        EnsureTileSkyLeaderboard();
        BindRankTabHandlers();
        SetupTweenCallback(() =>
        {
            btClose.onClick.AddListener(() => Close());
            btEditPlayer.onClick.AddListener(() => EditPlayer());
            toggleDailyRank.interactable = true;
            toggleAllTimeRank.interactable = true;
            toggleDailyChallengeRank.interactable = true;

            btInfo.onClick.AddListener(() => Info());
        });
    }
    bool isInit;
    public override void Open()
    {
        EnsureTileSkyLeaderboard();
        prizeReviewGO.SetActive(false);
        btClose.onClick.RemoveAllListeners();
        btEditPlayer.onClick.RemoveAllListeners();
        btEditPlayer.gameObject.SetActive(HomeScript.instance);
        toggleDailyRank.interactable = false;
        toggleAllTimeRank.interactable = false;
        toggleDailyChallengeRank.interactable = false;
        BindRankTabHandlers();
        toggleDailyChallengeRank.gameObject.SetActive(GameConfig.IsDailyChallenge);
        LayoutTileSkyTabs();
        btInfo.onClick.RemoveAllListeners();
        if (!isInit)
        {
            loadingGO.SetActive(true);
            toggleDailyRank.isOn = true;
            ChangeTab();
            //TabDaily(true);
            GetLeaderboard();
        }
        else
        {
            foreach (var card in userRankCards)
                card.UpdateInfo(GameConfig.GetPlayerAvatar(), GameConfig.GetDisplayName(), GameConfig.GetPlayerFrame(), GameConfig.GetPlayerFlag());
        }
        ChangeTab();
        base.Open();
    }
    public void ChangeTab()
    {
        bool dailyChallenge = toggleDailyChallengeRank.isOn && toggleDailyChallengeRank.gameObject.activeSelf;
        bool allTime = !dailyChallenge && toggleAllTimeRank.isOn;
        bool daily = !dailyChallenge && !allTime;
        toggleDailyRank.SetIsOnWithoutNotify(daily);
        toggleAllTimeRank.SetIsOnWithoutNotify(allTime);
        toggleDailyChallengeRank.SetIsOnWithoutNotify(dailyChallenge);
        txBtDailyRank.color = daily ? Color.white : new Color(1, 1, 1, 0.5f);
        txBtAllTimeRank.color = allTime ? Color.white : new Color(1, 1, 1, 0.5f);
        txBtDailyChallengeRank.color = dailyChallenge ? Color.white : new Color(1, 1, 1, 0.5f);
        dailyTab.SetActive(daily);
        allTimeTab.SetActive(allTime);
        dailyChallengeTab.SetActive(dailyChallenge);
        txTimeLeft.gameObject.SetActive(dailyChallenge);
        btInfo.gameObject.SetActive(dailyChallenge);
        btPrizePreview.gameObject.SetActive(dailyChallenge && hasChallengePrize);
        prizeReviewGO.SetActive(false);
        RefreshTileSkyTabs();
    }
    //void TabDaily(bool force = false)
    //{
    //    if (!isInit && !force)
    //        return;
    //    btDailyRank.image.color = Color.white;
    //    btAllTimeRank.image.color = new Color(0, 0, 0, 0.5f);
    //    btDailyChallengeRank.image.color = new Color(0, 0, 0, 0.5f);
    //    txBtDailyRank.color = Color.white;
    //    txBtAllTimeRank.color = new Color(1, 1, 1, 0.5f);
    //    txBtDailyChallengeRank.color = new Color(1, 1, 1, 0.5f);
    //    allTimeTab.SetActive(false);
    //    dailyChallengeTab.SetActive(false);
    //    dailyTab.SetActive(true);
    //    btEditPlayer.gameObject.SetActive(true);
    //}
    //void TabAllTime()
    //{
    //    if (!isInit)
    //        return;
    //    btDailyRank.image.color = new Color(0, 0, 0, 0.5f);
    //    btAllTimeRank.image.color = Color.white;
    //    btDailyChallengeRank.image.color = new Color(0, 0, 0, 0.5f);
    //    txBtDailyRank.color = new Color(1, 1, 1, 0.5f);
    //    txBtAllTimeRank.color = Color.white;
    //    txBtDailyChallengeRank.color = new Color(1, 1, 1, 0.5f);
    //    dailyTab.SetActive(false);
    //    dailyChallengeTab.SetActive(false);
    //    allTimeTab.SetActive(true);
    //    btEditPlayer.gameObject.SetActive(true);
    //}
    //void TabDailyChallenge()
    //{
    //    if (!isInit)
    //        return;
    //    btDailyRank.image.color = new Color(0, 0, 0, 0.5f);
    //    btAllTimeRank.image.color = new Color(0, 0, 0, 0.5f);
    //    btDailyChallengeRank.image.color = Color.white;
    //    txBtDailyRank.color = new Color(1, 1, 1, 0.5f);
    //    txBtAllTimeRank.color = new Color(1, 1, 1, 0.5f);
    //    txBtDailyChallengeRank.color = Color.white;
    //    dailyTab.SetActive(false);
    //    allTimeTab.SetActive(false);
    //    dailyChallengeTab.SetActive(true);
    //    btEditPlayer.gameObject.SetActive(false);
    //}
    async void GetLeaderboard()
    {
        userRankCards = new();
        if (!UnityAuthentication.IsLeaderboard())
        {
            currentUserDaily.CardSetup(-1, GameConfig.GetPlayerAvatar(), GameConfig.GetDisplayName(), GameConfig.GetPlayerFrame(), GameConfig.GetPlayerFlag(),
                UnityAuthentication.GetLocalDailyScore());
            currentUserAllTime.CardSetup(-1, GameConfig.GetPlayerAvatar(), GameConfig.GetDisplayName(), GameConfig.GetPlayerFrame(), GameConfig.GetPlayerFlag(),
                Mathf.Max(0, GameConfig.GetCurrentLevel() - 1));
            userRankCards.Add(currentUserDaily);
            userRankCards.Add(currentUserAllTime);
            loadFailedGO.SetActive(true);
            var message = loadFailedGO.GetComponentInChildren<TextMeshProUGUI>(true);
            if (message) message.text = "Leaderboard is not connected yet.";
            loadingGO.SetActive(false);
            isInit = true;
            return;
        }
        LeaderboardScoresPage dailyScore;
        LeaderboardScoresPage allTimeScore;
        //bool dailyChallenge = Config.IsDailyChallenge;

        LeaderboardScoresPage dailyChallengeScore;
        try
        {
            dailyScore = await UnityAuthentication.instance.GetScoresDaily();
            allTimeScore = await UnityAuthentication.instance.GetScores();
            if (GameConfig.IsDailyChallenge)
                dailyChallengeScore = await UnityAuthentication.instance.GetScoresDailyChallenge();
            else
                dailyChallengeScore = null;
        }
        catch
        {
            loadFailedGO.SetActive(true);
            loadingGO.SetActive(false);
            isInit = true;
            return;
        }
        LeaderboardEntry userDailyScore;
        LeaderboardEntry userAllTimeScore;
        LeaderboardEntry userDailyChallengeScore;
        try
        {
            userDailyScore = await UnityAuthentication.instance.GetPlayerScoreDaily();
        }
        catch
        {
            userDailyScore = null;
        }
        try
        {
            userAllTimeScore = await UnityAuthentication.instance.GetPlayerScore();
        }
        catch
        {
            userAllTimeScore = null;
        }
        if (GameConfig.IsDailyChallenge)
        {
            try
            {
                userDailyChallengeScore = await UnityAuthentication.instance.GetPlayerScoreDailyChallenge();
            }
            catch
            {
                userDailyChallengeScore = null;
            }
        }
        else
        {
            userDailyChallengeScore = null;
        }
        userRankCards.Add(currentUserDaily);
        userRankCards.Add(currentUserAllTime);
        userRankCards.Add(currentUserDailyChallenge);
        noDailyGO.SetActive(dailyScore.Results.Count == 0);
        noAllTimeGO.SetActive(allTimeScore.Results.Count == 0);
        noDailyChallengeGO.SetActive(dailyChallengeScore != null && dailyChallengeScore.Results.Count == 0);

        foreach (var entry in dailyScore.Results.Select((value, index) => new { value, index }))
        {
            ScoreMetadata meta = JsonUtility.FromJson<ScoreMetadata>(entry.value.Metadata);
            //if (entry.index < 3)
            //{
            //    if (userDailyScore != null && userDailyScore.PlayerId == entry.value.PlayerId)
            //    {
            //        top3Daily[entry.index].CardSetup(entry.value.Rank, GameConfig.GetPlayerAvatar(), GameConfig.GetDisplayName(), GameConfig.GetPlayerFrame(), GameConfig.GetPlayerFlag(), (int)entry.value.Score);
            //        userRankCards.Add(top3Daily[entry.index]);
            //    }
            //    else
            //        top3Daily[entry.index].CardSetup(entry.value.Rank, meta.playerAvatar, meta.displayName, meta.playerFrame, meta.playerFlag, (int)entry.value.Score);
            //}
            //else
            //{
            //if (entry.index < 3)
            //{
            //    if (userDailyScore != null && userDailyScore.PlayerId == entry.value.PlayerId)
            //        top3DailyImgs[entry.index].sprite = UnityAuthentication.instance.GetAvatarSprite(GameConfig.GetPlayerAvatar());
            //    else
            //        top3DailyImgs[entry.index].sprite = UnityAuthentication.instance.GetAvatarSprite(meta.playerAvatar);
            //}
            if (userDailyScore != null && userDailyScore.PlayerId == entry.value.PlayerId)
            {
                //var card = Instantiate(rankCardUserTemplate, dailyParent);
                var card = PoolManager.instance.GetObject<RankCard>(rankCardUserTemplate, dailyParent);
                card.CardSetup(entry.value.Rank, GameConfig.GetPlayerAvatar(), GameConfig.GetDisplayName(), GameConfig.GetPlayerFrame(), GameConfig.GetPlayerFlag(), (int)entry.value.Score);
                userRankCards.Add(card);
                if (entry.index < 3)
                {
                    top3DailyImgs[entry.index].sprite = UnityAuthentication.instance.GetAvatarSprite(GameConfig.GetPlayerAvatar());
                    top3DailyImgs[entry.index].color = Color.white;
                }
            }
            else
            {
                var card = PoolManager.instance.GetObject<RankCard>(rankCardTemplate, dailyParent);
                card.CardSetup(entry.value.Rank, meta.playerAvatar, meta.displayName, meta.playerFrame, meta.playerFlag, (int)entry.value.Score);
                if (entry.index < 3)
                {
                    top3DailyImgs[entry.index].sprite = UnityAuthentication.instance.GetAvatarSprite(meta.playerAvatar);
                    top3DailyImgs[entry.index].color = Color.white;
                }
            }
            //}
        }
        if (userDailyScore != null)
        {
            currentUserDaily.CardSetup(userDailyScore.Rank, GameConfig.GetPlayerAvatar(), GameConfig.GetDisplayName(), GameConfig.GetPlayerFrame(), GameConfig.GetPlayerFlag(), (int)userDailyScore.Score);
        }
        else
            currentUserDaily.CardSetup(-1, GameConfig.GetPlayerAvatar(), GameConfig.GetDisplayName(), GameConfig.GetPlayerFrame(), GameConfig.GetPlayerFlag(), 0);

        foreach (var entry in allTimeScore.Results.Select((value, index) => new { value, index }))
        {
            ScoreMetadata meta = JsonUtility.FromJson<ScoreMetadata>(entry.value.Metadata);
            //if (entry.index < 3)
            //{
            //    if (userAllTimeScore != null && userAllTimeScore.PlayerId == entry.value.PlayerId)
            //    {
            //        top3AllTime[entry.index].CardSetup(entry.value.Rank, GameConfig.GetPlayerAvatar(), GameConfig.GetDisplayName(), GameConfig.GetPlayerFrame(), GameConfig.GetPlayerFlag(), (int)entry.value.Score);
            //        userRankCards.Add(top3AllTime[entry.index]);
            //    }
            //    else
            //        top3AllTime[entry.index].CardSetup(entry.value.Rank, meta.playerAvatar, meta.displayName, meta.playerFrame, meta.playerFlag, (int)entry.value.Score);
            //}
            //else
            //{
            if (userAllTimeScore != null && userAllTimeScore.PlayerId == entry.value.PlayerId)
            {
                //var card = Instantiate(rankCardUserTemplate, allTimeParent);
                var card = PoolManager.instance.GetObject<RankCard>(rankCardUserTemplate, allTimeParent);
                card.CardSetup(entry.value.Rank, GameConfig.GetPlayerAvatar(), GameConfig.GetDisplayName(), GameConfig.GetPlayerFrame(), GameConfig.GetPlayerFlag(), (int)entry.value.Score);
                userRankCards.Add(card);
                if (entry.index < 3)
                {
                    top3AllTimeImgs[entry.index].sprite = UnityAuthentication.instance.GetAvatarSprite(GameConfig.GetPlayerAvatar());
                    top3AllTimeImgs[entry.index].color = Color.white;
                }
            }
            else
            {
                var card = PoolManager.instance.GetObject<RankCard>(rankCardTemplate, allTimeParent);
                card.CardSetup(entry.value.Rank, meta.playerAvatar, meta.displayName, meta.playerFrame, meta.playerFlag, (int)entry.value.Score);
                if (entry.index < 3)
                {
                    top3AllTimeImgs[entry.index].sprite = UnityAuthentication.instance.GetAvatarSprite(meta.playerAvatar);
                    top3AllTimeImgs[entry.index].color = Color.white;
                }
            }
            //}
        }
        if (userAllTimeScore != null)
            currentUserAllTime.CardSetup(userAllTimeScore.Rank, GameConfig.GetPlayerAvatar(), GameConfig.GetDisplayName(), GameConfig.GetPlayerFrame(), GameConfig.GetPlayerFlag(), (int)userAllTimeScore.Score);
        else
            currentUserAllTime.CardSetup(-1, GameConfig.GetPlayerAvatar(), GameConfig.GetDisplayName(), GameConfig.GetPlayerFrame(), GameConfig.GetPlayerFlag(), 0);

        if (GameConfig.IsDailyChallenge)
        {
            foreach (var entry in dailyChallengeScore.Results.Select((value, index) => new { value, index }))
            {
                ScoreMetadata meta = JsonUtility.FromJson<ScoreMetadata>(entry.value.Metadata);
                //if (entry.index < 3)
                //{
                //    if (userDailyChallengeScore != null && userDailyChallengeScore.PlayerId == entry.value.PlayerId)
                //    {
                //        top3DailyChallenge[entry.index].CardSetup(entry.value.Rank, GameConfig.GetPlayerAvatar(), GameConfig.GetDisplayName(), GameConfig.GetPlayerFrame(), GameConfig.GetPlayerFlag(), (int)entry.value.Score, true);
                //        userRankCards.Add(top3DailyChallenge[entry.index]);
                //    }
                //    else
                //        top3DailyChallenge[entry.index].CardSetup(entry.value.Rank, meta.playerAvatar, meta.displayName, meta.playerFrame, meta.playerFlag, (int)entry.value.Score, true);
                //}
                //else
                //{
                //if (entry.index < 3)
                //{
                //    if (userDailyScore != null && userDailyScore.PlayerId == entry.value.PlayerId)
                //        top3DailyChallengeImgs[entry.index].sprite = UnityAuthentication.instance.GetAvatarSprite(GameConfig.GetPlayerAvatar());
                //    else
                //        top3DailyChallengeImgs[entry.index].sprite = UnityAuthentication.instance.GetAvatarSprite(meta.playerAvatar);
                //}
                if (userDailyChallengeScore != null && userDailyChallengeScore.PlayerId == entry.value.PlayerId)
                {
                    //var card = Instantiate(rankCardUserTemplate, dailyChallengeParent);
                    var card = PoolManager.instance.GetObject<RankCard>(rankCardUserTemplate, dailyChallengeParent);
                    card.CardSetup(entry.value.Rank, GameConfig.GetPlayerAvatar(), GameConfig.GetDisplayName(), GameConfig.GetPlayerFrame(), GameConfig.GetPlayerFlag(), (int)entry.value.Score, true);
                    userRankCards.Add(card);
                    if (entry.index < 3)
                    {
                        top3DailyChallengeImgs[entry.index].sprite = UnityAuthentication.instance.GetAvatarSprite(GameConfig.GetPlayerAvatar());
                        top3DailyChallengeImgs[entry.index].color = Color.white;
                    }
                }
                else
                {
                    var card = PoolManager.instance.GetObject<RankCard>(rankCardTemplate, dailyChallengeParent);
                    card.CardSetup(entry.value.Rank, meta.playerAvatar, meta.displayName, meta.playerFrame, meta.playerFlag, (int)entry.value.Score, true);
                    if (entry.index < 3)
                    {
                        top3DailyChallengeImgs[entry.index].sprite = UnityAuthentication.instance.GetAvatarSprite(meta.playerAvatar);
                        top3DailyChallengeImgs[entry.index].color = Color.white;
                    }
                }
                //}
            }
            if (userDailyChallengeScore != null)
            {
                PrizeReviewSetup(userDailyChallengeScore.Rank);
                hasChallengePrize = true;
                currentUserDailyChallenge.CardSetup(userDailyChallengeScore.Rank, GameConfig.GetPlayerAvatar(), GameConfig.GetDisplayName(), GameConfig.GetPlayerFrame(), GameConfig.GetPlayerFlag(), (int)userDailyChallengeScore.Score, true);
            }
            else
            {
                hasChallengePrize = false;
                currentUserDailyChallenge.CardSetup(-1, GameConfig.GetPlayerAvatar(), GameConfig.GetDisplayName(), GameConfig.GetPlayerFrame(), GameConfig.GetPlayerFlag(), 5999, true);
            }
        }
        loadingGO.SetActive(false);
        isInit = true;
        ChangeTab();
    }
    public override void Close()
    {
        if (!isInit)
            return;
        base.Close();
    }
    private void Update()
    {
        UpdateTime();
    }
    private void LateUpdate()
    {
        FitTileSkyBackButton();
    }
    void EditPlayer()
    {
        if (HomeScript.instance)
        {
            HomeScript.instance.OnOpenEditPlayer(() =>
            {
                foreach (var card in userRankCards)
                    card.UpdateInfo(GameConfig.GetPlayerAvatar(), GameConfig.GetDisplayName(), GameConfig.GetPlayerFrame(), GameConfig.GetPlayerFlag());
                HomeScript.instance.UpdateInfo();
            });
        }
    }
    void Info()
    {
        if (HomeScript.instance)
            HomeScript.instance.OnChallengeInfo();
        if (GameScript.instance)
            GameScript.instance.OnChallengeInfo();
    }
    void UpdateTime()
    {
        if (!txTimeLeft.gameObject.activeInHierarchy || !TimeManager.instance) return;
        TimeSpan ts = TimeManager.instance.TimeToNextDay();
        txTimeLeft.text = $"{Mathf.Floor((float)ts.TotalHours):00}:{ts:mm}:{ts:ss}";
    }
    void PrizeReviewSetup(int rank)
    {
        var (gold, booster) = UnityAuthentication.GetDailyChallengePrizeFromRank(rank);
        boosterGO.SetActive(booster != null && booster.prizeTypes.Length > 0);
        goldImage.sprite = UnityAuthentication.instance.GetGoldSprite(gold);
        txGoldAm.text = $"{gold}";
        if (booster != null)
        {
            txBoosterAm.text = $"{booster.prizeAmount}";
            for (int i = 0; i < 4; i++)
                boosterPrizes[i].SetActive(i != 3 && booster.prizeTypes.Select(GameConfig.NormalizePowerUpPrizeType).Contains(i));
        }
    }
}
