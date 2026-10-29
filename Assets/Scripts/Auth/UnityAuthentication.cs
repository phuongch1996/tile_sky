using Beebyte.Obfuscator;
using EasyButtons;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

public class UnityAuthentication : MonoBehaviour
{
    const string allTimeLeaderboardId = "tileevo_alltime";
    const string dailyLeaderboardId = "tileevo_daily";
    const string dailyChallengeLeaderboardId = "tileevo_daily_challenge";

    public static DateTime nextReset;
    public static UnityAuthentication instance;

    public Sprite[] avatarSprites;
    public Sprite[] frameSprites;
    public Sprite[] flagSprites;
    public static readonly DailyChallengePrize dailyChallengePrize = new(new int[] { 100, 100, 100, 150, 100, 80, 60, 30, 50 },
        new BPPrizes[] {
            new(1, new int[] { 0, 1, 2 }),
            new(1, new int[] { 0, 1, 2 }),
            new(1, new int[] { 0, 2 }),
            new(1, new int[] { 0 }),
        });
    public static (int gold, BPPrizes booster) GetDailyChallengePrizeFromRank(int rank)
    {
        int gold = 0; BPPrizes booster = null;
        switch (rank)
        {
            case 0:
                gold = dailyChallengePrize.top1G;
                booster = dailyChallengePrize.top1B;
                break;
            case 1:
                gold = dailyChallengePrize.top2G;
                booster = dailyChallengePrize.top2B;
                break;
            case 2:
                gold = dailyChallengePrize.top3G;
                booster = dailyChallengePrize.top3B;
                break;
            case < 5:
                gold = dailyChallengePrize.top4_5G;
                booster = dailyChallengePrize.top4_5B;
                break;
            case < 10:
                gold = dailyChallengePrize.top6_10G;
                booster = dailyChallengePrize.top6_10B;
                break;
            case < 20:
                gold = dailyChallengePrize.top11_20G;
                booster = dailyChallengePrize.top11_20B;
                break;
            case < 50:
                gold = dailyChallengePrize.top21_50G;
                booster = dailyChallengePrize.top21_50B;
                break;
            case < 100:
                gold = dailyChallengePrize.top51_100G;
                booster = dailyChallengePrize.top51_100B;
                break;
            default:
                gold = dailyChallengePrize.top100_G;
                booster = dailyChallengePrize.top100_B;
                break;
        }
        //Debug.Log($"{gold},{(booster != null ? string.Join("|", booster.prizeTypes) : null)}");
        return (gold, booster);
    }
    const string loginStatusKey = "login_status"; //0: notlogin 1:anonymous 2:linkgoogle
    public static int GetLogInStatus()
    {
        return PlayerPrefs.GetInt(loginStatusKey, 0);
    }
    public static void SetLogInStatus(int status)
    {
        PlayerPrefs.SetInt(loginStatusKey, status);
    }
    private void Awake()
    {
        if (instance != null && instance.gameObject != gameObject)
        {
            Destroy(gameObject);
            return;
        }
        else
            instance = this;
        DontDestroyOnLoad(gameObject);
    }
    async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += async () =>
        {
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
            //if (!GPGScript.instance.IsLogin)
            //{
            string name = await AuthenticationService.Instance.GetPlayerNameAsync();
            string[] words = Regex.Split(name, @"(?<!^)(?=[A-Z])");
            string word = words[0].Substring(0, Mathf.Min(15, words[0].Length));
            GameConfig.SetDefaultName(word);
            SetLogInStatus(1);
            //}
            HasLocalScore();
            await GetDailyChallengeDate();
            isSignedIn = true;
        };
        AuthenticationService.Instance.SignInFailed += s =>
        {
            isSignedIn = false;
            // Take some action here...
            Debug.Log("Signed in fail");
        };

