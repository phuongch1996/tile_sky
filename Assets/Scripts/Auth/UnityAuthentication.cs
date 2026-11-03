using Beebyte.Obfuscator;
using EasyButtons;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

public class UnityAuthentication : MonoBehaviour
{

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
    // Retain local profile/art references without signing into the former game.
    void Start()
    {
        isSignedIn = false;
        SetLogInStatus(0);
        dailyChallengeDateIndex = null;
        currentDailyChallengeVersionId = string.Empty;
        dailyChallengeScore = null;
        pastEntry = null;
    }
    public static bool? isSignedIn = false;

    public static TimeSpan GetTimeLeft()
    {
        return nextReset - TimeManager.instance.currentTime;
    }
    public static bool IsLeaderboard() { return false; }
    public Sprite GetAvatarSprite(int avatar)
    {
        var heavenlyAvatars = HeavenlyInfoArt.Avatars;
        if (heavenlyAvatars.Length == 9)
            return heavenlyAvatars[((avatar % 9) + 9) % 9];
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

    public static string GetPlayerId()
    {
        const string key = "tilespace_player_id";
        if (!PlayerPrefs.HasKey(key)) PlayerPrefs.SetString(key, Guid.NewGuid().ToString("N"));
        return PlayerPrefs.GetString(key);
    }
    public void WinAddScore() { SaveLocalScore(); }
    public Task UpdateMetadata() { return Task.CompletedTask; }
    const string localLBKey = "tilespace_local_lb";

    public static int GetLocalDailyScore()
    {
        if (!PlayerPrefs.HasKey(localLBKey)) return 0;
        try
        {
            var score = JsonConvert.DeserializeObject<LocalScore>(PlayerPrefs.GetString(localLBKey));
            return score != null && score.date == GameConfig.GetCurrentDate() ? score.scoreDaily : 0;
        }
        catch (JsonException) { return 0; }
    }
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
    // Compatibility API: no requests, uploads, remote players or archived rewards.
    public Task<bool> UpdateDailyChallengeScore(float score) { return Task.FromResult(false); }
    public Task AddScoreDaily(int score) { return Task.CompletedTask; }
    public Task AddScore(int score) { return Task.CompletedTask; }
    public Task<LeaderboardEntry> AddScoreDailyChallenge(float score) { return Task.FromResult<LeaderboardEntry>(null); }
    //public void ButtonGetScores()
    //{
    //    GetScoresDaily();
    //    GetScores();
    //}
    public Task<LeaderboardScoresPage> GetScoresDaily() { return Task.FromResult(new LeaderboardScoresPage(results: new List<LeaderboardEntry>())); }
    public Task<LeaderboardScoresPage> GetScores() { return Task.FromResult(new LeaderboardScoresPage(results: new List<LeaderboardEntry>())); }
    public Task<LeaderboardScoresPage> GetScoresDailyChallenge() { return Task.FromResult(new LeaderboardScoresPage(results: new List<LeaderboardEntry>())); }
    public Task<LeaderboardScores> GetPlayerRangeDailyChallenge() { return Task.FromResult(new LeaderboardScores(new List<LeaderboardEntry>())); }
    //public void ButtonGetPlayerScore()
    //{
    //    GetPlayerScoreDaily();
    //    GetPlayerScore();
    //}
    public Task<LeaderboardEntry> GetPlayerScoreDaily() { return Task.FromResult<LeaderboardEntry>(null); }
    public Task<LeaderboardEntry> GetPlayerScore() { return Task.FromResult<LeaderboardEntry>(null); }
    public Task<LeaderboardEntry> GetPlayerScoreDailyChallenge() { return Task.FromResult<LeaderboardEntry>(null); }
    public Task<DateTime> GetDailyLeaderboardVersion() { return Task.FromResult(DateTime.UtcNow); }
    public LeaderboardEntry pastEntry;
    public Task<LeaderboardEntry> GetPastDailyChallengeScore() { return Task.FromResult<LeaderboardEntry>(null); }
    public Task<LeaderboardVersions> GetDailyChallengeLeaderboardVersion() { return Task.FromResult(new LeaderboardVersions(results: new List<LeaderboardVersion>())); }
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
    public bool IsPastLeaderboardPrizeClaim() { return false; }
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
