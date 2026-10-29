using Firebase.Analytics;
using Firebase.Extensions;
using Firebase.Messaging;
using Firebase.RemoteConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class Common
{
    public static int request_permission_notify = 0;
    public static string REQUEST_PERMISSION_NOTIFY = "REQUEST_PERMISSION_NOTIFY";
    //aoa
    public static bool ads_aoa_show = false;
    public static bool ads_aoa_resume_game_show = false;
    public static int ads_aoa_open_count = 1;
    public static int ads_aoa_type = 1;
    public static int ads_aoa_duration = 10;
    public static string ads_aoa_id_admob;
    public static string ads_aoa_id_max;
    public static int ads_aoa_start_level = 1;

    //inter
    public static bool ads_inter_shown_on_level_start = false;
    public static bool ads_inter_shown_on_level_end = false;
    public static int ads_inter_ad_break_time = 60;
    public static string ads_inter_ad_break_id_admob;
    public static string ads_inter_ad_break_id_max;
    public static int ads_inter_start_level = 1;
    public static int ads_inter_level_range = 1;
    public static int ads_inter_type = 1;
    public static int ads_inter_duration = 60;
    public static int ads_inter_first_show = 60;
    public static string ads_inter_id_admob;
    public static string ads_inter_id_max;

    //banner
    public static bool ads_banner_show = false;
    public static int ads_banner_type = 1;
    public static string ads_banner_id_admob;
    public static string ads_banner_id_max;
    //public static int ads_banner_start_level;
    public static bool ads_banner_fallback = true;

    //reward
    public static int ads_reward_type = 1;
    public static string ads_reward_id_admob;
    public static string ads_reward_id_max;

    //mrec
    public static bool ads_mrec_show = false;
    public static string ads_mrec_id_max;

    //colllapse banner
    public static bool ads_collapse_show = false;
    public static int ads_collapse_duration = 60;
    public static string ads_collapse_id_admob;
    public static int ads_collapse_start_level = 1;

    //
    public static bool rewarded_reset_capping_time;

    public static int config_loading_time = 2;

    public static int config_tile_shadow = 0;

    public static int config_unlock_battlepass = 31;

    public static int config_unlock_piggybank = 22;

    public static int config_unlock_rank = 26;

    public static int config_unlock_dailychallenge = 51;

    public static int config_event_dates = 7;

    public static int config_tile_set = 0;
}
public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager instance;

    private bool _isFetchDone;
    public bool isFetchDone => _isFetchDone;
    public DateTime? fetchTime;

    [SerializeField]
    private List<string> _keyConfigs = new List<string>();

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
        //return;
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.

                Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;

                InitializeFirebase();
                Messaging();
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }
    async void Messaging()
    {
        FirebaseMessaging.MessageReceived += OnMessageReceived;
        FirebaseMessaging.TokenReceived += OnTokenReceived;
        Common.request_permission_notify = PlayerPrefs.GetInt(Common.REQUEST_PERMISSION_NOTIFY, 0);
        if (Common.request_permission_notify == 0)
        {
#if UNITY_ANDROID
            askRequst();
            Common.request_permission_notify = 1;
            PlayerPrefs.SetInt(Common.REQUEST_PERMISSION_NOTIFY, 1);
#endif
        }
        await FirebaseMessaging.RequestPermissionAsync();
    }
    void askRequst()//call this function to ask request
    {
#if UNITY_ANDROID
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            UnityEngine.Android.Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
        }