        //if (GetLogInStatus() != 2)
        await SignInAnonymously();
        //else
        //    GPGScript.instance.Authenticate();
    }
    public static bool? isSignedIn;

    public static TimeSpan GetTimeLeft()
    {
        return nextReset - TimeManager.instance.currentTime;
    }
    public static bool IsLeaderboard()
    {
        return isSignedIn == true;
    }
    public Sprite GetAvatarSprite(int avatar)
    {
        return avatarSprites[avatar % avatarSprites.Length];
    }
    public Sprite GetFrameSprite(int frame)
    {
        return frameSprites[frame % frameSprites.Length];
    }
    public Sprite GetFlagSprite(int flag)
    {
        return flagSprites[flag % flagSprites.Length];
    }
    public Sprite[] goldSprites;
    public Sprite GetGoldSprite(int gold)
    {
        return goldSprites[gold switch
        {
            <= 200 => 0,
            <= 700 => 1,
            _ => 2
        }];
    }
    async Task SignInAnonymously()
    {
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
    public static string GetPlayerId()
    {
        //Debug.Log(AuthenticationService.Instance.PlayerId);
        return AuthenticationService.Instance.PlayerId;
    }
    public void WinAddScore()
    {
        SaveLocalScore();
        HasLocalScore();
    }
    public async Task UpdateMetadata()
    {
        //GPG_CloudSaveSystem.Instance.SaveToCloud();
        await AddScoreDaily(0);
        await AddScore(GameConfig.GetCurrentLevel() - 1);
        if (dailyChallengeScore != null)
            _ = await UpdateDailyChallengeScore((int)dailyChallengeScore.Score);
    }
    const string localLBKey = "local_lb";
    [Button]
    void SaveLocalScore()
    {
        int allTimeScore = GameConfig.GetCurrentLevel() - 1;
        if (!PlayerPrefs.HasKey(localLBKey))
        {
            string local_lb = JsonConvert.SerializeObject(new LocalScore { date = GameConfig.GetCurrentDate(), scoreDaily = 1, scoreAllTime = allTimeScore });
            PlayerPrefs.SetString(localLBKey, local_lb);
        }
        else
        {
            //string local_lb = PlayerPrefs.GetString("local_lb");
            var localScore = JsonConvert.DeserializeObject<LocalScore>(PlayerPrefs.GetString(localLBKey));
            if (GameConfig.GetCurrentDate() == localScore.date)
            {
                localScore.scoreDaily += 1;
                localScore.scoreAllTime = allTimeScore;
                string local_lb = JsonConvert.SerializeObject(localScore);
                PlayerPrefs.SetString(localLBKey, local_lb);
            }
            else
            {
                string local_lb = JsonConvert.SerializeObject(new LocalScore { date = GameConfig.GetCurrentDate(), scoreDaily = 1, scoreAllTime = allTimeScore });
                PlayerPrefs.SetString(localLBKey, local_lb);
            }
        }
    }
    //public DateTime? endtime;
    async void HasLocalScore()
    {
        if (PlayerPrefs.HasKey(localLBKey))
        {
            DateTime endTime;
            try
            {
                endTime = await GetDailyLeaderboardVersion();
            }
            catch
            {
                Debug.Log("add score fail");
                return;
            }
            var localScore = JsonConvert.DeserializeObject<LocalScore>(PlayerPrefs.GetString(localLBKey));
            PlayerPrefs.DeleteKey(localLBKey);
            await AddScore(localScore.scoreAllTime);
            if (localScore.date == GameConfig.GetDateIndex(endTime))
            {
                await AddScoreDaily(localScore.scoreDaily);
            }
        }
    }
    public int? dailyChallengeDateIndex;
    public string currentDailyChallengeVersionId;
    public int dailyChallengeTestDayOfYear;
    public LeaderboardEntry dailyChallengeScore;
    const string dailyChallengeAdKey = "daily_challenge_ad";
    public static bool GetDailyChallengeAds()
    {
        return PlayerPrefs.GetInt(dailyChallengeAdKey, 0) == 0;
    }
    public static void WatchDailyChallengeAds()
    {
        PlayerPrefs.SetInt(dailyChallengeAdKey, 1);
    }
    public static void ResetDailyChallengeAds()
    {
        PlayerPrefs.DeleteKey(dailyChallengeAdKey);
    }

    async Task<DateTime> GetDailyChallengeDate()
    {
        //DateTime dailyChallengeDate = await GetDailyChallengeLeaderboardVersion();
        var dailyChallengeVersions = await GetDailyChallengeLeaderboardVersion();
        DateTime dailyChallengeDate;
        if (dailyChallengeVersions.Results.Count > 0)
            dailyChallengeDate = dailyChallengeVersions.Results[0].End;
        else
            dailyChallengeDate = DateTime.UtcNow;
        dailyChallengeDateIndex = GameConfig.GetDateIndex(dailyChallengeDate);
        currentDailyChallengeVersionId = dailyChallengeVersions.VersionId;

        Debug.Log("dailyChallengeDateIndex " + dailyChallengeDateIndex);
        Debug.Log("currentDailyChallengeVersionId " + currentDailyChallengeVersionId);
        try
        {
            dailyChallengeScore = await GetPlayerScoreDailyChallenge();
        }
        catch
        {
            dailyChallengeScore = null;
            ResetDailyChallengeAds();
        }
        try
        {
            pastEntry = await GetPastDailyChallengeScore();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            pastEntry = null;
        }
        //Debug.Log(pastEntry);
        if (pastEntry != null)
        {
            Debug.Log("pastEntry" + pastEntry);
        }
        return dailyChallengeDate;
    }
    public bool IsDailyChallengeHasScore()
    {
        return dailyChallengeScore != null;
    }
    public bool BestScore(float score)
    {
        if (dailyChallengeScore != null)
            return score <= dailyChallengeScore.Score;
        return true;
    }
    public async Task<bool> UpdateDailyChallengeScore(float score)
    {
        //if (Config.GetCurrentDate() != dailyChallengeDateIndex)
        //    return;
        if (!GameConfig.IsDailyChallenge)
            return false;
        if (score <= 0)
            return false;

        //GPG_CloudSaveSystem.Instance.SaveToCloud();
        if (dailyChallengeScore == null || score <= dailyChallengeScore.Score)
            await AddScoreDailyChallenge(score);
        dailyChallengeScore = await GetPlayerScoreDailyChallenge();
        SetDailyChallengeLeaderboardId(currentDailyChallengeVersionId);
        return true;
    }
    public async Task AddScoreDaily(int score)
    {
        var metadata = new ScoreMetadata { playerAvatar = GameConfig.GetPlayerAvatar(), displayName = GameConfig.GetDisplayName(), playerFrame = GameConfig.GetPlayerFrame(), playerFlag = GameConfig.GetPlayerFlag() };
        var playerEntry = await LeaderboardsService.Instance.AddPlayerScoreAsync(dailyLeaderboardId, score, new AddPlayerScoreOptions() { Metadata = metadata });
        Debug.Log(JsonConvert.SerializeObject(playerEntry));
    }
    public async Task AddScore(int score)
    {
        var metadata = new ScoreMetadata { playerAvatar = GameConfig.GetPlayerAvatar(), displayName = GameConfig.GetDisplayName(), playerFrame = GameConfig.GetPlayerFrame(), playerFlag = GameConfig.GetPlayerFlag() };
        var playerEntry = await LeaderboardsService.Instance.AddPlayerScoreAsync(allTimeLeaderboardId, score, new AddPlayerScoreOptions() { Metadata = metadata });
        Debug.Log(JsonConvert.SerializeObject(playerEntry));
    }
    public async Task<LeaderboardEntry> AddScoreDailyChallenge(float score)
    {
        var metadata = new ScoreMetadata { playerAvatar = GameConfig.GetPlayerAvatar(), displayName = GameConfig.GetDisplayName(), playerFrame = GameConfig.GetPlayerFrame(), playerFlag = GameConfig.GetPlayerFlag() };
        var playerEntry = await LeaderboardsService.Instance.AddPlayerScoreAsync(dailyChallengeLeaderboardId, score, new AddPlayerScoreOptions() { Metadata = metadata });
        Debug.Log(JsonConvert.SerializeObject(playerEntry));
        return playerEntry;
    }
    //public void ButtonGetScores()
    //{
    //    GetScoresDaily();
    //    GetScores();
    //}
    public async Task<LeaderboardScoresPage> GetScoresDaily()
    {
        var scoreResponse = await LeaderboardsService.Instance.GetScoresAsync(dailyLeaderboardId, new GetScoresOptions { Limit = 100, IncludeMetadata = true });
        Debug.Log(JsonConvert.SerializeObject(scoreResponse));
        return scoreResponse;
    }
    public async Task<LeaderboardScoresPage> GetScores()
    {
        var scoreResponse = await LeaderboardsService.Instance.GetScoresAsync(allTimeLeaderboardId, new GetScoresOptions { Limit = 100, IncludeMetadata = true });
        Debug.Log(JsonConvert.SerializeObject(scoreResponse));
        return scoreResponse;
    }
    public async Task<LeaderboardScoresPage> GetScoresDailyChallenge()
    {
        var scoreResponse = await LeaderboardsService.Instance.GetScoresAsync(dailyChallengeLeaderboardId, new GetScoresOptions { Limit = 100, IncludeMetadata = true });
        Debug.Log(JsonConvert.SerializeObject(scoreResponse));
        return scoreResponse;
    }
    public async Task<LeaderboardScores> GetPlayerRangeDailyChallenge()
    {
        var scoreResponse = await LeaderboardsService.Instance.GetPlayerRangeAsync(dailyChallengeLeaderboardId, new GetPlayerRangeOptions { RangeLimit = 25, IncludeMetadata = true });
        Debug.Log(JsonConvert.SerializeObject(scoreResponse));
        return scoreResponse;
    }
    //public void ButtonGetPlayerScore()
    //{
    //    GetPlayerScoreDaily();
    //    GetPlayerScore();
    //}
    public async Task<LeaderboardEntry> GetPlayerScoreDaily()
    {
        var scoreResponse = await LeaderboardsService.Instance.GetPlayerScoreAsync(dailyLeaderboardId);
        Debug.Log(JsonConvert.SerializeObject(scoreResponse));
        return scoreResponse;
    }
    public async Task<LeaderboardEntry> GetPlayerScore()
    {
        var scoreResponse = await LeaderboardsService.Instance.GetPlayerScoreAsync(allTimeLeaderboardId);
        Debug.Log(JsonConvert.SerializeObject(scoreResponse));
        return scoreResponse;
    }
    public async Task<LeaderboardEntry> GetPlayerScoreDailyChallenge()
    {
        var scoreResponse = await LeaderboardsService.Instance.GetPlayerScoreAsync(dailyChallengeLeaderboardId);
        Debug.Log(JsonConvert.SerializeObject(scoreResponse));
        return scoreResponse;
    }
    public async Task<DateTime> GetDailyLeaderboardVersion()
    {
        var versionsResponse = await LeaderboardsService.Instance.GetVersionsAsync(dailyLeaderboardId);
        Debug.Log(JsonConvert.SerializeObject(versionsResponse));
        Debug.Log(JsonConvert.SerializeObject(versionsResponse.Results));
        //versionsResponse.VersionId;
        //return versionsResponse;
        if (versionsResponse.Results.Count > 0)
        {
            return versionsResponse.Results[0].End;
        }
        return DateTime.UtcNow;
    }
    public LeaderboardEntry pastEntry;
    public async Task<LeaderboardEntry> GetPastDailyChallengeScore()
    {
        if (HasDailyChallengeLeaderboardId())
        {
            string leaderboardId = GetDailyChallengeLeaderboardId();
            Debug.Log(leaderboardId);
            var response1 = await LeaderboardsService.Instance.GetVersionScoresByPlayerIdsAsync(dailyChallengeLeaderboardId, leaderboardId, new List<string>() { AuthenticationService.Instance.PlayerId });
            Debug.Log(JsonConvert.SerializeObject(response1));
            return response1.Results[0];
        }
        return null;
    }
    public async Task<LeaderboardVersions> GetDailyChallengeLeaderboardVersion()
    {
        var versionsResponse = await LeaderboardsService.Instance.GetVersionsAsync(dailyChallengeLeaderboardId);
        Debug.Log(JsonConvert.SerializeObject(versionsResponse));
        Debug.Log(JsonConvert.SerializeObject(versionsResponse.Results));
        return versionsResponse;
        //if (versionsResponse.Results.Count > 0)
        //{
        //    return versionsResponse.Results[0].End;
        //}
        //return DateTime.UtcNow;
    }
    //public void ButtonGetPastLeaderboardVersions()
    //{
    //    GetDailyLeaderboardVersion();
    //}

    const string dailyChallengeLeaderboardIdKey = "daily_leaderboard_id";
    const string dailyChallengeLeaderboardDateKey = "daily_leaderboard_date";
    const string dailyChallengeLeaderboardPrizeClaimKey = "daily_leaderboard_prize_claim";

    public static bool HasDailyChallengeLeaderboardId()
    {
        return PlayerPrefs.HasKey(dailyChallengeLeaderboardIdKey);
    }
    public static string GetDailyChallengeLeaderboardId()
    {
        return PlayerPrefs.GetString(dailyChallengeLeaderboardIdKey);
    }
    public static int GetDailyChallengeLeaderboardDate()
    {
        return PlayerPrefs.GetInt(dailyChallengeLeaderboardDateKey);
    }
    public static void SetDailyChallengeLeaderboardId(string leaderboardVersion)
    {
        PlayerPrefs.SetString(dailyChallengeLeaderboardIdKey, leaderboardVersion);
        PlayerPrefs.SetInt(dailyChallengeLeaderboardDateKey, GameConfig.GetCurrentDate());
        //ResetLeaderboardPrizeClaim();
        SetLeaderboardPrizeClaim(false);
    }
    public static void ResetDailyChallengeLeaderboardId()
    {
        PlayerPrefs.DeleteKey(dailyChallengeLeaderboardIdKey);
    }
    public static bool GetPastLeaderboardPrizeClaim()
    {
        return PlayerPrefs.GetInt(dailyChallengeLeaderboardPrizeClaimKey, 0) == 0;
    }
    public void PastLeaderboardPrizeClaim()
    {
        pastEntry = null;
        ResetDailyChallengeLeaderboardId();
        SetLeaderboardPrizeClaim(true);
        //PlayerPrefs.SetInt(dailyChallengeLeaderboardPrizeClaimKey, 1);
    }
    public static void SetLeaderboardPrizeClaim(bool claimed)
    {
        PlayerPrefs.SetInt(dailyChallengeLeaderboardPrizeClaimKey, claimed ? 1 : 0);
    }
    public bool IsPastLeaderboardPrizeClaim()
    {
        if (!GetPastLeaderboardPrizeClaim())
            pastEntry = null;
        return pastEntry != null;
    }
}

