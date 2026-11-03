using UnityEngine;

// Local gameplay configuration. No values are fetched from the previous backend.
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
/// Inert compatibility component for existing scenes. No Firebase initialization,
/// Remote Config, Analytics or Messaging runs. Gameplay uses Common local defaults.
/// Keep the script GUID so existing scenes do not acquire a missing script.
/// </summary>
public class FirebaseManager : MonoBehaviour
{
}
