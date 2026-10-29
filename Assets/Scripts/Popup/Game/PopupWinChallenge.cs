using DG.Tweening;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.UI;

public class PopupWinChallenge : Popup
{
    public Button btInfo;
    //public Button btAds;
    public Button btHome;
    public Button btRestart;
    public TextMeshProUGUI txGoldsGain;
    public GameObject bestGO;
    [Header("rank")]
    public RankCard rankCardTemplate;
    public RankCard rankCardUserTemplate;
    public GameObject loadingGO;
    public GameObject rankGO;
    public Transform rankCardParent;
    public ScrollRect rankScrollRect;
    [Space]
    public GameObject prizeReviewGO;
    public Image goldImage;
    public GameObject boosterGO;
    public GameObject[] boosterPrizes;
    public TextMeshProUGUI txGoldAm;
    public TextMeshProUGUI txBoosterAm;

    //public Button btPrizePreview;
    public override void SetupTweenCallback()
    {
        SetupTweenCallback(() =>
        {
            Debug.Log("settweencallback");
            Win();
        });
    }
    public bool openWin = false;
    public TextMeshProUGUI txChallengeTimer;
    public GameObject winGO;
    public void OpenWinChallenge(float score)
    {
        txGoldsGain.gameObject.SetActive(UnityAuthentication.instance.dailyChallengeScore == null);
        bestGO.SetActive(UnityAuthentication.instance.BestScore(score));
        UpdateScore(score);
        TimeSpan ts = TimeSpan.FromSeconds(score);
        txChallengeTimer.text = $"{Mathf.Floor((float)ts.TotalMinutes):00}:{ts:ss}";
        openWin = true;
        Open();
    }
    async void UpdateScore(float score)
    {
        try
        {
            loadingGO.SetActive(true);
            bool updateScore = await UnityAuthentication.instance.UpdateDailyChallengeScore(score);
            var newDailyChallengeScores = await UnityAuthentication.instance.GetPlayerRangeDailyChallenge();
            StartCoroutine(LB(newDailyChallengeScores));
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            rankGO.SetActive(false);
        }
    }
    IEnumerator LB(LeaderboardScores scorePage, LeaderboardEntry oldScore = null)
    {
        rankCardParent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        var newScore = scorePage.Results.Find(x => UnityAuthentication.GetPlayerId() == x.PlayerId);
        int newScoreIndex = scorePage.Results.IndexOf(newScore);
        foreach (var entry in scorePage.Results.Select((value, index) => new { value, index }))
        {
            ScoreMetadata meta = JsonUtility.FromJson<ScoreMetadata>(entry.value.Metadata);
            if (UnityAuthentication.GetPlayerId() == entry.value.PlayerId)
            {
                //var card = PoolManager.instance.GetObject<RankCard>(rankCardUserTemplate, rankCardParent);
                var card = rankCardUserTemplate;
                card.transform.SetParent(rankCardParent);
                card.CardSetup(entry.value.Rank, GameConfig.GetPlayerAvatar(), GameConfig.GetDisplayName(), GameConfig.GetPlayerFrame(), GameConfig.GetPlayerFlag(), (int)entry.value.Score, true);
                PrizeReviewSetup(entry.value.Rank);
            }
            else
            {
                var card = PoolManager.instance.GetObject<RankCard>(rankCardTemplate, rankCardParent);
                card.CardSetup(entry.value.Rank, meta.playerAvatar, meta.displayName, meta.playerFrame, meta.playerFlag, (int)entry.value.Score, true);
            }
        }
        yield return null;
        //rankCardParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, (newScoreIndex - 1) * 190);
        rankCardParent.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, (newScoreIndex - 1) * 190), 0.2f);
        yield return null;
        loadingGO.SetActive(false);
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
    public override void Open()
    {
        AudioManager.instance.PlaySFXOneShot(AudioManager.SFX.WIN);
        btInfo.onClick.RemoveAllListeners();
        //btAds.onClick.RemoveAllListeners();
        btHome.onClick.RemoveAllListeners();
        btRestart.onClick.RemoveAllListeners();
        //btAds.gameObject.SetActive(UnityAuthentication.GetDailyChallengeAds());
        winGO.SetActive(false);
        base.Open();
    }
    void Win()
    {
        MaskableGraphic[] maskgraphics = winGO.GetComponentsInChildren<MaskableGraphic>(false);
        foreach (var graphic in maskgraphics)
        {
            float a = graphic.color.a;
            graphic.DOFade(0, 0);
            graphic.DOFade(a, 0.5f);
        }
        winGO.transform.DOScale(1, 0).OnComplete(() => winGO.SetActive(true));
        winGO.transform.DOScale(1, 0.5f).SetDelay(0.5f)//.OnStart(() => win1GO.SetActive(true))
            .OnComplete(() =>
            {
                btInfo.onClick.AddListener(() => Info());
                btHome.onClick.AddListener(() => Home());
                btRestart.onClick.AddListener(() => Restart());
                //btAds.onClick.AddListener(() => GameConfig.ShowRewardedAd(11));
                if (txGoldsGain.gameObject.activeSelf)
                {
                    GameScript.instance.TopBarUpdateGolds(txGoldsGain.transform, 120, 10, () => txGoldsGain.gameObject.SetActive(false));
                }
            });
    }
    void Home()
    {
        rankScrollRect.gameObject.SetActive(false);
        GameScript.instance.Home();
    }
    void Restart()
    {
        rankScrollRect.gameObject.SetActive(false);
        GameScript.instance.Restart();
    }
    void Info()
    {
        GameScript.instance.OnChallengeInfo();
    }
}