[Skip]
[Serializable]
public class ScoreMetadata
{
    public int playerAvatar;
    public string displayName;
    public int playerFrame;
    public int playerFlag = -1;
}
[Skip]
[Serializable]
public class LocalScore
{
    public int date;
    public int scoreDaily;
    public int scoreAllTime;
}
[Serializable]
public class DailyChallengePrize
{
    public int top1G;
    public BPPrizes top1B;
    public int top2G;
    public BPPrizes top2B;
    public int top3G;
    public BPPrizes top3B;
    public int top4_5G;
    public BPPrizes top4_5B;
    public int top6_10G;
    public BPPrizes top6_10B;
    public int top11_20G;
    public BPPrizes top11_20B;
    public int top21_50G;
    public BPPrizes top21_50B;
    public int top51_100G;
    public BPPrizes top51_100B;
    public int top100_G;
    public BPPrizes top100_B;
    public DailyChallengePrize(int[] goldPrize, BPPrizes[] boosterPrize)
    {
        top1G = GetPrizeAt(goldPrize, 0);
        top1B = GetPrizeAt(boosterPrize, 0);
        top2G = GetPrizeAt(goldPrize, 1);
        top2B = GetPrizeAt(boosterPrize, 1);
        top3G = GetPrizeAt(goldPrize, 2);
        top3B = GetPrizeAt(boosterPrize, 2);
        top4_5G = GetPrizeAt(goldPrize, 3);
        top4_5B = GetPrizeAt(boosterPrize, 3);
        top6_10G = GetPrizeAt(goldPrize, 4);
        top6_10B = GetPrizeAt(boosterPrize, 4);
        top11_20G = GetPrizeAt(goldPrize, 5);
        top11_20B = GetPrizeAt(boosterPrize, 5);
        top21_50G = GetPrizeAt(goldPrize, 6);
        top21_50B = GetPrizeAt(boosterPrize, 6);
        top51_100G = GetPrizeAt(goldPrize, 7);
        top51_100B = GetPrizeAt(boosterPrize, 7);
        top100_G = GetPrizeAt(goldPrize, 8);
        top100_B = GetPrizeAt(boosterPrize, 8);
    }
    T GetPrizeAt<T>(T[] prize, int index)
    {
        return (prize != null && index < prize.Length) ? prize[index] : default;
    }
}
