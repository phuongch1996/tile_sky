# Tile Sky Firebase Remote Config

Android builds initialize Remote Config after Firebase dependencies are available. In Firebase Console, publish parameters using the exact names below. Missing keys retain the local defaults. Boolean values accept true/false or 1/0; integers must be nonnegative 32-bit values. Invalid values retain the existing setting.

The loading screen waits at most 3 additional seconds. Activated cache is available offline. Newly fetched values apply before gameplay starts; late results are activated and cached for the next app launch. Fetch cache duration: 1 hour. No real-time listener. Editor and iOS retain local settings.

| Parameter | Type | Local default |
| --- | --- | --- |
| ads_aoa_show | bool | false |
| ads_aoa_resume_game_show | bool | false |
| ads_aoa_open_count | int | 1 |
| ads_aoa_type | int | 1 |
| ads_aoa_duration | int | 10 |
| ads_aoa_id_admob | string | GoogleTestAds.AppOpen |
| ads_aoa_id_max | string | string.Empty |
| ads_aoa_start_level | int | 1 |
| ads_inter_shown_on_level_start | bool | false |
| ads_inter_shown_on_level_end | bool | false |
| ads_inter_ad_break_time | int | 60 |
| ads_inter_ad_break_id_admob | string | GoogleTestAds.Interstitial |
| ads_inter_ad_break_id_max | string | string.Empty |
| ads_inter_start_level | int | 1 |
| ads_inter_level_range | int | 1 |
| ads_inter_type | int | 1 |
| ads_inter_duration | int | 60 |
| ads_inter_first_show | int | 60 |
| ads_inter_id_admob | string | GoogleTestAds.Interstitial |
| ads_inter_id_max | string | string.Empty |
| ads_banner_show | bool | false |
| ads_banner_type | int | 1 |
| ads_banner_id_admob | string | GoogleTestAds.Banner |
| ads_banner_id_max | string | string.Empty |
| ads_banner_fallback | bool | false |
| ads_reward_type | int | 1 |
| ads_reward_id_admob | string | GoogleTestAds.Rewarded |
| ads_reward_id_max | string | string.Empty |
| ads_mrec_show | bool | false |
| ads_mrec_id_max | string | string.Empty |
| ads_collapse_show | bool | false |
| ads_collapse_duration | int | 60 |
| ads_collapse_id_admob | string | GoogleTestAds.Banner |
| ads_collapse_start_level | int | 1 |
| rewarded_reset_capping_time | bool | false |
| config_loading_time | int | 2 |
| config_tile_shadow | int | 0 |
| config_unlock_battlepass | int | 31 |
| config_unlock_piggybank | int | 22 |
| config_unlock_rank | int | 26 |
| config_unlock_dailychallenge | int | 51 |
| config_event_dates | int | 7 |
| config_tile_set | int | 0 |

Device validation: publish a distinctive config_unlock_rank value, install an Android build, and inspect logcat for "Remote Config applied for Tile Sky." Verify the unlock threshold on Home. If the download is late, reopen the app. Then verify airplane-mode startup still works with cached settings. No live Firebase fetch or Android device test has been performed by the compile checks.