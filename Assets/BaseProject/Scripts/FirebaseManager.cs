using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using System;
using System.Collections.Generic;
#endif

// Local defaults, overridden by matching Firebase Remote Config parameters on Android.
public class Common
{
    //aoa
    public static bool ads_aoa_show = false;
    public static bool ads_aoa_resume_game_show = false;
    public static int ads_aoa_open_count = 1;
    public static int ads_aoa_type = 1;
    public static int ads_aoa_duration = 10;
    public static string ads_aoa_id_admob = GoogleTestAds.AppOpen;
    public static string ads_aoa_id_max = string.Empty;
    public static int ads_aoa_start_level = 1;

    //inter
    public static bool ads_inter_shown_on_level_start = false;
    public static bool ads_inter_shown_on_level_end = false;
    public static int ads_inter_ad_break_time = 60;
    public static string ads_inter_ad_break_id_admob = GoogleTestAds.Interstitial;
    public static string ads_inter_ad_break_id_max = string.Empty;
    public static int ads_inter_start_level = 1;
    public static int ads_inter_level_range = 1;
    public static int ads_inter_type = 1;
    public static int ads_inter_duration = 60;
    public static int ads_inter_first_show = 60;
    public static string ads_inter_id_admob = GoogleTestAds.Interstitial;
    public static string ads_inter_id_max = string.Empty;

    //banner
    public static bool ads_banner_show = false;
    public static int ads_banner_type = 1;
    public static string ads_banner_id_admob = GoogleTestAds.Banner;
    public static string ads_banner_id_max = string.Empty;
    //public static int ads_banner_start_level;
    public static bool ads_banner_fallback = false;

    //reward
    public static int ads_reward_type = 1;
    public static string ads_reward_id_admob = GoogleTestAds.Rewarded;
    public static string ads_reward_id_max = string.Empty;

    //mrec
    public static bool ads_mrec_show = false;
    public static string ads_mrec_id_max = string.Empty;