#endif
    }
    public void OnTokenReceived(object sender, TokenReceivedEventArgs token)
    {
        UnityEngine.Debug.Log("Received Registration Token: " + token.Token);
    }

    public void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        UnityEngine.Debug.Log("Received a new message from: " + e.Message.From);
    }
    public void RemoteConfig()
    {
        //aoa
        _ = GetValueRemoteAsync("ads_aoa_show", value => { Common.ads_aoa_show = value.BooleanValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("ads_aoa_resume_game_show", value => { Common.ads_aoa_resume_game_show = value.BooleanValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("ads_aoa_open_count", value => { Common.ads_aoa_open_count = (int)value.DoubleValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("ads_aoa_type", value => { Common.ads_aoa_type = (int)value.DoubleValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("ads_aoa_duration", value => { Common.ads_aoa_duration = (int)value.DoubleValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("ads_aoa_id_admob", value => { Common.ads_aoa_id_admob = value.StringValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("ads_aoa_id_max", value => { Common.ads_aoa_id_max = value.StringValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("ads_aoa_start_level", value => { Common.ads_aoa_start_level = (int)value.DoubleValue; checkConfigSuccess(); });

        //inter
        _ = GetValueRemoteAsync("ads_inter_shown_on_level_start", value => { Common.ads_inter_shown_on_level_start = value.BooleanValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("ads_inter_shown_on_level_end", value => { Common.ads_inter_shown_on_level_end = value.BooleanValue; checkConfigSuccess(); });
        //_ = GetValueRemoteAsync("ads_inter_ad_break_time", value => { Common.ads_inter_ad_break_time = (int)value.DoubleValue; checkConfigSuccess(); });
        //_ = GetValueRemoteAsync("ads_inter_ad_break_id_admob", value => { Common.ads_inter_ad_break_id_admob = value.StringValue; checkConfigSuccess(); });
        //_ = GetValueRemoteAsync("ads_inter_ad_break_id_max", value => { Common.ads_inter_ad_break_id_max = value.StringValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("ads_inter_start_level", value => { Common.ads_inter_start_level = (int)value.DoubleValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("ads_inter_level_range", value => { Common.ads_inter_level_range = (int)value.DoubleValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("ads_inter_type", value => { Common.ads_inter_type = (int)value.DoubleValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("ads_inter_duration", value => { Common.ads_inter_duration = (int)value.DoubleValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("ads_inter_first_show", value => { Common.ads_inter_first_show = (int)value.DoubleValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("ads_inter_id_admob", value => { Common.ads_inter_id_admob = value.StringValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("ads_inter_id_max", value => { Common.ads_inter_id_max = value.StringValue; checkConfigSuccess(); });

        //banner
        _ = GetValueRemoteAsync("ads_banner_show", value => { Common.ads_banner_show = value.BooleanValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("ads_banner_type", value => { Common.ads_banner_type = (int)value.DoubleValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("ads_banner_id_admob", value => { Common.ads_banner_id_admob = value.StringValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("ads_banner_id_max", value => { Common.ads_banner_id_max = value.StringValue; checkConfigSuccess(); });
        //_ = GetValueRemoteAsync("ads_banner_start_level", value => { Common.ads_banner_start_level = (int)value.DoubleValue; checkConfigSuccess(); });

        //reward
        _ = GetValueRemoteAsync("ads_reward_type", value => { Common.ads_reward_type = (int)value.DoubleValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("ads_reward_id_admob", value => { Common.ads_reward_id_admob = value.StringValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("ads_reward_id_max", value => { Common.ads_reward_id_max = value.StringValue; checkConfigSuccess(); });

        //mrec
        _ = GetValueRemoteAsync("ads_mrec_show", value => { Common.ads_mrec_show = value.BooleanValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("ads_mrec_id_max", value => { Common.ads_mrec_id_max = value.StringValue; checkConfigSuccess(); });

        //colllapse banner
        _ = GetValueRemoteAsync("ads_collapse_show", value => { Common.ads_collapse_show = value.BooleanValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("ads_collapse_duration", value => { Common.ads_collapse_duration = (int)value.DoubleValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("ads_collapse_id_admob", value => { Common.ads_collapse_id_admob = value.StringValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("ads_collapse_start_level", value => { Common.ads_collapse_start_level = (int)value.DoubleValue; checkConfigSuccess(); });

        //misc
        _ = GetValueRemoteAsync("rewarded_reset_capping_time", value => { Common.rewarded_reset_capping_time = value.BooleanValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("config_unlock_battlepass", value => { Common.config_unlock_battlepass = (int)value.DoubleValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("config_unlock_piggybank", value => { Common.config_unlock_piggybank = (int)value.DoubleValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("config_unlock_rank", value => { Common.config_unlock_rank = (int)value.DoubleValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("config_unlock_dailychallenge", value => { Common.config_unlock_dailychallenge = (int)value.DoubleValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("config_event_dates", value => { Common.config_event_dates = (int)value.DoubleValue; checkConfigSuccess(); });
        _ = GetValueRemoteAsync("config_tile_set", value => { Common.config_tile_set = (int)value.DoubleValue; checkConfigSuccess(); });
    }

    void InitializeFirebase()
    {
        Dictionary<string, object> defaults = new Dictionary<string, object>();
        FetchFireBase();
    }
    public void FetchFireBase()
    {
        FetchDataAsync();
    }
    public Task FetchDataAsync()
    {
        Debug.Log("Fetching data...");
        // FetchAsync only fetches new data if the current data is older than the provided
        // timespan.  Otherwise it assumes the data is "recent enough", and does nothing.
        // By default the timespan is 12 hours, and for production apps, this is a good
        // number.  For this example though, it's set to a timespan of zero, so that
        // changes in the console will always show up immediately.

        Task fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);
        return fetchTask.ContinueWithOnMainThread(FetchComplete);
    }
    async void FetchComplete(Task fetchTask)
    {
        if (fetchTask.IsCanceled)
        {
            Debug.Log("[Huynn3rdLib]==> Fetch canceled. <==");
        }
        else if (fetchTask.IsFaulted)
        {
            Debug.Log("[Huynn3rdLib]==> Fetch encountered an error <==");
        }
        else if (fetchTask.IsCompleted)
        {
            Debug.Log("[Huynn3rdLib]==> Fetch completed successfully! <==");
        }

        var info = FirebaseRemoteConfig.DefaultInstance.Info;

        Debug.Log("time");
        fetchTime = info.FetchTime.ToUniversalTime();

        switch (info.LastFetchStatus)
        {
            case LastFetchStatus.Success:
                await FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
                _keyConfigs = FirebaseRemoteConfig.DefaultInstance.AllValues.Keys.ToList();
                Debug.Log(String.Format("[Huynn3rdLib]==> Remote data loaded and ready (last fetch time {0}, count {1}).<==",
                    info.FetchTime, _keyConfigs.Count));
                if (_keyConfigs.Count == 0)
                    isRemoteFailed = true;
                _isFetchDone = true;
                break;
            case LastFetchStatus.Failure:
                switch (info.LastFetchFailureReason)
                {
                    case FetchFailureReason.Error:
                        Debug.Log("[Huynn3rdLib]==> Fetch failed for unknown reason <==");
                        isRemoteFailed = true;
                        break;
                    case FetchFailureReason.Throttled:
                        Debug.Log("[Huynn3rdLib]==> Fetch throttled until " + info.ThrottledEndTime + " <==");
                        isRemoteFailed = true;
                        break;
                }
                break;
            case LastFetchStatus.Pending:
                Debug.Log("[Huynn3rdLib]==> Latest Fetch call still pending. <==");
                break;
            default:
                Debug.Log("[Huynn3rdLib]==> default????? <==");
                break;
        }
    }

    public async Task GetValueRemoteAsync(string key, Action<ConfigValue> waitOnDone)
    {
        double countTime = 0;
        while (!_isFetchDone && countTime < 360000f)
        {
            countTime += 1000;
            await Task.Delay(1000);
        }

        if (countTime >= 360000f)
        {
            Debug.LogError(string.Format("[Huynn3rdLib]==>Fetch data {0} fail, becuz time out! Check your network please!<==", key));
            return;
        }

        if (!_keyConfigs.Contains(key))
        {
            Debug.LogError(string.Format("[Huynn3rdLib]==>Remote dont have key {0} !<==", key));
            checkConfigSuccess();
            return;
        }
        var obj = FirebaseRemoteConfig.DefaultInstance.GetValue(key);
        waitOnDone?.Invoke(obj);
    }

    /// <summary>
    /// fix totalConfig là số lần gọi GetValueRemoteAsync()
    /// </summary>
    readonly int totalConfig = 37;
    public int count_config = 0;
    public bool isRemoteConfig;
    public bool isRemoteFailed;
    void checkConfigSuccess()
    {
        count_config++;
        if (count_config >= totalConfig)
        {
            isRemoteConfig = true;
        }
    }

    public void LogLevelStart(int level)
    {
        if (_isFetchDone)
            FirebaseAnalytics.LogEvent("level_start",
                new Parameter("level_index", level)
            );
    }

    public void LogLevelComplete(int level, double timeSpent)
    {
        if (_isFetchDone)
            FirebaseAnalytics.LogEvent("level_complete",
                new Parameter("level_index", level),
                new Parameter("time_spent", timeSpent)
            );
    }

    public void LogLevelFail(int level, double timeSpent, int attempts)
    {
        if (_isFetchDone)
            FirebaseAnalytics.LogEvent("level_fail",
                new Parameter("level_index", level),
                new Parameter("time_spent", timeSpent),
                new Parameter("attempts", attempts)
            );
    }

    public void LogLevelQuit(int level, double timeSpent)
    {
        if (_isFetchDone)
            FirebaseAnalytics.LogEvent("level_quit",
                new Parameter("level_index", level),
                new Parameter("time_spent", timeSpent)
            );
    }
    public void LogDailyLogin(int login)
    {
        if (_isFetchDone)
            FirebaseAnalytics.LogEvent("daily_login",
                new Parameter("login", login)
            );
    }
}