    //colllapse banner
    public static bool ads_collapse_show = false;
    public static int ads_collapse_duration = 60;
    public static string ads_collapse_id_admob = GoogleTestAds.Banner;
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
/// <summary>
/// Initializes Android Analytics independently of the legacy scene component.
/// Remote Config overrides Common defaults at startup. Messaging stays off.
/// </summary>
public class FirebaseManager : MonoBehaviour
{
    public static bool IsRemoteConfigReady { get; private set; } =
#if UNITY_ANDROID && !UNITY_EDITOR
        false;
#else
        true;
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
    private static FirebaseApp analyticsApp;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitializeAnalytics()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogWarning("Firebase Analytics initialization failed: " + task.Exception);
                IsRemoteConfigReady = true;
                return;
            }
            if (task.Result != DependencyStatus.Available)
            {
                Debug.LogWarning("Firebase Analytics dependencies unavailable: " + task.Result);
                IsRemoteConfigReady = true;
                return;
            }
            analyticsApp = FirebaseApp.DefaultInstance;
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            Debug.Log("Firebase Analytics initialized for " + analyticsApp.Options.ProjectId);
            InitializeRemoteConfig();
        });
    }

    private static void InitializeRemoteConfig()
    {
        var config = FirebaseRemoteConfig.DefaultInstance;
        var defaults = CreateRemoteDefaults();
        config.SetDefaultsAsync(defaults).ContinueWithOnMainThread(defaultTask =>
        {
            if (defaultTask.IsCanceled || defaultTask.IsFaulted)
            {
                Debug.LogWarning("Remote Config defaults failed; keeping local settings.");
                IsRemoteConfigReady = true;
                return;
            }
            // Previously activated values work offline. Do not change a running level.
            if (!GameConfig.isGameLoaded) ApplyRemoteConfig(config);
            config.FetchAsync(TimeSpan.FromHours(1)).ContinueWithOnMainThread(fetchTask =>
            {
                if (fetchTask.IsCanceled || fetchTask.IsFaulted || config.Info.LastFetchStatus != LastFetchStatus.Success)
                {
                    Debug.LogWarning("Remote Config fetch failed; keeping cached/local settings.");
                    IsRemoteConfigReady = true;
                    return;
                }
                config.ActivateAsync().ContinueWithOnMainThread(activateTask =>
                {
                    if (!activateTask.IsCanceled && !activateTask.IsFaulted)
                    {
                        if (!GameConfig.isGameLoaded)
                        {
                            ApplyRemoteConfig(config);
                            Debug.Log("Remote Config applied for Tile Sky.");
                        }
                        else Debug.Log("Remote Config cached for the next launch.");
                    }
                    else Debug.LogWarning("Remote Config activation failed; keeping cached/local settings.");
                    IsRemoteConfigReady = true;
                });
            });
        });
    }

    private static int ReadInt(FirebaseRemoteConfig config, string key, int fallback)
    {
        string text = config.GetValue(key).StringValue;
        if (int.TryParse(text, System.Globalization.NumberStyles.Integer,
            System.Globalization.CultureInfo.InvariantCulture, out int value) && value >= 0)
            return value;
        Debug.LogWarning("Invalid Remote Config integer: " + key + "; keeping existing value.");
        return fallback;
    }

    private static bool ReadBool(FirebaseRemoteConfig config, string key, bool fallback)
    {
        string text = config.GetValue(key).StringValue;
        if (bool.TryParse(text, out bool value)) return value;
        if (text == "1") return true;
        if (text == "0") return false;
        Debug.LogWarning("Invalid Remote Config boolean: " + key + "; keeping existing value.");
        return fallback;
    }

    private static Dictionary<string, object> CreateRemoteDefaults() => new Dictionary<string, object>
    {
        { nameof(Common.ads_aoa_show), Common.ads_aoa_show },
        { nameof(Common.ads_aoa_resume_game_show), Common.ads_aoa_resume_game_show },
        { nameof(Common.ads_aoa_open_count), Common.ads_aoa_open_count },
        { nameof(Common.ads_aoa_type), Common.ads_aoa_type },
        { nameof(Common.ads_aoa_duration), Common.ads_aoa_duration },
        { nameof(Common.ads_aoa_id_admob), Common.ads_aoa_id_admob },
        { nameof(Common.ads_aoa_id_max), Common.ads_aoa_id_max },
        { nameof(Common.ads_aoa_start_level), Common.ads_aoa_start_level },
        { nameof(Common.ads_inter_shown_on_level_start), Common.ads_inter_shown_on_level_start },
        { nameof(Common.ads_inter_shown_on_level_end), Common.ads_inter_shown_on_level_end },
        { nameof(Common.ads_inter_ad_break_time), Common.ads_inter_ad_break_time },
        { nameof(Common.ads_inter_ad_break_id_admob), Common.ads_inter_ad_break_id_admob },
        { nameof(Common.ads_inter_ad_break_id_max), Common.ads_inter_ad_break_id_max },
        { nameof(Common.ads_inter_start_level), Common.ads_inter_start_level },
        { nameof(Common.ads_inter_level_range), Common.ads_inter_level_range },
        { nameof(Common.ads_inter_type), Common.ads_inter_type },
        { nameof(Common.ads_inter_duration), Common.ads_inter_duration },
        { nameof(Common.ads_inter_first_show), Common.ads_inter_first_show },
        { nameof(Common.ads_inter_id_admob), Common.ads_inter_id_admob },
        { nameof(Common.ads_inter_id_max), Common.ads_inter_id_max },
        { nameof(Common.ads_banner_show), Common.ads_banner_show },
        { nameof(Common.ads_banner_type), Common.ads_banner_type },
        { nameof(Common.ads_banner_id_admob), Common.ads_banner_id_admob },
        { nameof(Common.ads_banner_id_max), Common.ads_banner_id_max },
        { nameof(Common.ads_banner_fallback), Common.ads_banner_fallback },
        { nameof(Common.ads_reward_type), Common.ads_reward_type },
        { nameof(Common.ads_reward_id_admob), Common.ads_reward_id_admob },
        { nameof(Common.ads_reward_id_max), Common.ads_reward_id_max },
        { nameof(Common.ads_mrec_show), Common.ads_mrec_show },
        { nameof(Common.ads_mrec_id_max), Common.ads_mrec_id_max },
        { nameof(Common.ads_collapse_show), Common.ads_collapse_show },
        { nameof(Common.ads_collapse_duration), Common.ads_collapse_duration },
        { nameof(Common.ads_collapse_id_admob), Common.ads_collapse_id_admob },
        { nameof(Common.ads_collapse_start_level), Common.ads_collapse_start_level },
        { nameof(Common.rewarded_reset_capping_time), Common.rewarded_reset_capping_time },
        { nameof(Common.config_loading_time), Common.config_loading_time },
        { nameof(Common.config_tile_shadow), Common.config_tile_shadow },
        { nameof(Common.config_unlock_battlepass), Common.config_unlock_battlepass },
        { nameof(Common.config_unlock_piggybank), Common.config_unlock_piggybank },
        { nameof(Common.config_unlock_rank), Common.config_unlock_rank },
        { nameof(Common.config_unlock_dailychallenge), Common.config_unlock_dailychallenge },
        { nameof(Common.config_event_dates), Common.config_event_dates },
        { nameof(Common.config_tile_set), Common.config_tile_set },
    };

    private static void ApplyRemoteConfig(FirebaseRemoteConfig config)
    {
        Common.ads_aoa_show = ReadBool(config, nameof(Common.ads_aoa_show), Common.ads_aoa_show);
        Common.ads_aoa_resume_game_show = ReadBool(config, nameof(Common.ads_aoa_resume_game_show), Common.ads_aoa_resume_game_show);
        Common.ads_aoa_open_count = ReadInt(config, nameof(Common.ads_aoa_open_count), Common.ads_aoa_open_count);
        Common.ads_aoa_type = ReadInt(config, nameof(Common.ads_aoa_type), Common.ads_aoa_type);
        Common.ads_aoa_duration = ReadInt(config, nameof(Common.ads_aoa_duration), Common.ads_aoa_duration);
        Common.ads_aoa_id_admob = config.GetValue(nameof(Common.ads_aoa_id_admob)).StringValue;
        Common.ads_aoa_id_max = config.GetValue(nameof(Common.ads_aoa_id_max)).StringValue;
        Common.ads_aoa_start_level = ReadInt(config, nameof(Common.ads_aoa_start_level), Common.ads_aoa_start_level);
        Common.ads_inter_shown_on_level_start = ReadBool(config, nameof(Common.ads_inter_shown_on_level_start), Common.ads_inter_shown_on_level_start);
        Common.ads_inter_shown_on_level_end = ReadBool(config, nameof(Common.ads_inter_shown_on_level_end), Common.ads_inter_shown_on_level_end);
        Common.ads_inter_ad_break_time = ReadInt(config, nameof(Common.ads_inter_ad_break_time), Common.ads_inter_ad_break_time);
        Common.ads_inter_ad_break_id_admob = config.GetValue(nameof(Common.ads_inter_ad_break_id_admob)).StringValue;
        Common.ads_inter_ad_break_id_max = config.GetValue(nameof(Common.ads_inter_ad_break_id_max)).StringValue;
        Common.ads_inter_start_level = ReadInt(config, nameof(Common.ads_inter_start_level), Common.ads_inter_start_level);
        Common.ads_inter_level_range = ReadInt(config, nameof(Common.ads_inter_level_range), Common.ads_inter_level_range);
        Common.ads_inter_type = ReadInt(config, nameof(Common.ads_inter_type), Common.ads_inter_type);
        Common.ads_inter_duration = ReadInt(config, nameof(Common.ads_inter_duration), Common.ads_inter_duration);
        Common.ads_inter_first_show = ReadInt(config, nameof(Common.ads_inter_first_show), Common.ads_inter_first_show);
        Common.ads_inter_id_admob = config.GetValue(nameof(Common.ads_inter_id_admob)).StringValue;
        Common.ads_inter_id_max = config.GetValue(nameof(Common.ads_inter_id_max)).StringValue;
        Common.ads_banner_show = ReadBool(config, nameof(Common.ads_banner_show), Common.ads_banner_show);
        Common.ads_banner_type = ReadInt(config, nameof(Common.ads_banner_type), Common.ads_banner_type);
        Common.ads_banner_id_admob = config.GetValue(nameof(Common.ads_banner_id_admob)).StringValue;
        Common.ads_banner_id_max = config.GetValue(nameof(Common.ads_banner_id_max)).StringValue;
        Common.ads_banner_fallback = ReadBool(config, nameof(Common.ads_banner_fallback), Common.ads_banner_fallback);
        Common.ads_reward_type = ReadInt(config, nameof(Common.ads_reward_type), Common.ads_reward_type);
        Common.ads_reward_id_admob = config.GetValue(nameof(Common.ads_reward_id_admob)).StringValue;
        Common.ads_reward_id_max = config.GetValue(nameof(Common.ads_reward_id_max)).StringValue;
        Common.ads_mrec_show = ReadBool(config, nameof(Common.ads_mrec_show), Common.ads_mrec_show);
        Common.ads_mrec_id_max = config.GetValue(nameof(Common.ads_mrec_id_max)).StringValue;
        Common.ads_collapse_show = ReadBool(config, nameof(Common.ads_collapse_show), Common.ads_collapse_show);
        Common.ads_collapse_duration = ReadInt(config, nameof(Common.ads_collapse_duration), Common.ads_collapse_duration);
        Common.ads_collapse_id_admob = config.GetValue(nameof(Common.ads_collapse_id_admob)).StringValue;
        Common.ads_collapse_start_level = ReadInt(config, nameof(Common.ads_collapse_start_level), Common.ads_collapse_start_level);
        Common.rewarded_reset_capping_time = ReadBool(config, nameof(Common.rewarded_reset_capping_time), Common.rewarded_reset_capping_time);
        Common.config_loading_time = ReadInt(config, nameof(Common.config_loading_time), Common.config_loading_time);
        Common.config_tile_shadow = ReadInt(config, nameof(Common.config_tile_shadow), Common.config_tile_shadow);
        Common.config_unlock_battlepass = ReadInt(config, nameof(Common.config_unlock_battlepass), Common.config_unlock_battlepass);
        Common.config_unlock_piggybank = ReadInt(config, nameof(Common.config_unlock_piggybank), Common.config_unlock_piggybank);
        Common.config_unlock_rank = ReadInt(config, nameof(Common.config_unlock_rank), Common.config_unlock_rank);
        Common.config_unlock_dailychallenge = ReadInt(config, nameof(Common.config_unlock_dailychallenge), Common.config_unlock_dailychallenge);
        Common.config_event_dates = ReadInt(config, nameof(Common.config_event_dates), Common.config_event_dates);
        Common.config_tile_set = ReadInt(config, nameof(Common.config_tile_set), Common.config_tile_set);
    }
#endif
}
